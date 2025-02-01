using System;
using UnityEngine;

namespace GameFramework.VfxSystems {
    /// <summary>
    /// Lod制御用のVfxComponent
    /// </summary>
    public class LodVfxComponent : MonoBehaviour, IVfxComponent {
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

        /// <summary>再生中か</summary>
        bool IVfxComponent.IsPlaying => false;

        /// <summary>
        /// 更新処理
        /// </summary>
        void IVfxComponent.Update(float deltaTime) {
        }

        /// <summary>
        /// 再生
        /// </summary>
        void IVfxComponent.Play() {
        }

        /// <summary>
        /// 停止
        /// </summary>
        void IVfxComponent.Stop() {
        }

        /// <summary>
        /// 即時停止
        /// </summary>
        void IVfxComponent.StopImmediate() {
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        void IVfxComponent.SetSpeed(float speed) {
        }

        /// <summary>
        /// Lodレベルの設定
        /// </summary>
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