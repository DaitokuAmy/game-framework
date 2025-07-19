using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション管理用クラス
    /// </summary>
    public sealed class SituationContainer : IDisposable, ITransitionResolver, IMonitoredContainer {
        /// <summary>
        /// 遷移オプション
        /// </summary>
        public class TransitionOption {
            /// <summary>バック遷移</summary>
            public bool Back = false;
            /// <summary>Rootから再構築して遷移するか</summary>
            public bool Refresh = false;
            /// <summary>遷移ステップ(どこまで遷移を進めるか)</summary>
            public TransitionStep Step = TransitionStep.Complete;
        }

        /// <summary>
        /// 遷移情報
        /// </summary>
        public class TransitionInfo {
            public IReadOnlyList<ISituation> PrevSituations;
            public IReadOnlyList<ISituation> NextSituations;
            public TransitionState State;
            public TransitionStep Step;
            public TransitionType TransitionType;
            public IReadOnlyList<ITransitionEffect> Effects;
            public bool EffectActive;
        }

        private readonly CoroutineRunner _coroutineRunner = new();
        private readonly List<Situation> _preloadSituations = new();
        private readonly List<ISituation> _runningSituations = new();
        private readonly string _label;
        
        private TransitionInfo _transitionInfo;
        private bool _disposed;

        /// <inheritdoc/>
        string IMonitoredContainer.Label => _label;
        /// <inheritdoc/>
        TransitionInfo IMonitoredContainer.CurrentTransitionInfo => _transitionInfo;
        /// <inheritdoc/>
        IReadOnlyList<Situation> IMonitoredContainer.PreloadSituations => _preloadSituations;
        /// <inheritdoc/>
        IReadOnlyList<Situation> IMonitoredContainer.RunningSituations => _runningSituations.Cast<Situation>().ToArray();
        
        /// <summary>RootとなるSituation</summary>
        public Situation RootSituation { get; private set; }
        /// <summary>現在のシチュエーション</summary>
        public Situation Current => _runningSituations.Count > 0 ? (Situation)_runningSituations[^1] : null;
        /// <summary>遷移中か</summary>
        public bool IsTransitioning => _transitionInfo != null;

        /// <summary>カレント変更時の通知</summary>
        public Action<Situation> ChangedCurrentEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="label">デバッグ等で利用するラベル</param>
        /// <param name="caller">自動解決用呼び出し元設定変数</param>
        public SituationContainer(string label = "", [CallerFilePath] string caller = "") {
            _label = string.IsNullOrEmpty(label) ? caller : label;
            SituationMonitor.AddContainer(this);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            SituationMonitor.RemoveContainer(this);
            Clear();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Setup(Situation rootSituation) {
            Clear();

            if (rootSituation == null || !rootSituation.IsRoot) {
                Debug.LogError($"Invalid root situation. {rootSituation}");
                return;
            }

            RootSituation = rootSituation;
            ((ISituation)rootSituation).Standby(this);
        }

        /// <summary>
        /// 現在のシチュエーションを再構築する(遷移オプション使用不可 = クロス系の同時にライフサイクルが存在する物は使用不可)
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Reset(Action<Situation> onSetup, params ITransitionEffect[] effects) {
            if (Current == null) {
                return new TransitionHandle(new Exception("Current situation is null"));
            }

            var situationName = Current.GetType().Name;

            if (IsTransitioning) {
                return new TransitionHandle(new Exception($"In transit other. Situation:{situationName}"));
            }

            // 閉じるSituationのリスト化
            var prevSituations = new List<ISituation>();
            var situation = (ISituation)Current;
            while (situation != null) {
                prevSituations.Add(situation);
                situation = (ISituation)situation.Parent;
            }

            // 開くSituationのリスト化
            var nextSituations = new List<ISituation>();
            for (var i = prevSituations.Count - 1; i >= 0; i--) {
                nextSituations.Add(prevSituations[i]);
            }

            // 遷移はOutIn専用
            var transition = (ITransition)new OutInTransition();

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                TransitionType = TransitionType.Forward,
                PrevSituations = prevSituations,
                NextSituations = nextSituations,
                State = TransitionState.Standby,
                Step = TransitionStep.Complete,
                Effects = effects
            };

            // 初期化通知
            onSetup?.Invoke(Current);

            // コルーチンの登録
            _coroutineRunner.StartCoroutine(transition.TransitRoutine(this), () => { _transitionInfo = null; });

            // ハンドルの返却
            return new TransitionHandle(_transitionInfo);
        }

        /// <summary>
        /// 現在のシチュエーションを再構築する(遷移オプション使用不可 = クロス系の同時にライフサイクルが存在する物は使用不可)
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Reset(params ITransitionEffect[] effects) {
            return Reset(null, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="situationType">遷移予定のSituationの型</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="option">遷移オプション</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition(Type situationType, Action<Situation> onSetup, TransitionOption option, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            var situation = FindSituation(situationType);
            if (situation == null) {
                return new TransitionHandle(new Exception($"Not found situation:{situationType.Name}"));
            }

            var nextName = situation.GetType().Name;

            if (IsTransitioning) {
                return new TransitionHandle(new Exception($"In transit other. Situation:{nextName}"));
            }

            var prev = (ISituation)Current;
            var next = (ISituation)situation;

            // 遷移の必要がなければキャンセル扱い
            if (prev == next) {
                return new TransitionHandle(new Exception($"Cancel transit. Situation:{nextName}"));
            }

            // 遷移先の共通親を探す
            var baseParent = default(ISituation);
            if (option == null || !option.Refresh) {
                baseParent = prev;
                while (baseParent != null) {
                    var p = next;
                    while (p != null) {
                        if (p == baseParent) {
                            break;
                        }

                        p = p.Parent;
                    }

                    if (p != null) {
                        break;
                    }

                    baseParent = baseParent.Parent;
                }
            }

            // 閉じるSituationリスト
            var prevSituations = new List<ISituation>();
            if (prev != null) {
                var p = prev;
                while (p != baseParent) {
                    prevSituations.Add(p);
                    p = p.Parent;
                }
            }

            // 開くSituationリスト
            var nextSituations = new List<ISituation>();
            {
                var p = next;
                while (p != baseParent) {
                    nextSituations.Insert(0, p);
                    p = p.Parent;
                }
            }

            // 遷移情報の取得
            var transition = overrideTransition ?? GetDefaultTransition(next);

            // 遷移可能チェック
            if (!CheckTransition(prevSituations, nextSituations, transition)) {
                return new TransitionHandle(
                    new Exception($"Cant transition. Situation:{nextName} Transition:{transition}"));
            }

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                TransitionType = (option != null && option.Back) ? TransitionType.Back : TransitionType.Forward,
                PrevSituations = prevSituations,
                NextSituations = nextSituations,
                State = TransitionState.Standby,
                Step = option != null ? option.Step : TransitionStep.Complete,
                Effects = effects
            };

            // コルーチンの登録
            _coroutineRunner.StartCoroutine(TransitionRoutine(next, onSetup, transition), () => _transitionInfo = null);

            // ハンドルの返却
            return new TransitionHandle(_transitionInfo);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="option">遷移オプション</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition<TSituation>(Action<TSituation> onSetup, TransitionOption option, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition(typeof(TSituation), s => onSetup?.Invoke((TSituation)s), option, overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition<TSituation>(ITransition overrideTransition, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(null, null, overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="option">遷移オプション</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition<TSituation>(TransitionOption option, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(null, option, null, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition<TSituation>(params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(null, null, null, effects);
        }

        /// <summary>
        /// シチュエーションのプリロード
        /// </summary>
        public AsyncOperationHandle PreLoadAsync<TSituation>()
            where TSituation : Situation {
            return PreLoadAsync(typeof(TSituation));
        }

        /// <summary>
        /// シチュエーションのプリロード
        /// </summary>
        public AsyncOperationHandle PreLoadAsync(Type situationType) {
            var situation = FindSituation(situationType);
            if (situation == null) {
                return AsyncOperationHandle.CanceledHandle;
            }

            var target = (ISituation)situation;
            if (!target.CanPreLoad) {
                Debug.LogWarning($"{situationType.Name} is not support preLoad.");
                return AsyncOperationHandle.CanceledHandle;
            }

            var asyncOp = new AsyncOperator();
            if (target.PreLoadState == PreLoadState.None) {
                _preloadSituations.Add(situation);
                _coroutineRunner.StartCoroutine(target.PreLoadRoutine(), () => { asyncOp.Completed(); }, () => { asyncOp.Aborted(); }, ex => { asyncOp.Aborted(ex); });
            }
            else {
                asyncOp.Completed();
            }

            return asyncOp;
        }

        /// <summary>
        /// シチュエーションのプリロード解除
        /// </summary>
        public void UnPreLoad<TSituation>() {
            UnPreLoad(typeof(TSituation));
        }

        /// <summary>
        /// シチュエーションのプリロード解除
        /// </summary>
        public void UnPreLoad(Type situationType) {
            var situation = FindSituation(situationType);
            if (situation == null) {
                return;
            }

            if (!situation.CanPreLoad) {
                Debug.LogWarning($"{situationType.Name} is not support preLoad.");
                return;
            }

            var target = (ISituation)situation;
            if (situation.PreLoadState != PreLoadState.None) {
                target.UnPreLoad();
                _preloadSituations.Remove(situation);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            // コルーチン更新
            _coroutineRunner.Update();

            if (IsTransitioning) {
                // 遷移中のシチュエーション更新
                foreach (var situation in _transitionInfo.PrevSituations) {
                    situation.Update();
                }

                foreach (var situation in _transitionInfo.NextSituations) {
                    situation.Update();
                }

                // エフェクト更新
                if (_transitionInfo.EffectActive) {
                    for (var i = 0; i < _transitionInfo.Effects.Count; i++) {
                        _transitionInfo.Effects[i].Update();
                    }
                }
            }
            // 現在有効なSituationの更新
            else {
                foreach (var situation in _runningSituations) {
                    situation.Update();
                }
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            // 遷移中のシチュエーション更新
            if (IsTransitioning) {
                // 遷移中のシチュエーション更新
                foreach (var situation in _transitionInfo.PrevSituations) {
                    situation.LateUpdate();
                }

                foreach (var situation in _transitionInfo.NextSituations) {
                    situation.LateUpdate();
                }
            }
            // 現在有効なSituationの更新
            else {
                foreach (var situation in _runningSituations) {
                    situation.LateUpdate();
                }
            }
        }

        /// <summary>
        /// 物理更新処理
        /// </summary>
        public void FixedUpdate() {
            // 遷移中のシチュエーション更新
            if (IsTransitioning) {
                // 遷移中のシチュエーション更新
                foreach (var situation in _transitionInfo.PrevSituations) {
                    situation.FixedUpdate();
                }

                foreach (var situation in _transitionInfo.NextSituations) {
                    situation.FixedUpdate();
                }
            }
            // 現在有効なSituationの更新
            else {
                foreach (var situation in _runningSituations) {
                    situation.FixedUpdate();
                }
            }
        }

        /// <summary>
        /// 中身のクリア
        /// </summary>
        public void Clear() {
            // PreLoad/PreRegister毎解放する
            void ForceRelease(ISituation situation) {
                foreach (var child in situation.Children) {
                    ForceRelease(child);
                }

                situation.UnPreLoad();
                situation.Release(this);
            }

            // PreLoad状態の物をUnPreLoad
            var preloadSituations = _preloadSituations.ToArray();
            foreach (var situation in preloadSituations) {
                UnPreLoad(situation.GetType());
            }

            // 有効なSituationを無くす
            _runningSituations.Clear();
            ChangedCurrentEvent?.Invoke(Current);

            // Coroutineキャンセル
            _coroutineRunner.StopAllCoroutines();
            _transitionInfo = null;

            // 全SituationのRelease
            if (RootSituation != null) {
                ForceRelease(RootSituation);
                RootSituation = null;
            }
        }

        /// <summary>
        /// 該当型のSituationを探す
        /// </summary>
        public TSituation FindSituation<TSituation>()
            where TSituation : Situation {
            return (TSituation)FindSituation(typeof(TSituation));
        }

        /// <summary>
        /// 該当型のSituationを探す
        /// </summary>
        public Situation FindSituation(Type type) {
            ISituation Find(ISituation situation) {
                if (situation.GetType() == type) {
                    return situation;
                }

                foreach (var child in situation.Children) {
                    var result = Find(child);
                    if (result == null) {
                        continue;
                    }

                    return result;
                }

                return null;
            }

            if (RootSituation == null) {
                Debug.LogError("RootSituation is null.");
                return null;
            }

            return (Situation)Find(RootSituation);
        }

        /// <summary>
        /// 遷移ルーチン
        /// </summary>
        private IEnumerator TransitionRoutine(ISituation nextSituation, Action<Situation> onSetup, ITransition transition) {
            // アクティブなSituationの更新
            _runningSituations.Clear();
            {
                var s = nextSituation;
                while (s != null) {
                    _runningSituations.Insert(0, s);
                    s = s.Parent;
                }
            }
            ChangedCurrentEvent?.Invoke(Current);
            
            // 初期化処理
            onSetup?.Invoke(Current);

            yield return transition.TransitRoutine(this);
        }

        /// <summary>
        /// 遷移チェック
        /// </summary>
        private bool CheckTransition(IReadOnlyList<ISituation> prevSituations, IReadOnlyList<ISituation> nextSituations, ITransition transition) {
            if (transition == null) {
                return false;
            }

            // 空への遷移 or 空からの遷移は常に許可
            if (prevSituations.Count <= 0 || nextSituations.Count <= 0) {
                return true;
            }

            // SceneSituationが介在するかチェック
            var sceneSituationTransition = false;
            foreach (var situation in prevSituations) {
                if (situation is SceneSituation) {
                    sceneSituationTransition = true;
                    break;
                }
            }

            foreach (var situation in nextSituations) {
                if (situation is SceneSituation) {
                    sceneSituationTransition = true;
                    break;
                }
            }

            // SceneSituationが介在する場合、OutInTransitionのみ許可
            if (sceneSituationTransition) {
                if (transition is not OutInTransition) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 遷移チェック
        /// </summary>
        private ITransition GetDefaultTransition(ISituation nextSituation) {
            if (nextSituation == null) {
                return new OutInTransition();
            }

            return nextSituation.GetDefaultNextTransition();
        }

        /// <summary>
        /// 遷移開始
        /// </summary>
        void ITransitionResolver.Start() {
            _transitionInfo.State = TransitionState.Standby;
            foreach (var effect in _transitionInfo.Effects) {
                effect.Begin();
            }
        }

        /// <summary>
        /// エフェクト開始コルーチン
        /// </summary>
        IEnumerator ITransitionResolver.EnterEffectRoutine() {
            yield return new MergedCoroutine(_transitionInfo.Effects.Select(x => x.EnterRoutine()).ToArray());
            _transitionInfo.EffectActive = true;
        }

        /// <summary>
        /// エフェクト終了コルーチン
        /// </summary>
        IEnumerator ITransitionResolver.ExitEffectRoutine() {
            _transitionInfo.EffectActive = false;
            yield return new MergedCoroutine(_transitionInfo.Effects.Select(x => x.ExitRoutine()).ToArray());
        }

        /// <summary>
        /// ディアクティベート
        /// </summary>
        void ITransitionResolver.DeactivatePrev() {
            var handle = new TransitionHandle(_transitionInfo);
            for (var i = 0; i < _transitionInfo.PrevSituations.Count; i++) {
                _transitionInfo.PrevSituations[i].Deactivate(handle);
            }
        }

        /// <summary>
        /// 閉じるコルーチン
        /// </summary>
        IEnumerator ITransitionResolver.ClosePrevRoutine(bool immediate) {
            var handle = new TransitionHandle(_transitionInfo);
            for (var i = 0; i < _transitionInfo.PrevSituations.Count; i++) {
                _transitionInfo.PrevSituations[i].PreClose(handle);
            }

            if (!immediate) {
                var routines = new List<IEnumerator>();
                for (var i = 0; i < _transitionInfo.PrevSituations.Count; i++) {
                    routines.Add(_transitionInfo.PrevSituations[i].CloseRoutine(handle));
                }

                yield return new MergedCoroutine(routines);
            }

            for (var i = 0; i < _transitionInfo.PrevSituations.Count; i++) {
                _transitionInfo.PrevSituations[i].PostClose(handle);
            }
        }

        /// <summary>
        /// 解放コルーチン
        /// </summary>
        IEnumerator ITransitionResolver.UnloadPrevRoutine() {
            var handle = new TransitionHandle(_transitionInfo);
            for (var i = 0; i < _transitionInfo.PrevSituations.Count; i++) {
                _transitionInfo.PrevSituations[i].Cleanup(handle);
            }

            for (var i = 0; i < _transitionInfo.PrevSituations.Count; i++) {
                _transitionInfo.PrevSituations[i].Unload(handle);
            }

            yield return null;
        }

        /// <summary>
        /// 読み込みコルーチン
        /// </summary>
        IEnumerator ITransitionResolver.LoadNextRoutine() {
            _transitionInfo.State = TransitionState.Initializing;

            var handle = new TransitionHandle(_transitionInfo);
            var routines = new List<IEnumerator>();
            for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                routines.Add(_transitionInfo.NextSituations[i].LoadRoutine(handle, false));
            }

            yield return new MergedCoroutine(routines);

            while (_transitionInfo.Step <= TransitionStep.Load) {
                yield return null;
            }

            for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                yield return _transitionInfo.NextSituations[i].SetupRoutine(handle);
            }

            while (_transitionInfo.Step <= TransitionStep.Setup) {
                yield return null;
            }
        }

        /// <summary>
        /// 開くコルーチン
        /// </summary>
        IEnumerator ITransitionResolver.OpenNextRoutine(bool immediate) {
            _transitionInfo.State = TransitionState.Opening;

            var handle = new TransitionHandle(_transitionInfo);
            for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                _transitionInfo.NextSituations[i].PreOpen(handle);
            }

            if (!immediate) {
                var routines = new List<IEnumerator>();
                for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                    routines.Add(_transitionInfo.NextSituations[i].OpenRoutine(handle));
                }

                yield return new MergedCoroutine(routines);
            }

            for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                _transitionInfo.NextSituations[i].PostOpen(handle);
            }
        }

        /// <summary>
        /// アクティベート
        /// </summary>
        void ITransitionResolver.ActivateNext() {
            var handle = new TransitionHandle(_transitionInfo);
            for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                _transitionInfo.NextSituations[i].Activate(handle);
            }
        }

        /// <summary>
        /// 遷移完了
        /// </summary>
        void ITransitionResolver.Finish() {
            foreach (var effect in _transitionInfo.Effects) {
                effect.End();
            }

            _transitionInfo.State = TransitionState.Completed;
        }
    }
}