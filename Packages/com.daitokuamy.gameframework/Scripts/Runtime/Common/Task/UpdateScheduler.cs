using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Profiling;

namespace GameFramework {
    /// <summary>
    /// 更新処理実行用クラス
    /// </summary>
    public class UpdateScheduler : IUpdatable, ILateUpdatable, IFixedUpdatable, IDisposable {
        /// <summary>
        /// Updatableの状態
        /// </summary>
        private enum UpdatableState {
            Active,
            Killed,
        }

        /// <summary>
        /// 更新先情報
        /// </summary>
        private sealed class UpdatableInfo<TUpdatable>
            where TUpdatable : IUpdatableBase {
            public UpdatableGroupInfo<TUpdatable> GroupInfo;
            public UpdatableState Status;
            public TUpdatable Updatable;
            public CustomSampler Sampler;
        }

        /// <summary>
        /// UpdatableGroup情報
        /// </summary>
        private sealed class UpdatableGroupInfo<TUpdatable>
            where TUpdatable : IUpdatableBase {
            public readonly List<UpdatableInfo<TUpdatable>> UpdatableInfos = new();
            public CustomSampler Sampler;
        }

        /// <summary>
        /// Updatable登録予定情報
        /// </summary>
        private class ScheduledUpdatableInfo<TUpdatable>
            where TUpdatable : IUpdatableBase {
            public int ExecutionOrder;
            public TUpdatable Updatable;
        }

        private readonly SortedDictionary<int, UpdatableGroupInfo<IUpdatable>> _updatableGroupInfos = new();
        private readonly SortedDictionary<int, UpdatableGroupInfo<ILateUpdatable>> _lateUpdatableGroupInfos = new();
        private readonly SortedDictionary<int, UpdatableGroupInfo<IFixedUpdatable>> _fixedUpdatableGroupInfos = new();

        private readonly Dictionary<IUpdatable, UpdatableInfo<IUpdatable>> _updatableInfos = new();
        private readonly Dictionary<ILateUpdatable, UpdatableInfo<ILateUpdatable>> _lateUpdatableInfos = new();
        private readonly Dictionary<IFixedUpdatable, UpdatableInfo<IFixedUpdatable>> _fixedUpdatableInfos = new();

        private readonly Dictionary<IUpdatable, ScheduledUpdatableInfo<IUpdatable>> _scheduledUpdatableInfos = new();
        private readonly Dictionary<ILateUpdatable, ScheduledUpdatableInfo<ILateUpdatable>> _scheduledLateUpdatableInfos = new();
        private readonly Dictionary<IFixedUpdatable, ScheduledUpdatableInfo<IFixedUpdatable>> _scheduledFixedUpdatableInfos = new();

        /// <summary>更新の有効状態</summary>
        public bool IsActive { get; set; }

        /// <inheritdoc/>
        public void Dispose() {
            foreach (var groupInfo in _updatableGroupInfos.Values) {
                ClearGroupInfo(groupInfo);
            }

            foreach (var groupInfo in _lateUpdatableGroupInfos.Values) {
                ClearGroupInfo(groupInfo);
            }

            foreach (var groupInfo in _fixedUpdatableGroupInfos.Values) {
                ClearGroupInfo(groupInfo);
            }

            // 各種リストリセット
            _updatableGroupInfos.Clear();
            _lateUpdatableGroupInfos.Clear();
            _fixedUpdatableGroupInfos.Clear();

            _updatableInfos.Clear();
            _lateUpdatableInfos.Clear();
            _fixedUpdatableInfos.Clear();

            _scheduledUpdatableInfos.Clear();
            _scheduledLateUpdatableInfos.Clear();
            _scheduledFixedUpdatableInfos.Clear();
        }

        /// <inheritdoc/>
        void IUpdatable.Update() {
            Update();
        }

        /// <inheritdoc/>
        void ILateUpdatable.Update() {
            LateUpdate();
        }

        /// <inheritdoc/>
        void IFixedUpdatable.Update() {
            FixedUpdate();
        }

