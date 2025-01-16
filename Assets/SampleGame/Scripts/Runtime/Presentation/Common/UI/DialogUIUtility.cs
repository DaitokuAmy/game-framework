using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.UISystems;

namespace SampleGame.Presentation {
    /// <summary>
    /// 常駐ダイアログUI用のユーティリティ
    /// </summary>
    public static class DialogUIUtility {
        /// <summary>UIManager</summary>
        private static UIManager Manager  => Services.Get<UIManager>();
        /// <summary>ResidentUI用のService</summary>
        private static DialogUIService UIService => Manager?.GetService<DialogUIService>();

        /// <summary>
        /// メッセージダイアログを開く処理
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="subTitle">サブタイトル</param>
        /// <param name="content">内容</param>
        /// <param name="decideText">決定ボタンの表示テキスト</param>
        /// <param name="cancelText">キャンセルボタンの表示テキスト</param>
        /// <param name="useBackgroundCancel">背面キャンセルボタンを使うか</param>
        /// <param name="ct">キャンセルハンドリング用</param>
        public static UniTask<MessageDialogUIScreen.Result> OpenMessageDialogAsync(string title, string subTitle, string content, string decideText = "決定", string cancelText = "キャンセル", bool useBackgroundCancel = false, CancellationToken ct = default) {
            return UIService.OpenMessageDialogAsync(title, subTitle, content, decideText, cancelText, useBackgroundCancel, ct);
        }
    }
}