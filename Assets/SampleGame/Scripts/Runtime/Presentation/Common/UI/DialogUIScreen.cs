using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.UISystems;
using R3;
using ThirdPersonEngine;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame.Presentation {
    /// <summary>
    /// ダイアログ基底
    /// </summary>
    public abstract class UIDialog : AnimatableUIScreen, IDialog {
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

        /// <summary>
        /// 選択処理
        /// </summary>
        public void SelectIndex(int index) {
            SelectedIndexEvent?.Invoke(index);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            if (_backgroundButton != null) {
                _backgroundButton.OnClickAsObservable()
                    .TakeUntil(scope)
                    .Where(_ => UseBackgroundButton)
                    .Subscribe(_ => { SelectIndex(CanceledIndex); });
            }
        }
    }
}