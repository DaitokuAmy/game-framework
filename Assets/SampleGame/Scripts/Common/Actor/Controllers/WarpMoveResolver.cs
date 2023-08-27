using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// ワープ移動解決用クラス
    /// </summary>
    public class WarpMoveResolver : MoveResolver, IPointMoveResolver {
        private bool _ignoreY;

        private Vector3 _targetPosition;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ignoreY">Y軸の移動を抑制するか</param>
        public WarpMoveResolver(bool ignoreY = true) {
            _ignoreY = ignoreY;
        }

        /// <summary>
        /// 座標指定移動
        /// </summary>
        void IPointMoveResolver.MoveToPoint(Vector3 point, bool updateRotation) {
            // ある程度の距離離れていなければ無視
            var sqrDistance = GetSqrDistance(point, _ignoreY);
            if (sqrDistance <= TolerancePow2) {
                EndMove(true);
                return;
            }
            
            // 移動開始
            _targetPosition = point;
            StartMove();
        }

        /// <summary>
        /// 移動スキップ
        /// </summary>
        protected override void SkipInternal() {
            var point = _targetPosition;
            if (_ignoreY) {
                point.y = Actor.GetPosition().y;
            }

            Actor.SetPosition(point);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime, float speedMultiplier) {            
            // 座標の吸着
            Skip();
        }
    }
}
