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
            /// <summary>設定値</summary>
            public IEnvironmentContext Context;
            /// <summary>遷移残り時間</summary>
            public float Timer;
            /// <summary>遷移時間</summary>
            public float Duration;
        }

        private readonly List<EnvironmentInfo> _stack = new();
        private readonly Dictionary<EnvironmentHandle, EnvironmentInfo> _environmentInfos = new();
        private readonly LayeredTime _layeredTime;
        
        private IEnvironmentResolver _resolver;
        private bool _disposed;

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
            if (_disposed) {
                return;
            }

            _disposed = true;
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
            if (_disposed) {
                return default;
            }
            
            // 今回の環境情報の生成
            var info = new EnvironmentInfo {
                Context = context,
                Timer = duration,
                Duration = duration
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
            if (_disposed) {
                return;
            }
            
            if (!_environmentInfos.TryGetValue(handle, out var target)) {
                // 対象ではない
                Debug.LogError("Not found environment handle.");
                return;
            }

            // カレントを戻す場合、バック処理を行う
            var current = GetCurrent();
            if (current == target && _stack.Count > 1) {
                var back = _stack[^2];
                back.Timer = current.Duration;
            }

            // インスタンス管理から除外
            _stack.Remove(target);
            _environmentInfos.Remove(handle);
        }

        /// <summary>
        /// 強制更新フラグ
        /// </summary>
        public void ForceApply(float blendDuration = 0.0f) {
            if (_disposed) {
                return;
            }
            
            if (_stack.Count <= 0) {
                return;
            }

            var currentInfo = _stack[^1];
            currentInfo.Timer = Mathf.Max(blendDuration, currentInfo.Timer);
        }

        /// <summary>
        /// タスク後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            var deltaTime = _layeredTime?.DeltaTime ?? Time.deltaTime;

            // スタックの状態を調べる
            for (var i = 0; i < _stack.Count; i++) {
                var info = _stack[i];
                if (info.Timer < 0.0f) {
                    continue;
                }

                // カレントではなければ、時間を-1にする
                if (i != _stack.Count - 1) {
                    info.Timer = -1.0f;
                    continue;
                }

                // ブレンド処理を行う
                info.Timer -= deltaTime;
                var blendRate = info.Timer > float.Epsilon ? Mathf.Clamp01(deltaTime / info.Timer) : 1.0f;
                var current = _resolver.GetCurrent();
                var context = _resolver.Lerp(current, info.Context, blendRate);

                // 設定反映
                _resolver.Apply(context);
            }
        }

        /// <summary>
        /// 現在適用されているEnvironment情報を取得
        /// </summary>
        private EnvironmentInfo GetCurrent() {
            return _stack.Count > 0 ? _stack[^1] : null;
        }
    }
}