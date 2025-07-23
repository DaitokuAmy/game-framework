using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

namespace GameFramework.UISystems {
    /// <summary>
    /// 要素を再利用する仮想スクロールリスト（縦横対応）
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class RecyclableScrollList : MonoBehaviour {
        /// <summary>
        /// 初期化用パラメータ
        /// </summary>
        public interface IParam {
        }

        /// <summary>
        /// 項目ビュー用インターフェース
        /// </summary>
        public interface IItemView {
            // ReSharper disable once InconsistentNaming
            GameObject gameObject { get; }
        }

        /// <summary>
        /// インターフェースがない場合にデフォルトでつけるView
        /// </summary>
        public sealed class ItemView : MonoBehaviour, IItemView {
        }

        /// <summary>
        /// テンプレート情報
        /// </summary>
        [Serializable]
        private class TemplateInfo {
            [Tooltip("識別キー")]
            public string key;
            [Tooltip("Content以下に配置してあるTemplate")]
            public RectTransform template;
            [Tooltip("テンプレート項目のサイズ")]
            public float size;
        }

        /// <summary>
        /// 項目情報
        /// </summary>
        private class ItemInfo {
            public RectTransform Template;
            public IItemView View;
        }

        [SerializeField, Tooltip("リストとして利用するScrollRect")]
        private ScrollRect _scrollRect;
        [SerializeField]
        private TemplateInfo[] _templateInfos;

        private readonly Dictionary<RectTransform, ObjectPool<IItemView>> _viewPools = new();
        private readonly Dictionary<string, TemplateInfo> _templateInfoMap = new();
        private readonly List<ItemInfo> _activeItemInfos = new();

        private LayoutElement _topSpacer;
        private LayoutElement _bottomSpacer;
        private ObjectPool<ItemInfo> _itemInfoPool;
        private IReadOnlyList<IParam> _params;
        private Func<IParam, string> _templateKeySelectorFunc;
        private Action<IItemView, IParam> _initializeAction;

        private int _prevStartIndex = -1;
        private int _prevEndIndex = -1;
        private Vector2 _prevScrollPosition;
        private HorizontalOrVerticalLayoutGroup _layoutGroup;

        /// <summary>縦スクロールか</summary>
        private bool IsVertical => _scrollRect.vertical;
        /// <summary>項目格納用コンテナ</summary>
        private RectTransform Content => _scrollRect.content;
        /// <summary>表示領域用ビューポート</summary>
        private RectTransform Viewport => _scrollRect.viewport != null ? _scrollRect.viewport : (RectTransform)_scrollRect.transform;

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _itemInfoPool = new ObjectPool<ItemInfo>(() => new ItemInfo());
            foreach (var templateInfo in _templateInfos) {
                if (string.IsNullOrEmpty(templateInfo.key) || templateInfo.template == null) {
                    continue;
                }

                if (!_templateInfoMap.TryAdd(templateInfo.key, templateInfo)) {
                    continue;
                }

                var template = templateInfo.template;

                if (template.parent != Content) {
                    template.gameObject.SetActive(false);
                    continue;
                }

                _viewPools[templateInfo.template] = new ObjectPool<IItemView>(
                    createFunc: () => {
                        template.gameObject.SetActive(true);
                        var instance = Instantiate(template.gameObject, Content, false);
                        template.gameObject.SetActive(false);
                        var view = instance.GetComponent<IItemView>();
                        if (view == null) {
                            view = instance.AddComponent<ItemView>();
                        }

                        return view;
                    },
                    actionOnGet: item => item.gameObject.SetActive(true),
                    actionOnRelease: item => item.gameObject.SetActive(false),
                    actionOnDestroy: item => Destroy(item.gameObject),
                    collectionCheck: false
                );
            }

            // LayoutGroupを取得
            if (IsVertical) {
                _layoutGroup = Content.GetComponent<VerticalLayoutGroup>();
                if (_layoutGroup == null) {
                    _layoutGroup = Content.gameObject.AddComponent<VerticalLayoutGroup>();
                }
            }
            else {
                _layoutGroup = Content.GetComponent<HorizontalLayoutGroup>();
                if (_layoutGroup == null) {
                    _layoutGroup = Content.gameObject.AddComponent<HorizontalLayoutGroup>();
                }
            }

            // Spacerの作成
            _topSpacer = CreateSpacer("TopSpacer");
            _bottomSpacer = CreateSpacer("BottomSpacer");
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if ((_scrollRect.normalizedPosition - _prevScrollPosition).sqrMagnitude > float.Epsilon) {
                UpdateVisibleItems();
                _prevScrollPosition = _scrollRect.normalizedPosition;
            }
        }

        /// <summary>
        /// 初期化処理を登録
        /// </summary>
        public void SetInitializer(Action<IItemView, IParam> initializer) {
            _initializeAction = initializer;
        }

        /// <summary>
        /// データとテンプレート選択関数を設定
        /// </summary>
        public void SetData(IReadOnlyList<IParam> dataList, Func<IParam, string> templateKeySelector) {
            _params = dataList;
            _templateKeySelectorFunc = templateKeySelector;
            Rebuild();
        }

