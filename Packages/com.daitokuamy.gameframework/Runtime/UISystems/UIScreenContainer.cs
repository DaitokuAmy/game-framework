using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIScreenコンテナクラス
    /// </summary>
    public class UIScreenContainer : UIScreen {
        /// <summary>
        /// 子要素
        /// </summary>
        [Serializable]
        protected class ChildScreen {
            [Tooltip("子要素を表現するキー")]
            public string key;
            [Tooltip("子要素のUIScreen")]
            public UIScreen uiScreen;
        }

        [SerializeField, Tooltip("子要素となるUIScreen情報")]
        private List<ChildScreen> _childScreens = new();

        private Dictionary<string, ChildScreen> _cachedChildScreens = new();

        /// <summary>
        /// 開く処理
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new AnimationHandle OpenAsync(TransitionType transitionType, bool immediate) {
            return base.OpenAsync(transitionType, immediate);
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new AnimationHandle CloseAsync(TransitionType transitionType, bool immediate) {
            return base.CloseAsync(transitionType, immediate);
        }

        /// <summary>
        /// 子要素の追加
        /// </summary>
        /// <param name="childKey">子要素を表すキー</param>
        /// <param name="uIScreen">追加対象のUIScreen</param>
        public void Add(string childKey, UIScreen uIScreen) {
            if (uIScreen == null) {
                Debug.LogWarning($"Child view is null. [{childKey}]");
                return;
            }

            if (_cachedChildScreens.ContainsKey(childKey)) {
                Debug.LogWarning($"Already exists child key. [{childKey}]");
                return;
            }
            
            // 閉じておく
            uIScreen.CloseAsync(TransitionType.None, true, true);

            // 要素を子として登録
            uIScreen.transform.SetParent(transform, false);
            var childView = new ChildScreen {
                key = childKey,
                uiScreen = uIScreen
            };
            _cachedChildScreens[childKey] = childView;
            _childScreens.Add(childView);
        }

        /// <summary>
        /// 子要素の削除
        /// </summary>
        /// <param name="childKey">子要素を表すキー</param>
        public bool Remove(string childKey) {
            if (!_cachedChildScreens.TryGetValue(childKey, out var childScreen)) {
                return false;
            }

            // 子要素の削除
            if (childScreen.uiScreen != null) {
                var uIScreen = childScreen.uiScreen;
                childScreen.uiScreen = null;
                Destroy(uIScreen);
            }

            _cachedChildScreens.Remove(childKey);
            _childScreens.Remove(childScreen);

            return true;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);
            
            _cachedChildScreens = _childScreens.ToDictionary(x => x.key, x => x);
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        /// <param name="scope"></param>
        protected override void StartInternal(IScope scope) {
            base.StartInternal(scope);
            
            foreach (var screen in _childScreens) {
                screen.uiScreen.CloseAsync(TransitionType.None, true);
            }
        }

        /// <summary>
        /// ChildViewの検索
        /// </summary>
        protected ChildScreen FindChild(string childKey) {
            if (string.IsNullOrEmpty(childKey)) {
                return null;
            }
            
            if (!_cachedChildScreens.TryGetValue(childKey, out var childView)) {
                Debug.LogWarning($"Not found child view. [{childKey}]");
                return null;
            }

            return childView;
        }

        /// <summary>
        /// 子の並び順を一番下にする
        /// </summary>
        protected void SetAsLastSibling(string childKey) {
            var screen = FindChild(childKey);
            if (screen == null) {
                return;
            }
            screen.uiScreen.transform.SetAsLastSibling();
        }
    }
}