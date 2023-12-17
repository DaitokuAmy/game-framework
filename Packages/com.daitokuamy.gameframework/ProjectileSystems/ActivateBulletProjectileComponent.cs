using System.Collections;
using GameFramework.CollisionSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// GameObjectのActiveコントロール用飛翔体コンポーネント
    /// </summary>
    public class ActivateBulletProjectileComponent : BulletProjectileComponent {
        [SerializeField, Tooltip("再生中にONになっているObject")]
        private GameObject[] _baseObjects;
        [SerializeField, Tooltip("ヒットした瞬間にONになるObject")]
        private GameObject[] _hitObjects;
        [SerializeField, Tooltip("終了した瞬間にONになるObject")]
        private GameObject[] _exitObjects;

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        protected override void StartProjectileInternal() {
            SetActiveObjects(_hitObjects, false);
            SetActiveObjects(_exitObjects, false);

            SetActiveObjects(_baseObjects, true);
        }

        /// <summary>
        /// 飛翔終了子ルーチン処理
        /// </summary>
        protected override IEnumerator ExitProjectileRoutine() {
            SetActiveObjects(_baseObjects, false);
            SetActiveObjects(_exitObjects, true);
            yield break;
        }

        /// <summary>
        /// コリジョンヒット通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        protected override void OnHitCollisionInternal(RaycastHitResult result) {
            SetActiveObjects(_hitObjects, false);
            SetActiveObjects(_hitObjects, true);
        }

        /// <summary>
        /// Particleを停止
        /// </summary>
        private void SetActiveObjects(GameObject[] targets, bool active) {
            foreach (var target in targets) {
                if (target.activeSelf == active) {
                    continue;
                }
                
                target.SetActive(active);
            }
        }
    }
}