        /// <summary>
        /// 強制リビルド
        /// </summary>
        public void ForceRebuild() {
            Rebuild();
        }

        /// <summary>
        /// 表示中の要素を再構築
        /// </summary>
        private void Rebuild() {
            foreach (var info in _activeItemInfos) {
                var key = info.Template;
                if (_viewPools.TryGetValue(key, out var pool)) {
                    pool.Release(info.View);
                }

                _itemInfoPool.Release(info);
            }

            _activeItemInfos.Clear();
            _prevScrollPosition = _scrollRect.normalizedPosition;
            _prevStartIndex = -1;
            _prevEndIndex = -1;
            UpdateVisibleItems();
        }

        /// <summary>
        /// 可視アイテムの再計算
        /// </summary>
        private void UpdateVisibleItems() {
            var scrollOffset = IsVertical ? Content.anchoredPosition.y : -Content.anchoredPosition.x;
            var viewportSize = IsVertical ? Viewport.rect.height : Viewport.rect.width;

            // 表示開始位置から、可視範囲のインデックスを探索
            var accumulated = (float)(IsVertical ? _layoutGroup.padding.top : _layoutGroup.padding.left);
            var spacing = _layoutGroup.spacing;
            var startIndex = 0;
            var topSpace = 0.0f;
            for (var i = 0; i < _params.Count; i++) {
                var size = GetItemSize(i);
                if (accumulated + size > scrollOffset) {
                    startIndex = i;
                    topSpace -= spacing;
                    break;
                }

                accumulated += size + spacing;
                topSpace += size + spacing;
            }

            var visibleExtent = 0.0f;
            var endIndex = _params.Count - 1;
            for (var i = startIndex + 1; i < _params.Count; i++) {
                visibleExtent += GetItemSize(i) + spacing;
                if (visibleExtent >= viewportSize) {
                    endIndex = i;
                    break;
                }
            }

            var bottomSpace = 0.0f;
            for (var i = endIndex + 1; i < _params.Count; i++) {
                bottomSpace += GetItemSize(i) + spacing;
            }

            bottomSpace -= spacing;
            
            // StartIndexとEndIndexが変わっていなければ何もしない
            if (startIndex == _prevStartIndex && endIndex == _prevEndIndex) {
                return;
            }
            
            _prevStartIndex = startIndex;
            _prevEndIndex = endIndex;

            // 項目をプールに返却
            foreach (var info in _activeItemInfos) {
                if (_viewPools.TryGetValue(info.Template, out var pool)) {
                    pool.Release(info.View);
                }

                _itemInfoPool.Release(info);
            }

            _activeItemInfos.Clear();

            // 表示対象生成
            var startSiblingIndex = _topSpacer.transform.GetSiblingIndex();
            for (var i = startIndex; i <= endIndex; i++) {
                var param = _params[i];
                var key = _templateKeySelectorFunc.Invoke(param);
                if (!_templateInfoMap.TryGetValue(key, out var templateInfo)) {
                    continue;
                }

                if (!_viewPools.TryGetValue(templateInfo.template, out var pool)) {
                    continue;
                }

                var view = pool.Get();
                view.gameObject.transform.SetSiblingIndex(startSiblingIndex + 1 + (i - startIndex));
                _initializeAction?.Invoke(view, param);

                _activeItemInfos.Add(new ItemInfo {
                    Template = templateInfo.template,
                    View = view,
                });
            }

            _bottomSpacer.transform.SetAsLastSibling();

            SetSpacerSize(_topSpacer, topSpace);
            SetSpacerSize(_bottomSpacer, bottomSpace);
        }

        /// <summary>
        /// 項目のサイズを取得
        /// </summary>
        private float GetItemSize(int index) {
            if (index < 0 || index >= _params.Count) {
                return 0.0f;
            }

            var templateKey = _templateKeySelectorFunc.Invoke(_params[index]);
            if (!_templateInfoMap.TryGetValue(templateKey, out var templateInfo)) {
                return 0.0f;
            }

            return templateInfo.size;
        }

        /// <summary>
        /// Spacerの作成
        /// </summary>
        private LayoutElement CreateSpacer(string spacerName) {
            var obj = new GameObject(spacerName, typeof(RectTransform), typeof(LayoutElement));
            obj.transform.SetParent(Content, false);
            var rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(IsVertical ? 0.0f : 0.5f, IsVertical ? 0.5f : 0.0f);
            rectTransform.anchorMax = new Vector2(IsVertical ? 1.0f : 0.5f, IsVertical ? 0.5f : 1.0f);
            return obj.GetComponent<LayoutElement>();
        }

        /// <summary>
        /// Spacerのサイズ設定
        /// </summary>
        private void SetSpacerSize(LayoutElement spacer, float size) {
            if (size <= 0.0f) {
                spacer.gameObject.SetActive(false);
                return;
            }

            spacer.gameObject.SetActive(true);

            var rectTransform = (RectTransform)spacer.transform;
            if (IsVertical) {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, size);
                spacer.preferredHeight = size;
            }
            else {
                rectTransform.sizeDelta = new Vector2(size, rectTransform.sizeDelta.y);
                spacer.preferredWidth = size;
            }
        }
    }
}