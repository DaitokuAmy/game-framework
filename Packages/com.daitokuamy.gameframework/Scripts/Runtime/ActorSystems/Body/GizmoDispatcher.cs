using System;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ギズモ描画イベントを提供するためのクラス
    /// </summary>
    [AddComponentMenu(""), DisallowMultipleComponent]
    public class GizmoDispatcher : MonoBehaviour {
        /// <summary>ギズモ描画通知</summary>
        public event Action DrawGizmosEvent;
        /// <summary>選択中ギズモ描画通知</summary>
        public event Action DrawGizmosSelectedEvent;

        /// <summary>
        /// ギズモ描画(選択中)
        /// </summary>
        private void OnDrawGizmosSelected() {
            DrawGizmosSelectedEvent?.Invoke();
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        private void OnDrawGizmos() {
            DrawGizmosEvent?.Invoke();
        }
    }
}