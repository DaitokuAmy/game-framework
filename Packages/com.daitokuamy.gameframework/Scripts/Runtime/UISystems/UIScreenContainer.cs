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
    public class UIScreenContainer : UIScreen, ITransitionResolver, IStateContainer<string, UIScreen, UIScreenContainer.Option> {
        /// <summary>OutIn遷移</summary>
        private static readonly ITransition OutInTransition = new OutInTransition();
        /// <summary>Cross遷移</summary>
        private static readonly ITransition CrossTransition = new CrossTransition();

        /// <summary>
        /// オプション
        /// </summary>
        public class Option {
            public bool Immediate;
        }

        /// <summary>
        /// 遷移情報
        /// </summary>
        internal class TransitionInfo : ITransitionInfo<UIScreen> {
            public IReadOnlyList<ITransitionEffect> Effects { get; set; }
            public bool EffectActive { get; set; }

            public TransitionDirection Direction { get; set; }
            public TransitionState State { get; set; }
            public TransitionStep EndStep => TransitionStep.Complete;
            public UIScreen Prev { get; set; }
            public UIScreen Next { get; set; }
            public Coroutine Coroutine { get; set; }

            public bool ChangeEndStep(TransitionStep step) {
                return false;
            }
        }

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
        private TransitionInfo _transitionInfo;

        /// <inheritdoc/>
        public UIScreen Current { get; private set; }

        /// <summary>遷移中か</summary>
        public bool IsTransitioning => _transitionInfo != null;

        /// <inheritdoc/>
        UIScreen IStateContainer<string, UIScreen, Option>.FindState(string key) {
            _cachedChildScreens.TryGetValue(key, out var childScreen);
            return childScreen?.uiScreen;
        }

        /// <inheritdoc/>
        UIScreen[] IStateContainer<string, UIScreen, Option>.GetStates() {
            return _childScreens.Select(x => x.uiScreen).ToArray();
        }

        /// <inheritdoc/>
        TransitionHandle<UIScreen> IStateContainer<string, UIScreen, Option>.Transition(string key, Option option, bool back, TransitionStep endStep, Action<UIScreen> setupAction, ITransition transition, params ITransitionEffect[] effects) {
            return TransitionInternal(key, option, back, setupAction, transition, effects);
        }

        /// <inheritdoc/>
        TransitionHandle<UIScreen> IStateContainer<string, UIScreen, Option>.Reset(Action<UIScreen> setupAction, params ITransitionEffect[] effects) {
            return ResetInternal(setupAction, effects);
        }

        /// <inheritdoc/>
        void ITransitionResolver.Start() {
            _transitionInfo.State = TransitionState.Standby;
            foreach (var effect in _transitionInfo.Effects) {
                effect.BeginTransition();
            }
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.EnterEffectRoutine() {
            yield return new MergedCoroutine(_transitionInfo.Effects.Select(x => x.EnterEffectRoutine()).ToArray());
            _transitionInfo.EffectActive = true;
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.ExitEffectRoutine() {
            _transitionInfo.EffectActive = false;
            yield return new MergedCoroutine(_transitionInfo.Effects.Select(x => x.ExitEffectRoutine()).ToArray());
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.LoadNextRoutine() {
            _transitionInfo.State = TransitionState.Initializing;
            yield break;
        }

        /// <inheritdoc/>
        void ITransitionResolver.ActivateNext() {
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.OpenNextRoutine(bool immediate) {
            _transitionInfo.State = TransitionState.Opening;
            if (_transitionInfo.Next != null) {
                yield return _transitionInfo.Next.OpenAsync(_transitionInfo.Direction, immediate);
            }
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.ClosePrevRoutine(bool immediate) {
            if (_transitionInfo.Prev != null) {
                yield return _transitionInfo.Prev.CloseAsync(_transitionInfo.Direction, immediate);
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
                effect.EndTransition();
            }

            _transitionInfo.State = TransitionState.Completed;
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
                screen.uiScreen.CloseAsync(TransitionDirection.None, true);
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
        /// 開く処理（後処理）
        /// </summary>
        protected override IEnumerator OpenRoutine(TransitionDirection transitionDirection, IScope cancelScope) {
            yield return base.OpenRoutine(transitionDirection, cancelScope);

            if (Current != null) {
                yield return Current.OpenAsync(transitionDirection);
            }
        }

        /// <summary>
        /// 開く処理（後処理）
        /// </summary>
        protected override void PostOpen(TransitionDirection transitionDirection, bool immediate) {
            base.PostOpen(transitionDirection, immediate);

            if (Current != null) {
                Current.OpenAsync(transitionDirection, true);
            }
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        protected override IEnumerator CloseRoutine(TransitionDirection transitionDirection, IScope cancelScope) {
            yield return base.CloseRoutine(transitionDirection, cancelScope);

            if (Current != null) {
                yield return Current.CloseAsync(transitionDirection);
            }
        }

        /// <summary>
        /// 閉じる処理（後処理）
        /// </summary>
        protected override void PostClose(TransitionDirection transitionDirection, bool immediate) {
            base.PostClose(transitionDirection, immediate);

            if (Current != null) {
                Current.CloseAsync(transitionDirection, true);
            }
        }

        /// <summary>
        /// 遷移処理
        /// </summary>
        public TransitionHandle<UIScreen> Transition(string key, Action<UIScreen> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects) {
            return TransitionInternal(key, null, false, setupAction, transition, effects);
        }

        /// <summary>
        /// 遷移処理
        /// </summary>
        public TransitionHandle<UIScreen> Clear(params ITransitionEffect[] effects) {
            return TransitionInternal(null, null, false, null, null, effects);
        }

        /// <summary>
        /// 遷移処理
        /// </summary>
        public TransitionHandle<UIScreen> Transition<TScreen>(string key, Action<TScreen> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects)
            where TScreen : UIScreen {
            return Transition(key, screen => {
                if (screen is TScreen s) {
                    setupAction?.Invoke(s);
                }
            }, transition, effects);
        }

        /// <summary>
        /// 子要素の追加
        /// </summary>
        /// <param name="childKey">子要素を表すキー</param>
        /// <param name="uIScreen">追加対象のUIScreen</param>
        public void Add(string childKey, UIScreen uIScreen) {
            if (uIScreen == null) {
                DebugLog.Warning($"Child view is null. [{childKey}]");
                return;
            }

            if (_cachedChildScreens.ContainsKey(childKey)) {
                DebugLog.Warning($"Already exists child key. [{childKey}]");
                return;
            }

            // 閉じておく
            uIScreen.CloseAsync(TransitionDirection.None, true, true);

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
        /// ChildScreenの検索
        /// </summary>
        protected UIScreen FindChildScreen(string key) {
            if (string.IsNullOrEmpty(key)) {
                return null;
            }

            if (!_cachedChildScreens.TryGetValue(key, out var childScreen)) {
                return null;
            }

            return childScreen.uiScreen;
        }

        /// <summary>
        /// 遷移を強制終了させる
        /// </summary>
        private void ForceExitTransition() {
            if (!IsTransitioning) {
                return;
            }
            
            var transitionInfo = _transitionInfo;
            
            // ステートが完了していたら何もしない
            if (transitionInfo.State == TransitionState.Canceled || transitionInfo.State == TransitionState.Completed) {
                return;
            }

            var resolver = (ITransitionResolver)this;

            // オープン状態を反映
            if (transitionInfo.Prev != null) {
                transitionInfo.Prev.CloseAsync(transitionInfo.Direction, true);
            }

            if (transitionInfo.Next != null) {
                transitionInfo.Next.OpenAsync(transitionInfo.Direction, true);
            }
            
            // 終了処理の呼び出し
            resolver.Finish();

            // コルーチンを停止
            if (transitionInfo.Coroutine != null) {
                StopCoroutine(transitionInfo.Coroutine);
            }

            _transitionInfo = null;
        }

        /// <summary>
        /// 遷移処理
        /// </summary>
        private TransitionHandle<UIScreen> TransitionInternal(string key, Option option, bool back, Action<UIScreen> setupAction, ITransition transition, ITransitionEffect[] effects) {
            // 遷移中なら遷移を強制的に終わらせる
            if (IsTransitioning) {
                ForceExitTransition();
            }

            var prevScreen = Current;
            var nextScreen = FindChildScreen(key);

            // カレントの更新
            Current = nextScreen;

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                Prev = prevScreen,
                Next = nextScreen,
                Direction = back ? TransitionDirection.Back : TransitionDirection.Forward,
                State = TransitionState.Standby,
                Effects = effects ?? Array.Empty<ITransitionEffect>(),
                EffectActive = false
            };

            // 遷移先を一番下に配置
            if (nextScreen != null) {
                nextScreen.transform.SetAsLastSibling();
            }

            // 初期化通知
            if (nextScreen != null) {
                setupAction?.Invoke(nextScreen);
            }

            transition ??= CrossTransition;

            // 遷移
            _transitionInfo.Coroutine = StartCoroutine(transition.TransitionRoutine(this, option?.Immediate ?? false),
                () => { _transitionInfo = null; },
                () => { _transitionInfo = null; },
                ex => {
                    DebugLog.Exception(ex);
                    _transitionInfo = null;
                });

            // ハンドルの返却
            return new TransitionHandle<UIScreen>(_transitionInfo);
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        private TransitionHandle<UIScreen> ResetInternal(Action<UIScreen> setupAction, ITransitionEffect[] effects) {
            // 遷移中なら遷移を強制的に終わらせる
            if (IsTransitioning) {
                ForceExitTransition();
            }

            var prevScreen = Current;
            var nextScreen = Current;

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                Prev = prevScreen,
                Next = nextScreen,
                Direction = TransitionDirection.Forward,
                State = TransitionState.Standby,
                Effects = effects ?? Array.Empty<ITransitionEffect>(),
                EffectActive = false
            };

            // 遷移先を一番下に配置
            if (nextScreen != null) {
                nextScreen.transform.SetAsLastSibling();
            }

            // 初期化通知
            if (nextScreen != null) {
                setupAction?.Invoke(nextScreen);
            }

            // 遷移
            _transitionInfo.Coroutine = StartCoroutine(OutInTransition.TransitionRoutine(this),
                () => { _transitionInfo = null; },
                () => { _transitionInfo = null; },
                ex => {
                    DebugLog.Exception(ex);
                    _transitionInfo = null;
                });

            // ハンドルの返却
            return new TransitionHandle<UIScreen>(_transitionInfo);
        }
    }
}