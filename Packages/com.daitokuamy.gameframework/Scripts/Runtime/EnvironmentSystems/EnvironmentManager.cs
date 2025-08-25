using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework.EnvironmentSystems {
    /// <summary>
    /// 環境管理クラス
    /// </summary>
    public class EnvironmentManager : DisposableLateUpdatableTask {
        /// <summary>
        /// 環境設定情報
        /// </summary>
        public class EnvironmentInfo {
            // 設定値
            public IEnvironmentContext context;
            // 遷移残り時間
            public float timer;
            // 遷移時間
            public float duration;
        }

        // 環境情報のスタック
        private List<EnvironmentInfo> _stack = new List<EnvironmentInfo>();
        // 環境情報
        private Dictionary<EnvironmentHandle, EnvironmentInfo> _environmentInfos =
            new Dictionary<EnvironmentHandle, EnvironmentInfo>();
        // 制御用Resolver
        private IEnvironmentResolver _resolver;
        // 時間制御クラス
        private LayeredTime _layeredTime;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">環境設定反映用のResolver</param>
        /// <param name="layeredTime">時間管理クラス</param>
        public EnvironmentManager(IEnvironmentResolver resolver, LayeredTime layeredTime = null) {
            _resolver = resolver;
            _layeredTime = layeredTime;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _stack.Clear();
            _environmentInfos.Clear();
            _resolver = null;
        }

        /// <summary>
        /// 環境のPush適用
        /// </summary>
        /// <param name="context">反映設定用のコンテキスト</param>
        /// <param name="duration">遷移時間</param>
        public EnvironmentHandle Push(IEnvironmentContext context, float duration) {
            // 今回の環境情報の生成
            var info = new EnvironmentInfo {
                context = context,
                timer = duration,
                duration = duration
            };
            var handle = new EnvironmentHandle(info);

            // 情報の記憶
            _environmentInfos[handle] = info;
            _stack.Add(info);

            return handle;
        }

        /// <summary>
        /// 環境の適用解除
        /// </summary>
        public void Remove(EnvironmentHandle handle) {
            if (!_environmentInfos.TryGetValue(handle, out var target)) {
                // 対象ではない
                Debug.LogError("Not found environment handle.");
                return;
            }

            // カレントを戻す場合、バック処理を行う
            var current = GetCurrent();
            if (current == target && _stack.Count > 1) {
                var back = _stack[_stack.Count - 2];
                back.timer = current.duration;
            }

            // インスタンス管理から除外
            _stack.Remove(target);
            _environmentInfos.Remove(handle);
        }

        /// <summary>
        /// 強制更新フラグ
        /// </summary>
        public void ForceApply(float blendDuration = 0.0f) {
            if (_stack.Count <= 0) {
                return;
            }

            var currentInfo = _stack[_stack.Count - 1];
            currentInfo.timer = Mathf.Max(blendDuration, currentInfo.timer);
        }

        /// <summary>
        /// タスク後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            var deltaTime = _layeredTime?.DeltaTime ?? Time.deltaTime;

            // スタックの状態を調べる
            for (var i = 0; i < _stack.Count; i++) {
                var info = _stack[i];
                if (info.timer < 0.0f) {
                    continue;
                }

                // カレントではなければ、時間を-1にする
                if (i != _stack.Count - 1) {
                    info.timer = -1.0f;
                    continue;
                }

                // ブレンド処理を行う
                info.timer -= deltaTime;
                var blendRate = info.timer > float.Epsilon ? Mathf.Clamp01(deltaTime / info.timer) : 1.0f;
                var current = _resolver.GetCurrent();
                var context = _resolver.Lerp(current, info.context, blendRate);

                // 設定反映
                _resolver.Apply(context);
            }
        }

        /// <summary>
        /// 現在適用されているEnvironment情報を取得
        /// </summary>
        private EnvironmentInfo GetCurrent() {
            return _stack.Count > 0 ? _stack[_stack.Count - 1] : null;
        }
    }
}