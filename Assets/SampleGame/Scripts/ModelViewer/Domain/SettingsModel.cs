using GameFramework.Core;
using GameFramework.ModelSystems;
using UniRx;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 設定用モデル
    /// </summary>
    public class SettingsModel : AutoIdModel<SettingsModel> {
        /// <summary>TimeScaleの最大値</summary>
        public const float TimeScaleMax = 10.0f;
        
        private ReactiveProperty<CameraControlType> _cameraControlType = new(ModelViewer.CameraControlType.Default);

        /// <summary>カメラ操作タイプ</summary>
        public IReadOnlyReactiveProperty<CameraControlType> CameraControlType => _cameraControlType;
        /// <summary>時間管理</summary>
        public LayeredTime LayeredTime { get; private set; }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Setup(LayeredTime parentLayeredTime = null) {
            LayeredTime.SetParent(parentLayeredTime);
        }

        /// <summary>
        /// Camera制御タイプの変更
        /// </summary>
        public void ChangeCameraControlType(CameraControlType type) {
            _cameraControlType.Value = type;
        }

        /// <summary>
        /// TimeScaleの設定
        /// </summary>
        public void SetTimeScale(float timeScale) {
            LayeredTime.LocalTimeScale = Mathf.Clamp(timeScale, 0.0f, TimeScaleMax);
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            LayeredTime = new LayeredTime();
            LayeredTime.ScopeTo(scope);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private SettingsModel(int id) 
            : base(id) {}
    }
}
