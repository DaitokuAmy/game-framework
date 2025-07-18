using System.Collections.Generic;
using GameFramework;
using GameFramework.Core;
using TMPro;
using R3;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace SituationTreeSample {
    /// <summary>
    /// 遷移用メニューView
    /// </summary>
    public class TransitionMenuView : MonoBehaviour {
        [SerializeField, Tooltip("タイトルテキスト")]
        private TextMeshProUGUI _titleText;
        [SerializeField, Tooltip("選択肢用のテンプレ")]
        private SelectionItemView _itemViewTemplate;
        [SerializeField, Tooltip("戻るボタン")]
        private Button _backButton;

        private readonly List<SelectionItemView> _itemViews = new();

        private ObjectPool<SelectionItemView> _itemViewPool;
        private Subject<int> _selectedSubject = new();
        private DisposableScope _itemsScope = new();

        /// <summary>タイトル</summary>
        public string Title {
            get => _titleText.text;
            set => _titleText.text = value;
        }
        /// <summary>戻る通知</summary>
        public Observable<Unit> BackSubject => _backButton.OnClickAsObservable();
        /// <summary>項目選択通知</summary>
        public Observable<int> SelectedSubject => _selectedSubject;

        /// <summary>
        /// 項目の初期化
        /// </summary>
        public void SetupItems(int currentIndex, params string[] items) {
            CleanupItems();

            // 項目を生成
            for (var i = 0; i < items.Length; i++) {
                var itemView = _itemViewPool.Get();
                var index = i;
                itemView.Text = items[i];
                itemView.ClickedSubject.Select(_ => index)
                    .TakeUntil(_itemsScope)
                    .Subscribe(_selectedSubject.OnNext);
                itemView.IsCurrent = i == currentIndex;
                _itemViews.Add(itemView);
            }
        }

        /// <summary>
        /// 項目のクリーンアップ
        /// </summary>
        public void CleanupItems() {
            _itemsScope.Clear();

            var itemViews = _itemViews.ToArray();
            foreach (var itemView in itemViews) {
                _itemViewPool.Release(itemView);
            }

            _itemViews.Clear();
        }

        /// <summary>
        /// 項目の選択
        /// </summary>
        public void SelectItem(int itemIndex) {
            for (var i = 0; i < _itemViews.Count; i++) {
                var itemView = _itemViews[i];
                itemView.IsCurrent = i == itemIndex;
            }
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            // Poolの初期化
            _itemViewPool = new ObjectPool<SelectionItemView>(() => {
                _itemViewTemplate.gameObject.SetActive(true);
                var itemView = Instantiate(_itemViewTemplate, _itemViewTemplate.transform.parent, false);
                _itemViewTemplate.gameObject.SetActive(false);
                itemView.gameObject.SetActive(false);
                return itemView;
            }, itemView => {
                itemView.gameObject.SetActive(true);
                itemView.Text = "";
                itemView.transform.SetAsLastSibling();
            }, itemView => { itemView.gameObject.SetActive(false); }, itemView => { Destroy(itemView.gameObject); });
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        private void Start() {
            _itemViewTemplate.gameObject.SetActive(false);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            _selectedSubject.OnCompleted();
            _selectedSubject.Dispose();

            CleanupItems();

            _itemViewPool.Dispose();
            _itemsScope.Dispose();
        }
    }
}