using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 汎用ダイアログ用のUIService
    /// </summary>
    public class DialogUIService : UIService {
        [SerializeField, Tooltip("メッセージダイアログ用のスクリーン")]
        private MessageDialogUIScreen _messageDialogScreen;
        [SerializeField, Tooltip("選択ダイアログ用のスクリーン")]
        private SelectionDialogUIScreen _selectionDialogScreen;

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
        public async UniTask<MessageDialogUIScreen.Result> OpenMessageDialogAsync(string title, string subTitle, string content, string decideText = "決定", string cancelText = "キャンセル", bool useBackgroundCancel = false, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();
            _messageDialogScreen.Setup(title, subTitle, content, decideText, cancelText, useBackgroundCancel);
            var index = await _messageDialogScreen.OpenDialogAsync(ct);
            return (MessageDialogUIScreen.Result)index;
        }

        /// <summary>
        /// 選択ダイアログを開く処理
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="itemLabels">内容</param>
        /// <param name="closeText">閉じるボタンの表示テキスト</param>
        /// <param name="useBackgroundCancel">背面キャンセルボタンを使うか</param>
        /// <param name="ct">キャンセルハンドリング用</param>
        public async UniTask<int> OpenSelectionDialogAsync(string title, IReadOnlyList<string> itemLabels, string closeText = "閉じる", bool useBackgroundCancel = false, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();
            _selectionDialogScreen.Setup(title, itemLabels, closeText, useBackgroundCancel);
            var index = await _selectionDialogScreen.OpenDialogAsync(ct);
            return index;
        }
    }
}