        /// <summary>
        /// 登録
        /// </summary>
        /// <param name="updatable">追加対象のUpdatable</param>
        /// <param name="executionOrder">Updatableの実行優先度</param>
        /// <typeparam name="TOrder">実行優先度を指定するenum型</typeparam>
        public void RegisterUpdatable<TOrder>(IUpdatable updatable, TOrder executionOrder)
            where TOrder : Enum {
            RegisterInternal(_scheduledUpdatableInfos, _updatableInfos, updatable, Convert.ToInt32(executionOrder));
        }

        /// <summary>
        /// 登録
        /// </summary>
        /// <param name="updatable">追加対象のUpdatable</param>
        /// <param name="executionOrder">Updatableの実行優先度</param>
        public void RegisterUpdatable(IUpdatable updatable, int executionOrder) {
            RegisterInternal(_scheduledUpdatableInfos, _updatableInfos, updatable, executionOrder);
        }

        /// <summary>
        /// 登録
        /// </summary>
        /// <param name="updatable">追加対象のLateUpdatable</param>
        /// <param name="executionOrder">LateUpdatableの実行優先度</param>
        /// <typeparam name="TOrder">実行優先度を指定するenum型</typeparam>
        public void RegisterLateUpdatable<TOrder>(ILateUpdatable updatable, TOrder executionOrder)
            where TOrder : Enum {
            RegisterInternal(_scheduledLateUpdatableInfos, _lateUpdatableInfos, updatable, Convert.ToInt32(executionOrder));
        }

        /// <summary>
        /// 登録
        /// </summary>
        /// <param name="updatable">追加対象のLateUpdatable</param>
        /// <param name="executionOrder">LateUpdatableの実行優先度</param>
        public void RegisterLateUpdatable(ILateUpdatable updatable, int executionOrder) {
            RegisterInternal(_scheduledLateUpdatableInfos, _lateUpdatableInfos, updatable, executionOrder);
        }

        /// <summary>
        /// 登録
        /// </summary>
        /// <param name="updatable">追加対象のFixedUpdatable</param>
        /// <param name="executionOrder">FixedUpdatableの実行優先度</param>
        /// <typeparam name="TOrder">実行優先度を指定するenum型</typeparam>
        public void RegisterFixedUpdatable<TOrder>(IFixedUpdatable updatable, TOrder executionOrder)
            where TOrder : Enum {
            RegisterInternal(_scheduledFixedUpdatableInfos, _fixedUpdatableInfos, updatable, Convert.ToInt32(executionOrder));
        }

        /// <summary>
        /// 登録
        /// </summary>
        /// <param name="updatable">追加対象のFixedUpdatable</param>
        /// <param name="executionOrder">FixedUpdatableの実行優先度</param>
        public void RegisterFixedUpdatable(IFixedUpdatable updatable, int executionOrder) {
            RegisterInternal(_scheduledFixedUpdatableInfos, _fixedUpdatableInfos, updatable, executionOrder);
        }

        /// <summary>
        /// 登録解除
        /// </summary>
        /// <param name="updatable">除外対象のUpdatable</param>
        public void UnregisterUpdatable(IUpdatable updatable) {
            UnregisterInternal(_updatableInfos, _scheduledUpdatableInfos, updatable);
        }

        /// <summary>
        /// 登録解除
        /// </summary>
        /// <param name="lateUpdatable">除外対象のLateUpdatable</param>
        public void UnregisterLateUpdatable(ILateUpdatable lateUpdatable) {
            UnregisterInternal(_lateUpdatableInfos, _scheduledLateUpdatableInfos, lateUpdatable);
        }

