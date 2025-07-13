using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework;
using GameFramework.ActorSystems;

namespace ThirdPersonEngine {
    /// <summary>
    /// タイムスケール制御用ActorComponent
    /// </summary>
    public sealed class TimeScaleActorComponent : ActorComponent {
        /// <summary>
        /// TimeScaleの情報
        /// </summary>
        private class TimeScaleInfo {
            public float TimeScale;
            public float Timer;
        }
        
        private readonly List<TimeScaleInfo> _timeScaleInfos = new();
        
        private LayeredTime _layeredTime;
        private LayeredScale _timeScale;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layeredTime">時間管理用クラス</param>
        public TimeScaleActorComponent(LayeredTime layeredTime) {
            _layeredTime = layeredTime;
            _timeScale = LayeredScale.One;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            if (_layeredTime != null) {
                _layeredTime.LocalTimeScale = 1.0f;
                _layeredTime = null;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            if (_layeredTime == null) {
                return;
            }
            
            var dirty = false;
            
            for (var i = 0; i < _timeScaleInfos.Count; i++) {
                var info = _timeScaleInfos[i];
                if (info.Timer < 0.0f) {
                    continue;
                }
                
                // 時間制限がある場合は時間切れしたらリセット
                info.Timer -= deltaTime;
                if (info.Timer <= 0.0f) {
                    info.Timer = -1;
                    info.TimeScale = 1.0f;
                    _timeScale.Set(i, 1.0f);
                    dirty = true;
                }
            }
            
            // 変更があったらLayeredTimeに適用
            if (dirty) {
                _layeredTime.LocalTimeScale = _timeScale;
            }
        }

        /// <summary>
        /// TimeScaleの設定
        /// </summary>
        /// <param name="index">合成用Index</param>
        /// <param name="timeScale">設定する値</param>
        /// <param name="duration">適用する長さ</param>
        public void SetTimeScale(int index, float timeScale, float duration = -1.0f) {
            if (index < 0) {
                return;
            }
            
            _timeScale.Set(index, timeScale);
            while (index >= _timeScaleInfos.Count) {
                _timeScaleInfos.Add(new TimeScaleInfo {
                    Timer = -1,
                    TimeScale = 1.0f
                });
            }

            _timeScaleInfos[index].TimeScale = timeScale;
            _timeScaleInfos[index].Timer = duration;
            
            // 時間の適用
            _layeredTime.LocalTimeScale = _timeScale;
        }
    }
}