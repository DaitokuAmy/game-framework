using GameFramework.ActorSystem;
using UnityEngine;

namespace Sample.Presentation {
    /// <summary>
    /// カメラ制御用アクタービュー
    /// </summary>
    public class CameraActorView : ActorView {
        private readonly Transform _targetPoint;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CameraActorView(Body body)
            : base(body) {
            _targetPoint = Body.Locators["TargetPoint"];
        }

        /// <summary>
        /// ターゲットのTransform情報を設定
        /// </summary>
        public void SetTargetTransform(Vector3 position, Quaternion rotation) {
            _targetPoint.position = position;
            _targetPoint.rotation = rotation;
        }
    }
}