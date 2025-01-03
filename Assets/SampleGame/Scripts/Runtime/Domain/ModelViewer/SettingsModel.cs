using GameFramework.Core;
using GameFramework.ModelSystems;
using UniRx;
using UnityEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlySettingsModel {
        /// <summary>現在のアクターアセットキー</summary>
        IReadOnlyReactiveProperty<string> CurrentActorAssetKey { get; }
        /// <summary>カメラ操作タイプ</summary>
        IReadOnlyReactiveProperty<CameraControlType> CameraControlType { get; }
        /// <summary>時間管理</summary>
        LayeredTime LayeredTime { get; }
    }
    
    /// <summary>
    /// 設定用モデル
    /// </summary>
    public class SettingsModel : AutoIdModel<SettingsModel>, IReadOnlySettingsModel {
        /// <summary>TimeScaleの最大値</summary>
        public const float TimeScaleMax = 10.0f;

        private ReactiveProperty<string> _currentActorAssetKey;
        private ReactiveProperty<CameraControlType> _cameraControlType;

        /// <summary>現在のアクターアセットキー</summary>
        public IReadOnlyReactiveProperty<string> CurrentActorAssetKey => _currentActorAssetKey;
        /// <summary>カメラ操作タイプ</summary>
        public IReadOnlyReactiveProperty<CameraControlType> CameraControlType => _cameraControlType;
        /// <summary>時間管理</summary>
        public LayeredTime LayeredTime { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private SettingsModel(int id) 
            : base(id) {}

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
        /// アクターアセットキーの変更
        /// </summary>
        public void ChangeActorAssetKey(string actorAssetKey) {
            _currentActorAssetKey.Value = actorAssetKey;
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

            _currentActorAssetKey = new ReactiveProperty<string>().ScopeTo(scope);
            _cameraControlType = new ReactiveProperty<CameraControlType>(ModelViewer.CameraControlType.Default).ScopeTo(scope);
        }
    }
}