        /// <summary>
        /// 登録解除
        /// </summary>
        /// <param name="fixedUpdatable">除外対象のFixedUpdatable</param>
        public void UnregisterFixedUpdatable(IFixedUpdatable fixedUpdatable) {
            UnregisterInternal(_fixedUpdatableInfos, _scheduledFixedUpdatableInfos, fixedUpdatable);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="scheduledUpdatableInfos">登録予約情報リスト</param>
        /// <param name="updatableInfos">登録情報リスト</param>
        /// <param name="updatable">追加対象のUpdatable</param>
        /// <param name="executionOrder">Updatableの実行優先度</param>
        private void RegisterInternal<TUpdatable>(
            Dictionary<TUpdatable, ScheduledUpdatableInfo<TUpdatable>> scheduledUpdatableInfos,
            Dictionary<TUpdatable, UpdatableInfo<TUpdatable>> updatableInfos,
            TUpdatable updatable, int executionOrder = 0)
            where TUpdatable : IUpdatableBase {
            if (updatable == null) {
                throw new ArgumentNullException(nameof(updatable));
            }

            // 既に登録済み
            if (scheduledUpdatableInfos.ContainsKey(updatable) || updatableInfos.ContainsKey(updatable)) {
                throw new ArgumentException($"Already registered updatable. ({updatable.GetType().Name})");
            }

            // 登録予約
            scheduledUpdatableInfos.Add(updatable, new ScheduledUpdatableInfo<TUpdatable> { ExecutionOrder = executionOrder, Updatable = updatable });
        }

        /// <summary>
        /// 登録解除処理
        /// </summary>
        private void UnregisterInternal<TUpdatable>(
            Dictionary<TUpdatable, UpdatableInfo<TUpdatable>> updatableInfos,
            Dictionary<TUpdatable, ScheduledUpdatableInfo<TUpdatable>> scheduledUpdatableInfos,
            TUpdatable target)
            where TUpdatable : IUpdatableBase {
            if (updatableInfos.TryGetValue(target, out var updatableInfo)) {
                if (updatableInfo.Status != UpdatableState.Killed) {
                    // ステータスをKilledに変更
                    updatableInfo.Status = UpdatableState.Killed;

                    // 除外通知
                    if (target is IUpdatableEventHandlerBase<TUpdatable> handler) {
                        handler.OnUnregistered(this);
                    }
                }
            }
            else {
                // 登録予約から除外
                scheduledUpdatableInfos.Remove(target);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            UpdateInternal(nameof(IUpdatable), _scheduledUpdatableInfos, _updatableGroupInfos, _updatableInfos, updatable => { updatable.Update(); });
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            UpdateInternal(nameof(ILateUpdatable), _scheduledLateUpdatableInfos,_lateUpdatableGroupInfos, _lateUpdatableInfos, updatable => { updatable.Update(); });
        }

        /// <summary>
        /// 固定更新処理
        /// </summary>
        public void FixedUpdate() {
            UpdateInternal(nameof(IFixedUpdatable), _scheduledFixedUpdatableInfos, _fixedUpdatableGroupInfos, _fixedUpdatableInfos, updatable => { updatable.Update(); });
        }

        /// <summary>
        /// 内部用更新処理
        /// </summary>
        private void UpdateInternal<TUpdatable>(
            string groupLabel,
            Dictionary<TUpdatable, ScheduledUpdatableInfo<TUpdatable>> scheduledUpdatableInfos,
            SortedDictionary<int, UpdatableGroupInfo<TUpdatable>> updatableGroupInfos,
            Dictionary<TUpdatable, UpdatableInfo<TUpdatable>> updatableInfos,
            [NotNull] Action<TUpdatable> onUpdate)
            where TUpdatable : IUpdatableBase {
            // リストのリフレッシュ
            RefreshUpdatableInfos(groupLabel, scheduledUpdatableInfos, updatableGroupInfos, updatableInfos);

            foreach (var groupInfo in updatableGroupInfos.Values) {
                groupInfo.Sampler.Begin();

                for (var i = 0; i < groupInfo.UpdatableInfos.Count; i++) {
                    var updatableInfo = groupInfo.UpdatableInfos[i];

                    // 無効な物は処理しない
                    if (updatableInfo.Status != UpdatableState.Active || !updatableInfo.Updatable.IsActive) {
                        continue;
                    }

                    // 更新
                    try {
                        updatableInfo.Sampler.Begin();
                        onUpdate(updatableInfo.Updatable);
                    }
                    catch (Exception exception) {
                        Debug.LogException(exception);
                    }
                    finally {
                        updatableInfo.Sampler.End();
                    }
                }

                groupInfo.Sampler.End();
            }
        }

        /// <summary>
        /// Updatable情報のリフレッシュ
        /// </summary>
        private void RefreshUpdatableInfos<TUpdatable>(
            string groupLabel,
            Dictionary<TUpdatable, ScheduledUpdatableInfo<TUpdatable>> scheduledUpdatableInfos,
            SortedDictionary<int, UpdatableGroupInfo<TUpdatable>> updatableGroupInfos,
            Dictionary<TUpdatable, UpdatableInfo<TUpdatable>> updatableInfos)
            where TUpdatable : IUpdatableBase {
            // 予約登録を実行
            foreach (var info in scheduledUpdatableInfos.Values) {
                RegisterComplete(groupLabel, info, updatableGroupInfos, updatableInfos);
            }

            scheduledUpdatableInfos.Clear();

            // 登録解除を実行
            foreach (var groupInfo in _updatableGroupInfos.Values) {
                for (var i = groupInfo.UpdatableInfos.Count - 1; i >= 0; i--) {
                    var updatableInfo = groupInfo.UpdatableInfos[i];

                    // KillされたUpdatableを除外
                    if (updatableInfo.Status == UpdatableState.Killed) {
                        groupInfo.UpdatableInfos.RemoveAt(i);
                        _updatableInfos.Remove(updatableInfo.Updatable);
                    }
                }
            }
        }

        /// <summary>
        /// 登録完了処理
        /// </summary>
        private void RegisterComplete<TUpdatable>(
            string groupLabel,
            ScheduledUpdatableInfo<TUpdatable> scheduledUpdatableInfo,
            SortedDictionary<int, UpdatableGroupInfo<TUpdatable>> updatableGroupInfos,
            Dictionary<TUpdatable, UpdatableInfo<TUpdatable>> updatableInfos)
            where TUpdatable : IUpdatableBase {
            var executionOrder = scheduledUpdatableInfo.ExecutionOrder;
            var updatable = scheduledUpdatableInfo.Updatable;

            // GroupInfoの取得/生成
            if (!updatableGroupInfos.TryGetValue(executionOrder, out var groupInfo)) {
                groupInfo = new UpdatableGroupInfo<TUpdatable> {
                    Sampler = CustomSampler.Create($"{groupLabel}.Update()[{executionOrder}]")
                };
                updatableGroupInfos[executionOrder] = groupInfo;
            }

            // 既に存在しているUpdatableの場合、ステータスを更新
            if (updatableInfos.TryGetValue(updatable, out var info)) {
                // Groupが変わっていたら変更
                if (info.GroupInfo != groupInfo) {
                    info.GroupInfo.UpdatableInfos.Remove(info);
                    groupInfo.UpdatableInfos.Add(info);
                    info.GroupInfo = groupInfo;
                }

                // Killされていた場合、Activeに戻す
                if (info.Status == UpdatableState.Killed) {
                    info.Status = UpdatableState.Active;

                    // 登録通知
                    if (updatable is IUpdatableEventHandlerBase<TUpdatable> handler) {
                        handler.OnRegistered(this);
                    }
                }
            }
            // 存在していない場合、Updatable情報を追加
            else {
                var updatableType = updatable.GetType();
                info = new UpdatableInfo<TUpdatable> {
                    Status = UpdatableState.Active,
                    Updatable = updatable,
                    Sampler = CustomSampler.Create($"{updatableType}.Update()")
                };
                groupInfo.UpdatableInfos.Add(info);
                updatableInfos[updatable] = info;

                // 登録通知
                if (updatable is IUpdatableEventHandler handler) {
                    handler.OnRegistered(this);
                }
            }
        }

        /// <summary>
        /// 更新グループ情報のクリア
        /// </summary>
        private void ClearGroupInfo<TUpdatable>(UpdatableGroupInfo<TUpdatable> groupInfo)
            where TUpdatable : IUpdatableBase {
            for (var i = groupInfo.UpdatableInfos.Count - 1; i >= 0; i--) {
                var updatableInfo = groupInfo.UpdatableInfos[i];
                if (updatableInfo.Status != UpdatableState.Killed) {
                    updatableInfo.Status = UpdatableState.Killed;

                    // 登録解除通知
                    if (updatableInfo.Updatable is IUpdatableEventHandlerBase<TUpdatable> handler) {
                        handler.OnUnregistered(this);
                    }
                }
            }
        }
    }
}