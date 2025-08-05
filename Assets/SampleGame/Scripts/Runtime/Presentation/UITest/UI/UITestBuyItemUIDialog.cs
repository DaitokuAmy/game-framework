using System.Linq;
using GameFramework;
using GameFramework.Core;
using GameFramework.UISystems;
using R3;
using ThirdPersonEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame.Presentation.UITest {
    /// <summary>
    /// アイテム購入用ダイアログ
    /// </summary>
    public class UITestBuyItemUIDialog : AnimatableUIDialog, IDialog<UITestBuyItemUIDialog.Result> {
        /// <summary>
        /// 結果
        /// </summary>
        public struct Result {
            public int ItemId;
            public int Count;
        }

        [SerializeField, Tooltip("タイトルテキスト")]
        private TMP_Text _titleText;
        [SerializeField, Tooltip("サムネ設定用イメージ")]
        private RawImage _thumbnailImage;
        [SerializeField, Tooltip("個数選択用ビュー")]
        private TextSelectorUIView _selectorView;
        [SerializeField, Tooltip("決定ボタンビュー")]
        private ButtonUIView _decideButtonView;
        [SerializeField, Tooltip("キャンセルボタンビュー")]
        private ButtonUIView _cancelButtonView;

        private int _count;

        /// <summary>設定されている項目Id</summary>
        public int ItemId { get; private set; }

        /// <summary>決定押下時通知</summary>
        public Observable<int> DecidedSubject => _decideButtonView.ClickedSubject.Select(_ => _count);

        /// <inheritdoc/>
        Result IDialog<Result>.GetResult(int selectedIndex) {
            if (selectedIndex < 0 || _count <= 0) {
                return new Result {
                    ItemId = -1,
                    Count = 0,
                };
            }

            return new Result {
                ItemId = ItemId,
                Count = _count,
            };
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            // キャンセルボタン
            _cancelButtonView.ClickedSubject
                .TakeUntil(scope)
                .Subscribe(_ => { Cancel(); });
            
            // セレクター
            _selectorView.ChangedSelectedIndexSubject
                .TakeUntil(scope)
                .Subscribe(idx => {
                    _count = idx + 1;
                });
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        public void Setup(int itemId) {
            ItemId = itemId;
        }

        /// <summary>
        /// 情報の設定
        /// </summary>
        public void SetInformation(int count, int maxCount, string itemName, Texture thumbnail) {
            _count = count;
            _titleText.text = itemName;
            _thumbnailImage.texture = thumbnail;
            _selectorView.Setup(Enumerable.Range(1, maxCount).Select(x => x.ToString()).ToArray());
            _selectorView.SetIndex(count - 1);
        }

        /// <summary>
        /// 決定
        /// </summary>
        public void Decide() {
            SelectIndex(0);
        }
    }
}