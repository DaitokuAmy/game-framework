using System;
using UnityEngine;

namespace GameFramework.VfxSystem {
    /// <summary>
    /// Lod制御用のVfxComponent
    /// </summary>
    public sealed class LodVfxComponent : MonoBehaviour, IVfxComponent {
        /// <summary>
        /// Lod情報
        /// </summary>
        [Serializable]
        private class LodInfo {
            [Tooltip("該当レベル")]
            public int Level;
            [Tooltip("該当レベル以下だった場合にアクティブになるオブジェクト")]
            public GameObject[] Targets;
        }

        [SerializeField, Tooltip("Lod情報")]
        private LodInfo[] _lodInfos;

        /// <inheritdoc/>
        bool IVfxComponent.IsPlaying => false;

        /// <inheritdoc/>
        void IVfxComponent.Tick(float deltaTime) {
        }

        /// <inheritdoc/>
        void IVfxComponent.Play() {
        }

        /// <inheritdoc/>
        void IVfxComponent.Stop() {
        }

        /// <inheritdoc/>
        void IVfxComponent.StopImmediate() {
        }

        /// <inheritdoc/>
        void IVfxComponent.SetSpeed(float speed) {
        }

        /// <inheritdoc/>
        void IVfxComponent.SetLodLevel(int level) {
            foreach (var info in _lodInfos) {
                var active = level <= info.Level;
                foreach (var target in info.Targets) {
                    if (target == null || target.activeSelf == active) {
                        continue;
                    }

                    target.SetActive(active);
                }
            }
        }
    }
}