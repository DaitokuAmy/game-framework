using System.Collections.Generic;
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
            for (var i = 0; i < s_raycastResults.Count; i++) {
                var target = s_raycastResults[i].gameObject;
                if (current != target) {
                    if (target.GetComponent<T>() != null) {
                        ExecuteEvents.Execute(s_raycastResults[i].gameObject, data, function);
                        break;
                    }
                }
            }
        }
    }
}