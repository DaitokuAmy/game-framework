using System.Collections;
using GameFramework.CollisionSystem;
using UnityEngine;

namespace GameFramework.ProjectileSystem {
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

        /// <inheritdoc/>
        protected override void PlayInternal() {
            SetActiveObjects(_hitObjects, false);
            SetActiveObjects(_exitObjects, false);

            SetActiveObjects(_baseObjects, true);
        }

        /// <inheritdoc/>
        protected override IEnumerator StopRoutineInternal() {
            SetActiveObjects(_baseObjects, false);
            SetActiveObjects(_exitObjects, true);
            yield break;
        }

        /// <inheritdoc/>
        protected override void OnHitCollisionInternal(RaycastHit hit) {
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
