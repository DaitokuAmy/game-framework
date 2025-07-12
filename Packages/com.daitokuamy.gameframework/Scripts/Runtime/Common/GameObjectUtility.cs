using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// GameObject用のユーティリティ
    /// </summary>
    public static class GameObjectUtility {
        /// <summary>
        /// GameObjectの廃棄
        /// </summary>
        public static void Destroy(GameObject gameObject) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Object.DestroyImmediate(gameObject);
                return;
            }
#endif
            Object.Destroy(gameObject);
        }

        /// <summary>
        /// Componentの廃棄
        /// </summary>
        public static void Destroy(Component component) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Object.DestroyImmediate(component);
                return;
            }
#endif
            Object.Destroy(component);
        }

        /// <summary>
        /// レイヤーの再帰的な設定
        /// </summary>
        public static void SetLayer(GameObject target, int layer, bool recursive = true) {
            target.layer = layer;
            if (!recursive) {
                return;
            }

            var targetTrans = target.transform;
            for (var i = 0; i < targetTrans.childCount; i++) {
                SetLayer(targetTrans.GetChild(i).gameObject, layer);
            }
        }
    }
}