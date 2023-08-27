using System;
using System.Collections;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UniRx;

namespace SampleGame {
    /// <summary>
    /// ゲーム用の定義処理
    /// </summary>
    public static class Extensions {
        /// <summary>
        /// IObservableのコルーチン変換
        /// </summary>
        public static IEnumerator StartAsEnumerator<T>(this IObservable<T> source, IScope scope) {
            var finished = false;
            source
                .Subscribe(_ => { }, () => finished = true)
                .ScopeTo(scope);
            while (!finished) {
                yield return null;
            }
        }

        /// <summary>
        /// タスクへ登録
        /// </summary>
        public static void RegisterTask(this ITask source, TaskOrder order) {
            var taskRunner = Services.Get<TaskRunner>();
            taskRunner.Register(source, order);
        }

        /// <summary>
        /// タスクから登録除外
        /// </summary>
        public static void UnregisterTask(this ITask source) {
            var taskRunner = Services.Get<TaskRunner>();
            taskRunner.Unregister(source);
        }

        /// <summary>
        /// SubjectのComplete & Dispose
        /// </summary>
        public static void SafeDispose<T>(this Subject<T> source) {
            if (source == null) {
                return;
            }
            
            source.OnCompleted();
            source.Dispose();
        }
    }
}