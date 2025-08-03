using GameFramework.Core;
using GameFramework.UISystems;
using R3;
using ThirdPersonEngine;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 常駐UI用のユーティリティ
    /// </summary>
    public static class ResidentUIUtility {
        /// <summary>UIManager</summary>
        private static UIManager Manager => Services.Resolve<UIManager>();
        /// <summary>ResidentUI用のService</summary>
        private static ResidentUIService UIService => Manager?.GetService<ResidentUIService>();

        /// <summary>ブロックスクリーンのタップ検知</summary>
        public static Observable<Unit> OnClickedBlockingSubject => UIService.BlockScreen.OnClickedSubject;

        /// <summary>
        /// ローディングの表示
        /// </summary>
        public static IProcess ShowLoading(string key) {
            return UIService.ShowLoading(key);
        }

        /// <summary>
        /// ローディングの非表示
        /// </summary>
        public static IProcess HideLoading() {
            return UIService.HideLoading();
        }

        /// <summary>
        /// ブロックの開始
        /// </summary>
        public static BlockUIScreen.Handle StartBlocking(int order = 100) {
            return UIService.BlockScreen.StartBlocking(order);
        }

        /// <summary>
        /// ブロックを止める
        /// </summary>
        public static void ClearBlockingAll() {
            UIService.BlockScreen.ClearBlockingAll();
        }

        /// <summary>
        /// 通知メッセージの表示
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="duration">表示時間</param>
        public static void ShowNotification(string message, float duration = 1.0f) {
            UIService.NotificationScreen.Show(message, duration);
        }

        /// <summary>
        /// 通知メッセージの非表示
        /// </summary>
        public static void HideNotification() {
            UIService.NotificationScreen.Hide();
        }

        /// <summary>
        /// フェードイン
        /// </summary>
        public static AsyncOperationHandle ColorFadeInAsync(float duration) {
            return UIService.FaderScreen.FadeInAsync("Color", duration);
        }

        /// <summary>
        /// フェードアウト
        /// </summary>
        public static AsyncOperationHandle ColorFadeOutAsync(Color color, float duration) {
            return UIService.FaderScreen.FadeOutAsync("Color", color, duration);
        }
    }
}