using System;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIDialogクラス（UIDialogContainerに登録して使う想定の物）
    /// </summary>
    public class UIDialog : UIScreen, IDialog {
        [SerializeField, Tooltip("背面タッチ判定用のボタン")]
        private Button _backgroundButton;

        /// <summary>背面ボタンを使うか</summary>
        public bool UseBackgroundButton { get; set; } = true;
        
        /// <summary>キャンセルされた際に返すIndex</summary>
        protected virtual int CanceledIndex => -1;
        
        /// <inheritdoc/>
        public event Action<int> SelectedIndexEvent;

        /// <inheritdoc/>
        public void Cancel() {
            SelectIndex(CanceledIndex);
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            if (_backgroundButton != null) {
                _backgroundButton.onClick.AddListener(OnClickBackgroundButton);
            }
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal() {
            if (_backgroundButton != null) {
                _backgroundButton.onClick.RemoveListener(OnClickBackgroundButton);
            }
            
            base.DeactivateInternal();
        }

        /// <summary>
        /// 選択処理
        /// </summary>
        public void SelectIndex(int index) {
            SelectedIndexEvent?.Invoke(index);
        }

        /// <summary>
        /// 背面ボタン押下時処理
        /// </summary>
        private void OnClickBackgroundButton() {
            if (!UseBackgroundButton) {
                return;
            }
            
            SelectIndex(CanceledIndex);
        }
    }
}
