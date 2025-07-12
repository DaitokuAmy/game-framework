using GameFramework.Core;
using R3;
using UnityEngine;
using GameFramework;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlySettingsModel {
        /// <summary>現在のアクターアセットキー</summary>
        string CurrentActorAssetKey { get; }
        /// <summary>カメラ操作タイプ</summary>
        CameraControlType CameraControlType { get; }
        /// <summary>時間管理</summary>
        LayeredTime LayeredTime { get; }
        /// <summary>モーション再生時に位置をリセットするか</summary>
        bool ResetOnPlay { get; }
        
        /// <summary>アクターアセットキー変更通知</summary>
        Observable<string> ChangedCurrentActorAssetKeySubject { get; }
        /// <summary>カメラ操作タイプの変更通知</summary>
        Observable<CameraControlType> ChangedCameraControlTypeSubject { get; }
    }
    
    /// <summary>
    /// 設定用モデル
    /// </summary>
    public class SettingsModel : SingleModel<SettingsModel>, IReadOnlySettingsModel {
        /// <summary>TimeScaleの最大値</summary>
        public const float TimeScaleMax = 10.0f;

        private Subject<string> _changedCurrentActorAssetKeySubject;
        private Subject<CameraControlType> _changedCameraControlTypeSubject;

        /// <summary>現在のアクターアセットキー</summary>
        public string CurrentActorAssetKey { get; private set; }
        /// <summary>カメラ操作タイプ</summary>
        public CameraControlType CameraControlType { get; private set; }
        /// <summary>時間管理</summary>
        public LayeredTime LayeredTime { get; private set; }
        /// <summary>モーション再生時に位置をリセットするか</summary>
        public bool ResetOnPlay { get; private set; }

        /// <summary>アクターアセットキー変更通知</summary>
        public Observable<string> ChangedCurrentActorAssetKeySubject => _changedCurrentActorAssetKeySubject;
        /// <summary>カメラ操作タイプの変更通知</summary>
        public Observable<CameraControlType> ChangedCameraControlTypeSubject => _changedCameraControlTypeSubject;

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
            CameraControlType = type;
            _changedCameraControlTypeSubject.OnNext(CameraControlType);
        }

        /// <summary>
        /// アクターアセットキーの変更
        /// </summary>
        public void ChangeActorAssetKey(string actorAssetKey) {
            CurrentActorAssetKey = actorAssetKey;
            _changedCurrentActorAssetKeySubject.OnNext(CurrentActorAssetKey);
        }

        /// <summary>
        /// TimeScaleの設定
        /// </summary>
        public void SetTimeScale(float timeScale) {
            LayeredTime.LocalTimeScale = Mathf.Clamp(timeScale, 0.0f, TimeScaleMax);
        }

        /// <summary>
        /// ResetOnPlayの設定
        /// </summary>
        public void SetResetOnPlay(bool reset) {
            ResetOnPlay = reset;;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            LayeredTime = new LayeredTime();
            LayeredTime.RegisterTo(scope);

            _changedCurrentActorAssetKeySubject = new Subject<string>().RegisterTo(scope);
            _changedCameraControlTypeSubject = new Subject<CameraControlType>().RegisterTo(scope);

            CurrentActorAssetKey = "";
            CameraControlType = CameraControlType.Default;
        }
    }
}
