using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Profiling;

namespace GameFramework {
    /// <summary>
    /// タスク実行用のランナー
    /// </summary>
    public class TaskRunner : ILateUpdatableTask, IFixedUpdatableTask, IDisposable {
        /// <summary>Update用のSamplerIndex</summary>
        private const int UpdateSamplerIndex = 0;
        /// <summary>LateUpdate用のSamplerIndex</summary>
        private const int LateUpdateSamplerIndex = 1;
        /// <summary>FixedUpdate用のSamplerIndex</summary>
        private const int FixedUpdateSamplerIndex = 2;

        /// <summary>
        /// タスクの状態
        /// </summary>
        private enum TaskStatus {
            Active,
            Killed,
        }

        /// <summary>
        /// タスク情報
        /// </summary>
        private class TaskInfo {
            public TaskGroupInfo GroupInfo;
            public TaskStatus Status;
            public ITask Task;
            public CustomSampler[] Samplers;
        }

        /// <summary>
        /// タスクグループ情報
        /// </summary>
        private class TaskGroupInfo {
            public readonly List<TaskInfo> TaskInfos = new();
            public CustomSampler[] Samplers;
        }

        /// <summary>
        /// タスク登録予定情報
        /// </summary>
        private class ScheduledTaskInfo {
            public int ExecutionOrder;
            public ITask Task;
        }

        // Order毎のタスクグループ情報
        private readonly SortedDictionary<int, TaskGroupInfo> _taskGroupInfos = new();
        // TaskInfo検索用
        private readonly Dictionary<ITask, TaskInfo> _taskInfos = new();
        // 登録待ちタスク情報
        private readonly Dictionary<ITask, ScheduledTaskInfo> _scheduledTaskInfos = new();

        /// <summary>タスクの有効状態</summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            foreach (var groupInfo in _taskGroupInfos.Values) {
                for (var i = groupInfo.TaskInfos.Count - 1; i >= 0; i--) {
                    var taskInfo = groupInfo.TaskInfos[i];
                    if (taskInfo.Status != TaskStatus.Killed) {
                        taskInfo.Status = TaskStatus.Killed;

                        // 登録解除通知
                        if (taskInfo.Task is ITaskEventHandler handler) {
                            handler.OnUnregistered(this);
                        }
                    }
                }
            }

