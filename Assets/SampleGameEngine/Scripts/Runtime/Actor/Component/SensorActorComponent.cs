using GameFramework.ActorSystem;
using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// 環境情報サーチ用ActorComponent
    /// </summary>
    public sealed class SensorActorComponent : ActorViewComponent {
        /// <summary>
        /// 設定
        /// </summary>
        public interface ISettings {
            /// <summary>地面検出半径</summary>
            float GroundSensorRadius { get; }
            /// <summary>地面検出位置Yオフセット</summary>
            float GroundSensorOffsetY { get; }
        }

        private readonly IMovable _movable;
        private readonly ISettings _settings;
        private readonly int _groundLayerMask;
        private readonly Collider[] _workColliders = new Collider[16];
        
        /// <inheritdoc/>
        public override int ExecutionOrder => (int)ActorComponentOrder.Sensor;
        
        /// <summary>空中にいるか</summary>
        public bool IsAir { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="movable">移動制御対象</param>
        /// <param name="settings">設定値</param>
        /// <param name="groundLayerMask">地面検出Layer</param>
        public SensorActorComponent(IMovable movable, ISettings settings, int groundLayerMask) {
            _movable = movable;
            _settings = settings;
            _groundLayerMask = groundLayerMask;
        }

        /// <inheritdoc/>
        protected override void UpdateInternal(float deltaTime) {
            // 地面をスフィアで検出
            var groundSensorPosition = _movable.Position + Vector3.up * _settings.GroundSensorOffsetY;
            var count = Physics.OverlapSphereNonAlloc(groundSensorPosition, _settings.GroundSensorRadius, _workColliders, _groundLayerMask);
            IsAir = count <= 0;
        }

        /// <inheritdoc/>
        protected override void DrawGizmosInternal() {
            var prevColor = Gizmos.color;
            var actorPos = _movable.Position;
            
            // 地面スフィア
            var groundSensorPosition = actorPos + Vector3.up * _settings.GroundSensorOffsetY;
            Gizmos.color = IsAir ? Color.red : Color.green;
            Gizmos.DrawWireSphere(groundSensorPosition, _settings.GroundSensorRadius);

            Gizmos.color = prevColor;
        }
    }
}