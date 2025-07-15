using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIScreenコンテナクラス
    /// </summary>
    public class UIScreenContainer : UIScreen, ITransitionResolver {
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

        /// <summary>
        /// 遷移情報
        /// </summary>
        private class TransitionInfo {
            public UIScreen PrevScreen;
            public UIScreen NextScreen;
            public TransitionType TransitionType;
            public IReadOnlyList<ITransitionEffect> Effects;
            public bool EffectActive;
        }

        [SerializeField, Tooltip("子要素となるUIScreen情報")]
        private List<ChildScreen> _childScreens = new();

        private Dictionary<string, ChildScreen> _cachedChildScreens = new();
        private TransitionInfo _transitionInfo;

        /// <summary>遷移中か</summary>
        public bool IsTransitioning => _transitionInfo != null;

        /// <inheritdoc/>
        void ITransitionResolver.Start() {
            foreach (var effect in _transitionInfo.Effects) {
                effect.Begin();
            }
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.EnterEffectRoutine() {
            yield return new MergedCoroutine(_transitionInfo.Effects.Select(x => x.EnterRoutine()).ToArray());
            _transitionInfo.EffectActive = true;
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.ExitEffectRoutine() {
            _transitionInfo.EffectActive = false;
            yield return new MergedCoroutine(_transitionInfo.Effects.Select(x => x.ExitRoutine()).ToArray());
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.LoadNextRoutine() {
            yield break;
        }

        /// <inheritdoc/>
        void ITransitionResolver.ActivateNext() {
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.OpenNextRoutine(bool immediate) {
            if (_transitionInfo.NextScreen != null) {
                yield return _transitionInfo.NextScreen.OpenAsync(_transitionInfo.TransitionType, immediate);
            }
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.ClosePrevRoutine(bool immediate) {
            if (_transitionInfo.PrevScreen != null) {
                yield return _transitionInfo.PrevScreen.CloseAsync(_transitionInfo.TransitionType, immediate);
            }
        }

        /// <inheritdoc/>
        void ITransitionResolver.DeactivatePrev() {
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.UnloadPrevRoutine() {
            yield break;
        }

        /// <inheritdoc/>
        void ITransitionResolver.Finish() {
            foreach (var effect in _transitionInfo.Effects) {
                effect.End();
            }
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
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            base.UpdateInternal(deltaTime);

            if (IsTransitioning) {
                if (_transitionInfo.EffectActive) {
                    for (var i = 0; i < _transitionInfo.Effects.Count; i++) {
                        _transitionInfo.Effects[i].Update();
                    }
                }
            }
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        public AnimationHandle OpenAsync(TransitionType transitionType, bool immediate) {
            return base.OpenAsync(transitionType, immediate);
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        public AnimationHandle CloseAsync(TransitionType transitionType, bool immediate) {
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

        /// <summary>
        /// 遷移開始
        /// </summary>
        protected Coroutine StartTransition(ITransition transition, UIScreen prevScreen, UIScreen nextScreen, TransitionType transitionType, bool immediate, ITransitionEffect[] effects, Action<UIScreen> initAction, AsyncOperator<UIScreen> asyncOperator) {
            // 遷移中は失敗
            if (IsTransitioning) {
                Debug.LogWarning("Already transitioning.");
                return null;
            }
            
            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                PrevScreen = prevScreen,
                NextScreen = nextScreen,
                TransitionType = transitionType,
                Effects = effects ?? Array.Empty<ITransitionEffect>(),
                EffectActive = false
            };

            // 初期化通知
            if (nextScreen != null) {
                initAction?.Invoke(nextScreen);
            }

            // 遷移
            return StartCoroutine(transition.TransitRoutine(this, immediate),
                () => {
                    asyncOperator.Completed(nextScreen);
                    _transitionInfo = null;
                },
                () => {
                    asyncOperator.Aborted();
                    _transitionInfo = null;
                },
                err => {
                    asyncOperator.Aborted(err);
                    _transitionInfo = null;
                });
        }
    }
}