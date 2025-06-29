using GameFramework;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;
using R3;

namespace SampleGame.Presentation {
    /// <summary>
    /// 選択用のUIViewの基底
    /// </summary>
    public abstract class SelectorUIView : UIView {
        [SerializeField, Tooltip("左に動かすボタン")]
        private ButtonUIView _leftButtonView;
        [SerializeField, Tooltip("右に動かすボタン")]
        private ButtonUIView _rightButtonView;
        [SerializeField, Tooltip("項目をリピートするか")]
        private bool _repeat;

        private int _itemCount;

        private Subject<int> _changedSelectedIndexSubject;

        /// <summary>要素数</summary>
        public int ItemCount {
            get => _itemCount;
            protected set {
                _itemCount = Mathf.Max(0, value);
                ChangeIndex(Mathf.Clamp(SelectedIndex, _itemCount > 0 ? 0 : -1, _itemCount - 1));
            }
        }
        /// <summary>選択中のIndex</summary>
        public int SelectedIndex { get; private set; } = -1;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            _changedSelectedIndexSubject = new Subject<int>().RegisterTo(scope);
        }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected override void StartInternal(IScope scope) {
            _leftButtonView.ClickedSubject
                .TakeUntil(scope)
                .Subscribe(_ => { ChangeIndex(SelectedIndex - 1); });

            _rightButtonView.ClickedSubject
                .TakeUntil(scope)
                .Subscribe(_ => { ChangeIndex(SelectedIndex + 1); });
        }

        /// <summary>
        /// 選択Indexの変更通知(_selectedIndexの変更より前)
        /// </summary>
        /// <param name="index">変更されたIndex(負の値は無効値)</param>
        protected abstract void OnChangedIndex(int index);

        /// <summary>
        /// Indexの設定
        /// </summary>
        public void SetIndex(int index) {
            ChangeIndex(index);
        }

        /// <summary>
        /// Indexの変更
        /// </summary>
        private void ChangeIndex(int index) {
            if (_repeat) {
                if (_itemCount < 1) {
                    index = -1;
                }
                else {
                    index = (index + _itemCount) % _itemCount;
                }
            }
            else {
                index = Mathf.Clamp(index, -1, _itemCount - 1);
            }

            var currentIndex = SelectedIndex;
            if (currentIndex == index) {
                return;
            }

            OnChangedIndex(index);
            _leftButtonView.Interactable = _repeat || index > 0;
            _rightButtonView.Interactable = _repeat || index < _itemCount - 1;
            SelectedIndex = index;
            _changedSelectedIndexSubject.OnNext(SelectedIndex);
        }
    }
}