            // 各種リストリセット
            _taskGroupInfos.Clear();
            _taskInfos.Clear();
            _scheduledTaskInfos.Clear();
        }

        /// <summary>
        /// タスク登録
        /// </summary>
        /// <param name="task">追加対象のTask</param>
        /// <param name="executionOrder">Taskの実行優先度</param>
        public void Register(ITask task, int executionOrder = 0) {
            if (task == null) {
                Debug.LogError("Registered task is null.");
                return;
            }

            // 既に登録済み
            if (_scheduledTaskInfos.ContainsKey(task) || _taskInfos.ContainsKey(task)) {
                Debug.LogError("Already registered task.");
                return;
            }

            // タスク登録情報に追加
            _scheduledTaskInfos.Add(task, new ScheduledTaskInfo {
                ExecutionOrder = executionOrder,
                Task = task,
            });
        }

        /// <summary>
        /// タスク登録
        /// </summary>
        /// <param name="task">追加対象のTask</param>
        /// <param name="executionOrder">Taskの実行優先度</param>
        /// <typeparam name="T">実行優先度を指定するenum型</typeparam>
        public void Register<T>(ITask task, T executionOrder)
            where T : Enum {
            Register(task, Convert.ToInt32(executionOrder));
        }

        /// <summary>
        /// タスク登録解除
        /// </summary>
        /// <param name="task">除外対象のTask</param>
        public void Unregister(ITask task) {
            if (_taskInfos.TryGetValue(task, out var taskInfo)) {
                if (taskInfo.Status != TaskStatus.Killed) {
                    // タスクのステータスをKilledに変更
                    taskInfo.Status = TaskStatus.Killed;

                    // 除外通知
                    if (task is ITaskEventHandler handler) {
                        handler.OnUnregistered(this);
                    }
                }
            }
            else {
                // 登録予約から除外
                _scheduledTaskInfos.Remove(task);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            UpdateInternal(task => { task.Update(); }, UpdateSamplerIndex);
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            UpdateInternal(task => {
                if (task is ILateUpdatableTask lateUpdatableTask) {
                    lateUpdatableTask.LateUpdate();
                }
            }, LateUpdateSamplerIndex);
        }

        /// <summary>
        /// 固定更新処理
        /// </summary>
        public void FixedUpdate() {
            UpdateInternal(task => {
                if (task is IFixedUpdatableTask fixedUpdatableTask) {
                    fixedUpdatableTask.FixedUpdate();
                }
            }, FixedUpdateSamplerIndex);
        }

        /// <summary>
        /// 内部用更新処理
        /// </summary>
        private void UpdateInternal([NotNull] Action<ITask> onUpdate, int samplerIndex) {
            // Taskリストのリフレッシュ
            RefreshTaskInfos();

            foreach (var groupInfo in _taskGroupInfos.Values) {
                groupInfo.Samplers[samplerIndex].Begin();

                for (var i = 0; i < groupInfo.TaskInfos.Count; i++) {
                    var taskInfo = groupInfo.TaskInfos[i];

                    // 無効なタスクは処理しない
                    if (taskInfo.Status != TaskStatus.Active || !taskInfo.Task.IsActive) {
                        continue;
                    }

                    // タスク更新
                    try {
                        taskInfo.Samplers[samplerIndex].Begin();
                        onUpdate(taskInfo.Task);
                    }
                    catch (Exception exception) {
                        Debug.LogException(exception);
                    }
                    finally {
                        taskInfo.Samplers[samplerIndex].End();
                    }
                }

                groupInfo.Samplers[samplerIndex].End();
            }
        }

        /// <summary>
        /// タスク情報のリフレッシュ
        /// </summary>
        private void RefreshTaskInfos() {
            // タスク登録を実行
            foreach (var info in _scheduledTaskInfos.Values) {
                RegisterInternal(info);
            }

            _scheduledTaskInfos.Clear();

            // タスク登録解除を実行
            foreach (var groupInfo in _taskGroupInfos.Values) {
                for (var i = groupInfo.TaskInfos.Count - 1; i >= 0; i--) {
                    var taskInfo = groupInfo.TaskInfos[i];

                    // KillされたTaskを除外
                    if (taskInfo.Status == TaskStatus.Killed) {
                        groupInfo.TaskInfos.RemoveAt(i);
                        _taskInfos.Remove(taskInfo.Task);
                    }
                }
            }
        }

        /// <summary>
        /// タスク登録処理(内部用)
        /// </summary>
        private void RegisterInternal(ScheduledTaskInfo scheduledTaskInfo) {
            var executionOrder = scheduledTaskInfo.ExecutionOrder;
            var task = scheduledTaskInfo.Task;

            // TaskGroupInfoの取得/生成
            if (!_taskGroupInfos.TryGetValue(executionOrder, out var groupInfo)) {
                groupInfo = new TaskGroupInfo();
                groupInfo.Samplers = new[] {
                    CustomSampler.Create($"Task Update()[{executionOrder}]"),
                    CustomSampler.Create($"Task LateUpdate()[{executionOrder}]"),
                    CustomSampler.Create($"Task FixedUpdate()[{executionOrder}]"),
                };
                _taskGroupInfos[executionOrder] = groupInfo;
            }

            // 既に存在しているTaskの場合、ステータスを更新
            if (_taskInfos.TryGetValue(task, out var info)) {
                // Groupが変わっていたら変更
                if (info.GroupInfo != groupInfo) {
                    info.GroupInfo.TaskInfos.Remove(info);
                    groupInfo.TaskInfos.Add(info);
                    info.GroupInfo = groupInfo;
                }

                // Killされていた場合、Activeに戻す
                if (info.Status == TaskStatus.Killed) {
                    info.Status = TaskStatus.Active;

                    // 登録通知
                    if (task is ITaskEventHandler handler) {
                        handler.OnRegistered(this);
                    }
                }
            }
            // 存在していない場合、タスク情報を追加
            else {
                var taskType = task.GetType();
                info = new TaskInfo {
                    Status = TaskStatus.Active,
                    Task = task,
                    Samplers = new[] {
                        CustomSampler.Create($"{taskType}.Update()"),
                        CustomSampler.Create($"{taskType}.LateUpdate()"),
                        CustomSampler.Create($"{taskType}.FixedLateUpdate()")
                    }
                };
                groupInfo.TaskInfos.Add(info);
                _taskInfos[task] = info;

                // 登録通知
                if (task is ITaskEventHandler handler) {
                    handler.OnRegistered(this);
                }
            }
        }
    }
}