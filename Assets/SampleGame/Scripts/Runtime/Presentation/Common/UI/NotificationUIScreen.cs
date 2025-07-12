using GameFramework.Core;
using GameFramework.UISystems;
using ThirdPersonEngine;
using TMPro;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 通知メッセージを表示するためのScreen
    /// </summary>
    public class NotificationUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("テキスト")]
        private TextMeshProUGUI _text;

        private float _timer = -1.0f;

        /// <summary>
        /// 開始
        /// </summary>
        protected override void StartInternal(IScope scope) {
            base.StartInternal(scope);
            CloseAsync(TransitionType.Forward, true);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            base.UpdateInternal(deltaTime);
            if (_timer >= 0.0f) {
                _timer -= deltaTime;
                if (_timer <= 0.0f) {
                    Hide();
                }
            }
        }

        /// <summary>
        /// メッセージの表示
        /// </summary>
        public void Show(string message, float duration = 3.0f) {
            _text.text = message;
            _timer = duration;
            OpenAsync(TransitionType.Forward, false, true);
        }

        /// <summary>
        /// メッセージの非表示
        /// </summary>
        public void Hide() {
            _timer = -1.0f;
            CloseAsync(TransitionType.Forward, false);
        }
    }
}