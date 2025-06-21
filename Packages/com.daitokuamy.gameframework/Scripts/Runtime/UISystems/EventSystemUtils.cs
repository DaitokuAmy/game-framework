using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameFramework.UISystems {
    /// <summary>
    /// EventSystemのユーティリティ
    /// </summary>
    public static class EventSystemUtils {
        private static readonly List<RaycastResult> s_raycastResults = new();

        /// <summary>
        /// イベント貫通処理
        /// </summary>
        public static void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
            where T : IEventSystemHandler {
            s_raycastResults.Clear();
            EventSystem.current.RaycastAll(data, s_raycastResults);
            var current = data.pointerCurrentRaycast.gameObject;
            var currentDepth = data.pointerCurrentRaycast.depth;
            var currentIndex = current.transform.GetSiblingIndex();

            for (var i = 0; i < s_raycastResults.Count; i++) {
                var result = s_raycastResults[i];
                if (result.depth > currentDepth) {
                    continue;
                }

                if (result.gameObject.transform.GetSiblingIndex() >= currentIndex) {
                    continue;
                }

                if (result.gameObject.GetComponent<T>() != null) {
                    ExecuteEvents.Execute(result.gameObject, data, function);
                    break;
                }
            }
        }
    }
}