using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// VFXビューア用の設定
    /// </summary>
    public class VfxViewerSettings : MonoBehaviour {
        [SerializeField, Tooltip("開始位置")]
        private Transform _startPoint;
        [SerializeField, Tooltip("遠距離用ターゲット")]
        private Transform _targetPoint;
        [SerializeField, Tooltip("遠距離用のパラメータ")]
        private StraightBulletProjectile.Context _projectileContext;
        
        /// <summary>開始位置</summary>
        public Transform StartPoint => _startPoint;
        /// <summary>遠距離ターゲット</summary>
        public Transform TargetPoint => _targetPoint;
        /// <summary>遠距離用パラメータ</summary>
        public StraightBulletProjectile.Context ProjectileContext => _projectileContext;
    }
}
