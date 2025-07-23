using GameFramework.ActorSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 環境情報サーチ用ActorComponent
    /// </summary>
    public sealed class SensorActorComponent : ActorComponent {
        /// <summary>
        /// 設定
        /// </summary>
        public interface ISettings {
            /// <summary>地面検出半径</summary>
            float GroundSensorRadius { get; }
            /// <summary>地面検出位置Yオフセット</summary>
            float GroundSensorOffsetY { get; }
        }

        private readonly IMovableActor _actor;
        private readonly ISettings _settings;
        private readonly int _groundLayerMask;
        private readonly Collider[] _workColliders = new Collider[16];
        
        /// <summary>空中にいるか</summary>
        public bool IsAir { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="actor">制御対象のActor</param>
        /// <param name="settings">設定値</param>
        /// <param name="groundLayerMask">地面検出Layer</param>
        public SensorActorComponent(IMovableActor actor, ISettings settings, int groundLayerMask) {
            _actor = actor;
            _settings = settings;
            _groundLayerMask = groundLayerMask;
        }

        /// <inheritdoc/>
        protected override void UpdateInternal(float deltaTime) {
            // 地面をスフィアで検出
            var groundSensorPosition = _actor.GetPosition() + Vector3.up * _settings.GroundSensorOffsetY;
            var count = Physics.OverlapSphereNonAlloc(groundSensorPosition, _settings.GroundSensorRadius, _workColliders, _groundLayerMask);
            IsAir = count <= 0;
        }

        /// <inheritdoc/>
        protected override void DrawGizmosInternal() {
            var prevColor = Gizmos.color;
            
            var actorPos = _actor.GetPosition();
            
            // 地面スフィア
            var groundSensorPosition = actorPos + Vector3.up * _settings.GroundSensorOffsetY;
            Gizmos.color = IsAir ? Color.red : Color.green;
            Gizmos.DrawWireSphere(groundSensorPosition, _settings.GroundSensorRadius);

            Gizmos.color = prevColor;
        }
    }
}