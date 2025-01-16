using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション管理用クラス
    /// </summary>
    public sealed class SituationContainer : IDisposable, ITransitionResolver {
        /// <summary>
        /// 遷移オプション
        /// </summary>
        public class TransitionOption {
            /// <summary>強制バック遷移</summary>
            public bool ForceBack = false;
        }

        /// <summary>
        /// 遷移情報
        /// </summary>
        public class TransitionInfo {
            public SituationContainer Container;
            public IReadOnlyList<ISituation> PrevSituations;
            public IReadOnlyList<ISituation> NextSituations;
            public TransitionState State;
            public bool Back;
            public ITransitionEffect[] Effects = new ITransitionEffect[0];
            public bool EffectActive;
        }

        // コルーチン実行用
        private readonly CoroutineRunner _coroutineRunner = new();
        // プリロードしているSituationリスト
        private readonly List<Situation> _preloadSituations = new();

        // 遷移中情報
        private TransitionInfo _transitionInfo;

        /// <summary>RootとなるSituation</summary>
        public Situation RootSituation { get; private set; }
        /// <summary>現在のシチュエーション</summary>
        public Situation Current { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationContainer(Situation rootSituation) {
            if (rootSituation == null || rootSituation.Parent != null) {
                Debug.LogError($"Invalid root situation. {rootSituation}");
                return;
            }

            RootSituation = rootSituation;
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

            if (_transitionInfo != null) {
                return new TransitionHandle(new Exception($"In transit other. Situation:{situationName}"));
            }

            // 閉じるSituationのリスト化
            var prevSituations = new List<Situation>();
            var situation = Current;
            while (situation != null) {
                prevSituations.Add(situation);
                situation = situation.Parent;
            }

            // 開くSituationのリスト化
            var nextSituations = new List<Situation>();
            for (var i = prevSituations.Count - 1; i >= 0; i--) {
                nextSituations.Add(prevSituations[i]);
            }

            // 遷移はOutIn前提
            var transition = (ITransition)new OutInTransition();

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                Container = this,
                Back = false,
                PrevSituations = prevSituations,
                NextSituations = nextSituations,
                State = TransitionState.Standby,
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
        /// <param name="onSetup">初期化処理</param>
        /// <param name="option">遷移オプション</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition<TSituation>(Action<TSituation> onSetup, TransitionOption option, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            var situation = FindLeafSituation<TSituation>();
            if (situation == null) {
                return new TransitionHandle(new Exception($"Not found situation:{typeof(TSituation).Name}"));
            }

            var nextName = situation.GetType().Name;

            if (_transitionInfo != null) {
                return new TransitionHandle(new Exception($"In transit other. Situation:{nextName}"));
            }

            var prev = (ISituation)Current;
            var next = (ISituation)situation;

            // 遷移の必要がなければキャンセル扱い
            if (prev == next) {
                return new TransitionHandle(new Exception($"Cancel transit. Situation:{nextName}"));
            }

            // 遷移先の共通親を探す
            var baseParent = Current?.Parent;
            while (baseParent != null) {
                var p = situation.Parent;
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

            // 閉じるSituationリスト
            var prevSituations = new List<Situation>();
            if (Current != null) {
                prevSituations.Add(Current);
                var p = Current.Parent;
                while (p != baseParent) {
                    prevSituations.Add(p);
                    p = p.Parent;
                }
            }

            // 開くSituationリスト
            var nextSituations = new List<Situation>();
            {
                nextSituations.Insert(0, situation);
                var p = situation.Parent;
                while (p != baseParent) {
                    nextSituations.Insert(0, p);
                    p = p.Parent;
                }
            }

            // 遷移情報の取得
            var transition = overrideTransition ?? GetDefaultTransition(next);

            // 遷移可能チェック
            if (!CheckTransition(next, transition)) {
                return new TransitionHandle(
                    new Exception($"Cant transition. Situation:{nextName} Transition:{transition}"));
            }

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                Container = this,
                Back = option != null && option.ForceBack,
                PrevSituations = prevSituations,
                NextSituations = nextSituations,
                State = TransitionState.Standby,
                Effects = effects
            };

            // コルーチンの登録
            _coroutineRunner.StartCoroutine(transition.TransitRoutine(this), () => _transitionInfo = null);

            // ハンドルの返却
            return new TransitionHandle(_transitionInfo);
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
        /// <param name="situation">プリロード対象のSituation</param>
        public AsyncOperationHandle PreLoadAsync(Situation situation) {
            var target = (ISituation)situation;
            var asyncOp = new AsyncOperator();
            if (target.PreLoadState == PreLoadState.None) {
                _preloadSituations.Add(situation);
                target.Standby(this);
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
        /// <param name="situation">プリロード解除対象のSituation</param>
        public void UnPreLoad(Situation situation) {
            var target = (ISituation)situation;
            if (target.PreLoadState != PreLoadState.None) {
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

            if (_transitionInfo != null) {
                // 遷移中のシチュエーション更新
                foreach (var situation in _transitionInfo.PrevSituations) {
                    situation.Update();
                }

                foreach (var situation in _transitionInfo.NextSituations) {
                    situation.Update();
                }

                // エフェクト更新
                if (_transitionInfo.EffectActive) {
                    for (var i = 0; i < _transitionInfo.Effects.Length; i++) {
                        _transitionInfo.Effects[i].Update();
                    }
                }
            }
            // 現在有効なSituationの更新
            else {
                // todo:あとでキャッシュする
                var situations = new List<ISituation>();
                var situation = Current;
                while (situation != null) {
                    situations.Add(situation);
                    situation = situation.Parent;
                }

                for (var i = situations.Count - 1; i >= 0; i--) {
                    situations[i].Update();
                }
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            // 遷移中のシチュエーション更新
            if (_transitionInfo != null) {
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
                // todo:あとでキャッシュする
                var situations = new List<ISituation>();
                var situation = Current;
                while (situation != null) {
                    situations.Add(situation);
                    situation = situation.Parent;
                }

                for (var i = situations.Count - 1; i >= 0; i--) {
                    situations[i].LateUpdate();
                }
            }
        }

        /// <summary>
        /// 物理更新処理
        /// </summary>
        public void FixedUpdate() {
            // 遷移中のシチュエーション更新
            if (_transitionInfo != null) {
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
                // todo:あとでキャッシュする
                var situations = new List<ISituation>();
                var situation = Current;
                while (situation != null) {
                    situations.Add(situation);
                    situation = situation.Parent;
                }

                for (var i = situations.Count - 1; i >= 0; i--) {
                    situations[i].FixedUpdate();
                }
            }
        }

        /// <summary>
        /// 中身のクリア
        /// </summary>
        public void Clear() {
            // PreLoad/PreRegister毎解放する
            void ForceRelease(ISituation situation) {
                situation.UnPreLoad();
                situation.Release(this);
            }

            // PreLoad状態の物をUnPreLoad
            var preloadSituations = _preloadSituations.ToArray();
            foreach (var situation in preloadSituations) {
                UnPreLoad(situation);
            }

            // カレントの階層を全部クリア
            var target = Current;
            while (target != null) {
                ForceRelease(target);
                target = target.Parent;
            }

            _coroutineRunner.StopAllCoroutines();
            _transitionInfo = null;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            Clear();
            RootSituation = null;
        }

        /// <summary>
        /// 遷移チェック
        /// </summary>
        private bool CheckTransition(ISituation nextSituation, ITransition transition) {
            if (transition == null) {
                return false;
            }

            // null遷移は常に許可
            if (nextSituation == null) {
                return true;
            }

            return nextSituation.CheckNextTransition((Situation)nextSituation, transition);
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
        /// 該当型の階層一番下にあるSituationを探す
        /// </summary>
        private Situation FindLeafSituation<T>()
            where T : Situation {
            var type = typeof(T);

            Situation Find(Situation situation) {
                if (situation.Children.Count == 0) {
                    if (situation.GetType() == type) {
                        return situation;
                    }

                    return null;
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

            return Find(RootSituation);
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
                for (var i = 0; i < _transitionInfo.PrevSituations.Count; i++) {
                    yield return _transitionInfo.PrevSituations[i].CloseRoutine(handle);
                }
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

            for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                yield return _transitionInfo.NextSituations[i].SetupRoutine(handle);
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
                for (var i = 0; i < _transitionInfo.NextSituations.Count; i++) {
                    yield return _transitionInfo.NextSituations[i].OpenRoutine(handle);
                }
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