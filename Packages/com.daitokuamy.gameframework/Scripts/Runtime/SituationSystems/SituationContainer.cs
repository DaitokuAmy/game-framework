using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション管理用クラス
    /// </summary>
    public sealed class SituationContainer : ITransitionResolver, IStateContainer<Type, Situation, SituationContainer.TransitionOption>, IMonitoredStateContainer {
        /// <summary>
        /// 遷移オプション
        /// </summary>
        public class TransitionOption {
            /// <summary>Rootから再構築して遷移するか</summary>
            public bool Refresh = false;
        }

        /// <summary>
        /// 遷移情報
        /// </summary>
        internal class TransitionInfo : ITransitionInfo<Situation> {
            public IReadOnlyList<ISituation> PrevSituations { get; set; }
            public IReadOnlyList<ISituation> NextSituations { get; set; }
            public IReadOnlyList<ITransitionEffect> Effects { get; set; }
            public bool EffectActive { get; set; }

            public TransitionDirection Direction { get; set; }
            public TransitionState State { get; set; }
            public TransitionStep EndStep { get; set; }
            public Situation Prev => PrevSituations.FirstOrDefault() as Situation;
            public Situation Next => NextSituations.LastOrDefault() as Situation;

            /// <inheritdoc/>
            public event Action<Situation> FinishedEvent;

            /// <inheritdoc/>
            public bool ChangeEndStep(TransitionStep step) {
                if (step <= EndStep) {
                    return false;
                }

                EndStep = step;
                return true;
            }

            public void SendFinish() {
                FinishedEvent?.Invoke(Next);
                FinishedEvent = null;
            }
        }

        private readonly CoroutineRunner _coroutineRunner = new();
        private readonly List<Situation> _preloadSituations = new();
        private readonly List<ISituation> _runningSituations = new();
        private readonly string _label;

        private TransitionInfo _transitionInfo;
        private bool _disposed;

        /// <inheritdoc/>
        string IMonitoredStateContainer.Label => _label;
        /// <inheritdoc/>
        string IMonitoredStateContainer.CurrentStateInfo => Current != null ? Current.GetType().Name : "None";

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
            _label = string.IsNullOrEmpty(label) ? PathUtility.GetRelativePath(caller) : label;
            StateMonitor.AddContainer(this);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            StateMonitor.RemoveContainer(this);

            Clear();
        }

        /// <summary>
        /// 遷移情報の取得
        /// </summary>
        bool IMonitoredStateContainer.TryGetTransitionInfo(out IMonitoredStateContainer.TransitionInfo info) {
            if (_transitionInfo == null) {
                info = default;
                return false;
            }

            info.State = _transitionInfo.State;
            info.Direction = _transitionInfo.Direction;
            info.EndStep = _transitionInfo.EndStep;
            info.PrevStateInfo = string.Join('\n', _transitionInfo.PrevSituations
                .Select(x => x.GetType().Name));
            info.NextStateInfo = string.Join('\n', _transitionInfo.NextSituations
                .Select(x => x.GetType().Name));
            return true;
        }

        /// <summary>
        /// 詳細情報取得
        /// </summary>
        void IMonitoredStateContainer.GetDetails(List<(string label, string text)> lines) {
            lines.Add(("Root", RootSituation?.GetType().Name ?? "None"));
            for (var i = 0; i < _runningSituations.Count; i++) {
                var situation = _runningSituations[i];
                lines.Add((i == 0 ? "Running Situations" : "", $"{situation.GetType().Name}{(situation.IsFocused ? "(Focused)" : "")}"));
            }

            for (var i = 0; i < _preloadSituations.Count; i++) {
                lines.Add((i == 0 ? "Preload Situations" : "", _preloadSituations[i].GetType().Name));
            }
        }

        /// <inheritdoc/>
        Situation IStateContainer<Type, Situation, TransitionOption>.FindState(Type key) {
            Situation Find(Situation situation, Type targetType) {
                if (situation == null) {
                    return null;
                }

                if (situation.GetType() == targetType) {
                    return situation;
                }

                foreach (var child in situation.Children) {
                    var result = Find(child, targetType);
                    if (result != null) {
                        return result;
                    }
                }

                return null;
            }

            return Find(RootSituation, key);
        }

        /// <inheritdoc/>
        Situation[] IStateContainer<Type, Situation, TransitionOption>.GetStates() {
            var result = new List<Situation>();

            void Add(Situation situation, List<Situation> list) {
                if (situation == null) {
                    return;
                }

                list.Add(situation);
                list.AddRange(situation.Children);
            }

            Add(RootSituation, result);
            return result.ToArray();
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

        /// <inheritdoc/>
        public TransitionHandle<Situation> Transition(Type key, TransitionOption option, bool back, TransitionStep endStep, Action<Situation> setupAction, ITransition transition, params ITransitionEffect[] effects) {
            var situation = FindSituation(key);
            if (situation == null) {
                return new TransitionHandle<Situation>(new Exception($"Not found situation:{key.Name}"));
            }

            var nextName = situation.GetType().Name;

            if (IsTransitioning) {
                return new TransitionHandle<Situation>(new Exception($"In transit other. Situation:{nextName}"));
            }

            var prev = (ISituation)Current;
            var next = (ISituation)situation;

            // 遷移の必要がなければキャンセル扱い
            if (prev == next) {
                return TransitionHandle<Situation>.Empty;
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
            transition ??= GetDefaultTransition(next);

            // 遷移可能チェック
            if (!CheckTransition(prevSituations, nextSituations, transition)) {
                return new TransitionHandle<Situation>(
                    new Exception($"Cant transition. Situation:{nextName} Transition:{transition}"));
            }

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                Direction = back ? TransitionDirection.Back : TransitionDirection.Forward,
                PrevSituations = prevSituations,
                NextSituations = nextSituations,
                State = TransitionState.Standby,
                EndStep = endStep,
                Effects = effects
            };

            // コルーチンの登録
            _coroutineRunner.StartCoroutine(TransitionRoutine(next, setupAction, transition), () => _transitionInfo = null);

            // ハンドルの返却
            return new TransitionHandle<Situation>(_transitionInfo);
        }

        /// <inheritdoc/>
        public TransitionHandle<Situation> Reset(Action<Situation> setupAction, params ITransitionEffect[] effects) {
            if (Current == null) {
                return new TransitionHandle<Situation>(new Exception("Current situation is null"));
            }

            var situationName = Current.GetType().Name;

            if (IsTransitioning) {
                return new TransitionHandle<Situation>(new Exception($"In transit other. Situation:{situationName}"));
            }

            // 閉じるSituationのリスト化
            var prevSituations = new List<ISituation>();
            var situation = (ISituation)Current;
            while (situation != null) {
                prevSituations.Add(situation);
                situation = situation.Parent;
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
                Direction = TransitionDirection.Forward,
                PrevSituations = prevSituations,
                NextSituations = nextSituations,
                State = TransitionState.Standby,
                EndStep = TransitionStep.Complete,
                Effects = effects
            };

            // 初期化通知
            setupAction?.Invoke(Current);

            // コルーチンの登録
            _coroutineRunner.StartCoroutine(transition.TransitionRoutine(this), () => { _transitionInfo = null; });

            // ハンドルの返却
            return new TransitionHandle<Situation>(_transitionInfo);
        }

        /// <summary>
        /// 現在のシチュエーションを再構築する(遷移オプション使用不可 = クロス系の同時にライフサイクルが存在する物は使用不可)
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<Situation> Reset(params ITransitionEffect[] effects) {
            return Reset(null, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="back">戻り遷移か</param>
        /// <param name="endStep">終了ステップ</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<Situation> Transition<TSituation>(TransitionOption option, bool back, TransitionStep endStep, Action<TSituation> setupAction, ITransition transition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition(typeof(TSituation), option, back, endStep, s => setupAction?.Invoke((TSituation)s), transition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="endStep">終了ステップ</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<Situation> Transition<TSituation>(TransitionOption option, TransitionStep endStep, Action<TSituation> setupAction, ITransition transition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition(option, true, endStep, setupAction, transition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<Situation> Transition<TSituation>(Action<TSituation> setupAction, ITransition transition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition(null, TransitionStep.Complete, setupAction, transition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="transition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<Situation> Transition<TSituation>(ITransition transition, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(null, transition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="option">遷移オプション</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<Situation> Transition<TSituation>(TransitionOption option, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(option, true, TransitionStep.Complete, null, null, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<Situation> Transition<TSituation>(params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(null, null, effects);
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
        private IEnumerator TransitionRoutine(ISituation nextSituation, Action<Situation> setupAction, ITransition transition) {
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
            setupAction?.Invoke(Current);

            yield return transition.TransitionRoutine(this);
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
                effect.BeginTransition();
            }
            
            // 遷移開始時に全部のフォーカスを外す
            foreach (var situation in _transitionInfo.PrevSituations) {
                situation.SetFocus(false);
            }
            
            foreach (var situation in _runningSituations) {
                situation.SetFocus(false);
            }
        }

        /// <summary>
        /// エフェクト開始コルーチン
        /// </summary>
        IEnumerator ITransitionResolver.EnterEffectRoutine() {
            yield return new MergedCoroutine(_transitionInfo.Effects.Select(x => x.EnterEffectRoutine()).ToArray());
            _transitionInfo.EffectActive = true;
        }

        /// <summary>
        /// エフェクト終了コルーチン
        /// </summary>
        IEnumerator ITransitionResolver.ExitEffectRoutine() {
            _transitionInfo.EffectActive = false;
            yield return new MergedCoroutine(_transitionInfo.Effects.Select(x => x.ExitEffectRoutine()).ToArray());
        }

        /// <summary>
        /// ディアクティベート
        /// </summary>
        void ITransitionResolver.DeactivatePrev() {
            var handle = new TransitionHandle<Situation>(_transitionInfo);
            for (var i = 0; i < _transitionInfo.PrevSituations.Count; i++) {
                _transitionInfo.PrevSituations[i].Deactivate(handle);
            }
        }

        /// <summary>
        /// 閉じるコルーチン
        /// </summary>
        IEnumerator ITransitionResolver.ClosePrevRoutine(bool immediate) {
            var handle = new TransitionHandle<Situation>(_transitionInfo);
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
            var handle = new TransitionHandle<Situation>(_transitionInfo);
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

            var handle = new TransitionHandle<Situation>(_transitionInfo);
            var routines = new List<IEnumerator>();
            for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                routines.Add(_transitionInfo.NextSituations[i].LoadRoutine(handle, false));
            }

            yield return new MergedCoroutine(routines);

            while (_transitionInfo.EndStep <= TransitionStep.Load) {
                yield return null;
            }

            for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                yield return _transitionInfo.NextSituations[i].SetupRoutine(handle);
            }

            while (_transitionInfo.EndStep <= TransitionStep.Setup) {
                yield return null;
            }
        }

        /// <summary>
        /// 開くコルーチン
        /// </summary>
        IEnumerator ITransitionResolver.OpenNextRoutine(bool immediate) {
            _transitionInfo.State = TransitionState.Opening;

            var handle = new TransitionHandle<Situation>(_transitionInfo);
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
            var handle = new TransitionHandle<Situation>(_transitionInfo);
            for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                _transitionInfo.NextSituations[i].Activate(handle);
            }
        }

        /// <summary>
        /// 遷移完了
        /// </summary>
        void ITransitionResolver.Finish() {
            foreach (var effect in _transitionInfo.Effects) {
                effect.EndTransition();
            }
            
            // 遷移開始時に戦闘にいる物にフォーカスを充てる
            if (_runningSituations.Count > 0) {
                _runningSituations[^1].SetFocus(true);
            }

            _transitionInfo.State = TransitionState.Completed;
            _transitionInfo.SendFinish();
        }
    }
}