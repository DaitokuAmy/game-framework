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
        /// 初期化用データ
        /// </summary>
        public interface IData {
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
        private sealed class ItemView : MonoBehaviour, IItemView {
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
            public IData Data;
        }

        [SerializeField, Tooltip("リストとして利用するScrollRect")]
        private ScrollRect _scrollRect;
        [SerializeField, Tooltip("テンプレート情報")]
        private TemplateInfo[] _templateInfos;

        private readonly Dictionary<RectTransform, ObjectPool<IItemView>> _viewPools = new();
        private readonly Dictionary<string, TemplateInfo> _templateInfoMap = new();
        private readonly List<ItemInfo> _activeItemInfos = new();
        private readonly List<IItemView> _activeViews = new();
        private readonly List<string> _itemTemplateKeys = new();
        private readonly List<float> _itemSizes = new();

        private bool _initialized;
        private LayoutElement _beginSpacer;
        private LayoutElement _endSpacer;
        private ObjectPool<ItemInfo> _itemInfoPool;
        private IReadOnlyList<IData> _dataList;
        private Action<IItemView, IData> _initAction;

        private bool _updateListRequest;
        private int _prevStartIndex = -1;
        private int _prevEndIndex = -1;
        private Vector2 _prevScrollPosition;
        private HorizontalOrVerticalLayoutGroup _layoutGroup;

        /// <summary>制御しているScrollRect</summary>
        public ScrollRect ScrollRect => _scrollRect;
        /// <summary>ビューの一覧</summary>
        public IReadOnlyList<IItemView> ItemViews => _activeViews;

        /// <summary>ItemView生成時イベント</summary>
        public event Action<IItemView> CreatedItemViewEvent;
        /// <summary>ItemView削除時イベント</summary>
        public event Action<IItemView> DeletedItemViewEvent;

        /// <summary>有効な状態か</summary>
        private bool IsValid => _dataList != null && _initAction != null;
        /// <summary>縦スクロールか</summary>
        private bool IsVertical => _scrollRect.vertical;
        /// <summary>項目格納用コンテナ</summary>
        private RectTransform Content => _scrollRect.content;
        /// <summary>表示領域用ビューポート</summary>
        private RectTransform Viewport => _scrollRect.viewport != null ? _scrollRect.viewport : (RectTransform)_scrollRect.transform;

        /// <summary>
        /// 初期化処理を登録
        /// </summary>
        /// <param name="initAction">Viewの初期化関数</param>
        public void SetInitializer(Action<IItemView, IData> initAction) {
            Initialize();
            _initAction = initAction;
        }

        /// <summary>
        /// データとテンプレート選択関数を設定
        /// </summary>
        /// <param name="dataList">項目を初期化する際に使うデータのリスト</param>
        /// <param name="getTemplateKeyFunc">Templateのキーを選択する関数</param>
        /// <param name="calcItemSizeFunc">項目サイズを計算する関数(未指定だとTemplate側に設定された物が利用される, 0以下を返しても同様)</param>
        public void SetDataList(IReadOnlyList<IData> dataList, Func<IData, string> getTemplateKeyFunc = null, Func<IData, IItemView, float> calcItemSizeFunc = null) {
            Initialize();
            _dataList = dataList;

            _itemTemplateKeys.Clear();
            _itemSizes.Clear();
            _itemTemplateKeys.Capacity = _dataList.Count;
            _itemSizes.Capacity = _dataList.Count;

            var defaultKey = _templateInfos.Length > 0 ? _templateInfos[0].key : string.Empty;
            for (var i = 0; i < _dataList.Count; i++) {
                var key = getTemplateKeyFunc?.Invoke(_dataList[i]) ?? defaultKey;
                if (!_templateInfoMap.TryGetValue(key, out var info)) {
                    info = _templateInfoMap[defaultKey];
                }

                _itemTemplateKeys.Add(key);
                var size = info.size;
                if (calcItemSizeFunc != null) {
                    size = calcItemSizeFunc.Invoke(_dataList[i], info.template.GetComponent<IItemView>());
                    if (size <= 0.0f) {
                        size = info.size;
                    }
                }

                _itemSizes.Add(size);
            }

            // 一度スクロール位置を初期値にしておく
            if (IsVertical) {
                _scrollRect.verticalNormalizedPosition = 1.0f;
            }
            else {
                _scrollRect.horizontalNormalizedPosition = 0.0f;
            }

            Rebuild();
        }

        /// <summary>
        /// 強制リビルド
        /// </summary>
        public void ForceRebuild() {
            Initialize();
            Rebuild();
        }

        /// <summary>
        /// 正規化済みのスクロール値を設定
        /// </summary>
        public void SetNormalizedScrollPosition(float normalizedPosition) {
            Initialize();

            if (!IsValid) {
                return;
            }

            if (IsVertical) {
                _scrollRect.verticalNormalizedPosition = normalizedPosition;
            }
            else {
                _scrollRect.horizontalNormalizedPosition = normalizedPosition;
            }

            UpdateVisibleItems(true);
        }

        /// <summary>
        /// IItemViewにアタッチされているDataの取得（なければnull）
        /// </summary>
        public IData GetData(IItemView view) {
            var index = _activeViews.IndexOf(view);
            if (index < 0) {
                return null;
            }

            return _activeItemInfos[index].Data;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            Initialize();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            if (!_initialized) {
                return;
            }

            ClearItems();
            _itemInfoPool.Dispose();
            foreach (var pair in _viewPools) {
                pair.Value.Dispose();
            }

            _viewPools.Clear();
            _dataList = null;
            _initAction = null;
        }

        /// <summary>
        /// アクティブ時
        /// </summary>
        private void OnEnable() {
            _updateListRequest = true;
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            if (!IsValid) {
                return;
            }

            if (_updateListRequest || (_scrollRect.normalizedPosition - _prevScrollPosition).sqrMagnitude > float.Epsilon) {
                UpdateVisibleItems();
                _prevScrollPosition = _scrollRect.normalizedPosition;
                _updateListRequest = false;
            }
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;

            // Templateの初期化
            _itemInfoPool = new ObjectPool<ItemInfo>(() => new ItemInfo());
            foreach (var templateInfo in _templateInfos) {
                if (string.IsNullOrEmpty(templateInfo.key) || templateInfo.template == null) {
                    continue;
                }

                var template = templateInfo.template;
                var templateObj = template.gameObject;

                templateObj.SetActive(true);

                // Componentが無ければ追加
                var templateView = templateObj.GetComponent<IItemView>();
                if (templateView == null) {
                    templateView = templateObj.AddComponent<ItemView>();
                }

                templateObj.SetActive(false);

                if (template.parent != Content) {
                    continue;
                }

                if (!_templateInfoMap.TryAdd(templateInfo.key, templateInfo)) {
                    continue;
                }

                _viewPools[templateInfo.template] = new ObjectPool<IItemView>(
                    createFunc: () => {
                        template.gameObject.SetActive(true);
                        var instance = Instantiate(template.gameObject, Content, false);
                        template.gameObject.SetActive(false);
                        var view = instance.GetComponent<IItemView>();
                        CreatedItemViewEvent?.Invoke(view);
                        return view;
                    },
                    actionOnGet: view => view.gameObject.SetActive(true),
                    actionOnRelease: view => view.gameObject.SetActive(false),
                    actionOnDestroy: view => {
                        DeletedItemViewEvent?.Invoke(view);
                        Destroy(view.gameObject);
                    },
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

            // ContentSizeFitterを追加
            var contentSizeFitter = Content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null) {
                contentSizeFitter = Content.gameObject.AddComponent<ContentSizeFitter>();
                contentSizeFitter.verticalFit = IsVertical ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
                contentSizeFitter.horizontalFit = IsVertical ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
            }

            // Spacerの作成
            _beginSpacer = CreateSpacer("BeginSpacer");
            _endSpacer = CreateSpacer("EndSpacer");
        }

        /// <summary>
        /// 表示中の要素を再構築
        /// </summary>
        private void Rebuild() {
            if (!IsValid) {
                return;
            }

            ClearItems();
            UpdateVisibleItems(true);

            // RectTransformをリビルド
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_scrollRect.transform);
        }

        /// <summary>
        /// 可視アイテムの再計算
        /// </summary>
        private void UpdateVisibleItems(bool force = false) {
            var scrollOffset = IsVertical ? Content.anchoredPosition.y : -Content.anchoredPosition.x;
            var viewportSize = IsVertical ? Viewport.rect.height : Viewport.rect.width;

            // 表示開始位置から、可視範囲のインデックスを探索
            var accumulated = (float)(IsVertical ? _layoutGroup.padding.top : _layoutGroup.padding.left);
            var spacing = _layoutGroup.spacing;
            var startIndex = 0;
            var beginSpace = 0.0f;
            for (var i = 0; i < _dataList.Count; i++) {
                var size = GetItemSize(i);
                if (accumulated + size > scrollOffset) {
                    startIndex = i;
                    beginSpace -= spacing;
                    break;
                }

                accumulated += size + spacing;
                beginSpace += size + spacing;
            }

            var visibleExtent = 0.0f;
            var endIndex = _dataList.Count - 1;
            for (var i = startIndex + 1; i < _dataList.Count; i++) {
                visibleExtent += GetItemSize(i) + spacing;
                if (visibleExtent >= viewportSize) {
                    endIndex = i;
                    break;
                }
            }

            var endSpace = 0.0f;
            for (var i = endIndex + 1; i < _dataList.Count; i++) {
                endSpace += GetItemSize(i) + spacing;
            }

            endSpace -= spacing;

            // StartIndexとEndIndexが変わっていなければ何もしない
            if (!force && startIndex == _prevStartIndex && endIndex == _prevEndIndex) {
                return;
            }

            // 項目をプールに返却
            ClearItems();

            // Indexを記憶
            _prevStartIndex = startIndex;
            _prevEndIndex = endIndex;

            // 表示対象生成
            var startSiblingIndex = _beginSpacer.transform.GetSiblingIndex();
            for (var i = startIndex; i <= endIndex; i++) {
                var data = _dataList[i];
                var key = GetItemTemplateKey(i);
                if (!_templateInfoMap.TryGetValue(key, out var templateInfo)) {
                    continue;
                }

                if (!_viewPools.TryGetValue(templateInfo.template, out var pool)) {
                    continue;
                }

                var view = pool.Get();
                view.gameObject.transform.SetSiblingIndex(startSiblingIndex + 1 + (i - startIndex));
                _initAction?.Invoke(view, data);

                _activeViews.Add(view);
                _activeItemInfos.Add(new ItemInfo {
                    Template = templateInfo.template,
                    View = view,
                    Data = data,
                });
            }

            _endSpacer.transform.SetAsLastSibling();

            SetSpacerSize(_beginSpacer, beginSpace);
            SetSpacerSize(_endSpacer, endSpace);
        }

        /// <summary>
        /// 項目のサイズを取得
        /// </summary>
        private float GetItemSize(int index) {
            if (index < 0 || index >= _itemSizes.Count) {
                return 0.0f;
            }

            return _itemSizes[index];
        }

        /// <summary>
        /// テンプレートキーを取得
        /// </summary>
        private string GetItemTemplateKey(int index) {
            if (index < 0 || index >= _itemTemplateKeys.Count) {
                return string.Empty;
            }

            return _itemTemplateKeys[index];
        }

        /// <summary>
        /// 項目情報のクリア
        /// </summary>
        private void ClearItems() {
            foreach (var info in _activeItemInfos) {
                var key = info.Template;
                if (_viewPools.TryGetValue(key, out var pool)) {
                    pool.Release(info.View);
                }

                info.Template = null;
                info.View = null;
                info.Data = null;
                _itemInfoPool.Release(info);
            }

            _activeItemInfos.Clear();
            _activeViews.Clear();
            _prevScrollPosition = _scrollRect.normalizedPosition;
            _prevStartIndex = -1;
            _prevEndIndex = -1;
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