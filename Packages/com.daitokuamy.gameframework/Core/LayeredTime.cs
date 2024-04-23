using System;
using UnityEngine;

#if USE_R3
using R3;
#elif USE_UNI_RX
using UniRx;
#endif

namespace GameFramework.Core {
    /// <summary>
    /// 時間の階層管理用クラス
    /// </summary>
    public class LayeredTime : IDisposable {
        // 自身の持つTimeLayer
        private float _localTimeScale = 1.0f;

        // TimeScaleの変更通知
        public event Action<float> OnChangedTimeScale;
        // 内部用TimeScaleの変更通知（通知タイミングを揃えるため）
        private event Action<float> OnChangedTimeScaleInternal;

#if USE_R3
        // TimeScaleの変化を監視するためのReactiveProperty
        private readonly ReactiveProperty<float> _timeScaleProp = new(1.0f);
        public ReadOnlyReactiveProperty<float> TimeScaleProp => _timeScaleProp;
#elif USE_UNI_RX
        // TimeScaleの変化を監視するためのReactiveProperty
        private readonly FloatReactiveProperty _timeScaleProp = new(1.0f);
        public IReadOnlyReactiveProperty<float> TimeScaleProp => _timeScaleProp;
#endif

        // 階層用の親TimeLayer
        public LayeredTime Parent { get; private set; }
        // 自身のTimeScale
        public float LocalTimeScale {
            get => _localTimeScale;
            set {
                _localTimeScale = Mathf.Max(0.0f, value);
                var timeScale = TimeScale;
                OnChangedTimeScaleInternal?.Invoke(timeScale);
            }
        }
        // 親階層を考慮したTimeScale
        public float TimeScale => ParentTimeScale * _localTimeScale;
        public float ParentTimeScale => Parent?.TimeScale ?? 1.0f;
        // 現フレームのDeltaTime
        public float DeltaTime => BaseDeltaTime * TimeScale;
        public float ParentDeltaTime => BaseDeltaTime * ParentTimeScale;
        // 固定DeltaTime(0以下の値を入れると変動DeltaTime)
        public float FixedDeltaTime { get; set; } = 0.0f;

        // ベースとなるDeltaTime
        private float BaseDeltaTime => FixedDeltaTime <= float.Epsilon ? Time.deltaTime : FixedDeltaTime;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent">親となるTimeLayer, 未指定の場合UnityEngine.Timeに直接依存</param>
        public LayeredTime(LayeredTime parent = null) {
            OnChangedTimeScaleInternal += x => OnChangedTimeScale?.Invoke(x);
#if USE_R3
            OnChangedTimeScaleInternal += x => _timeScaleProp.Value = x;
            _timeScaleProp.Value = TimeScale;
#elif USE_UNI_RX
            OnChangedTimeScaleInternal += x => _timeScaleProp.Value = x;
            _timeScaleProp.Value = TimeScale;
#endif
            SetParent(parent);
        }

        /// <summary>
        /// 親のTimeLayerの設定
        /// </summary>
        /// <param name="parent">親となるTimeLayer, 未指定の場合UnityEngine.Timeに直接依存</param>
        public void SetParent(LayeredTime parent) {
            if (Parent != null) {
                Parent.OnChangedTimeScaleInternal -= OnChangedParentTimeScale;
                Parent = null;
            }

            Parent = parent;

            if (Parent != null) {
                Parent.OnChangedTimeScaleInternal += OnChangedParentTimeScale;
            }

            OnChangedParentTimeScale(TimeScale);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (Parent != null) {
                Parent.OnChangedTimeScaleInternal -= OnChangedParentTimeScale;
                Parent = null;
            }
        }

        /// <summary>
        /// 親のTimeScale変換通知
        /// </summary>
        private void OnChangedParentTimeScale(float timeScale) {
            OnChangedTimeScaleInternal?.Invoke(TimeScale);
        }
    }
}