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
        [SerializeField, Tooltip("ダイアログコンテナ")]
        private UIDialogContainer _dialogContainer;

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
        public async UniTask<MessageUIDialog.Result> OpenMessageDialogAsync(string title, string subTitle, string content, string decideText = "決定", string cancelText = "キャンセル", bool useBackgroundCancel = false, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();
            var index = await _dialogContainer.OpenDialog<MessageUIDialog>("Message", dialog => { dialog.Setup(title, subTitle, content, decideText, cancelText, useBackgroundCancel); });
            return (MessageUIDialog.Result)index;
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
            return await _dialogContainer.OpenDialog<SelectionUIDialog>("Selection", dialog => { dialog.Setup(title, itemLabels, closeText, useBackgroundCancel); });
        }
    }
}