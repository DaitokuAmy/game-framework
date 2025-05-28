#if USE_ANIMATION_RIGGING
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// AimRigLayer制御用パーツ
    /// </summary>
    public class MultiAimRigParts : RigParts {
        [SerializeField, Tooltip("ターゲットにするGameObjectのリスト")]
        private GameObject[] _sourceObjects;

        // ターゲットの総数
        public int TargetCount => _sourceObjects.Length;

        /// <summary>
        /// ターゲット座標の設定
        /// </summary>
        /// <param name="position">ターゲット座標</param>
        /// <param name="index">複数ターゲットがある時用のIndex</param>
        public void SetTargetPosition(Vector3 position, int index = 0) {
            if (index < 0 || index >= _sourceObjects.Length) {
                return;
            }

            var target = _sourceObjects[index];
            if (target == null) {
                return;
            }

            target.transform.position = position;
        }
    }
}
#endif