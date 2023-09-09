using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.CoroutineSystems;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション管理用クラス
    /// </summary>
    public class SituationContainer : IDisposable, ITransitionResolver {
        /// <summary>
        /// 遷移オプション
        /// </summary>
        public class TransitionOption {
            /// <summary>スタックをリセットするか</summary>
            public bool resetStack = false;
            /// <summary>強制バック遷移</summary>
            public bool forceBack = false;
        }
        
        /// <summary>
        /// 遷移情報
        /// </summary>
        public class TransitionInfo {
            public SituationContainer container;
            public ISituation prev;
            public ISituation next;
            public TransitionState state;
            public bool back;
            public ITransitionEffect[] effects = new ITransitionEffect[0];
            public bool effectActive;
        }

        // 子シチュエーションスタック
        private List<Situation> _stack = new List<Situation>();
        // 遷移中情報
        private TransitionInfo _transitionInfo;
        // コルーチン実行用
        private CoroutineRunner _coroutineRunner = new CoroutineRunner();

        // 持ち主のSituation
        public Situation Owner { get; private set; }
        // 現在のシチュエーション
        public Situation Current => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="situation">遷移先のシチュエーション(nullの場合、全部閉じる)</param>
        /// <param name="option">遷移オプション</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition(Situation situation, TransitionOption option, ITransition overrideTransition = null,
            params ITransitionEffect[] effects) {
            var nextName = situation != null ? situation.GetType().Name : "null";

            if (_transitionInfo != null) {
                return new TransitionHandle(new Exception($"In transit other. Situation:{nextName}"));
            }

            // 既に同タイプのシチュエーションが登録されている場合、そこにスタックを戻す
            var backIndex = -1;
            var back = false;
            var reset = false;

            if (situation != null) {
                for (var i = 0; i < _stack.Count; i++) {
                    // 同じインスタンスは使いまわす
                    if (_stack[i] == situation) {
                        backIndex = i;
                        back = true;
                        break;
                    }

                    // 同じ型は置き換える
                    if (_stack[i].GetType() == situation.GetType()) {
                        // 同じSituationに遷移しなおす
                        if (i == _stack.Count - 1) {
                            reset = true;
                            break;
                        }
                        else {
                            var old = _stack[i];
                            ((ISituation)old).Release(this);
                            _stack[i] = situation;
                            backIndex = i;
                            back = true;
                        }
                    }
                }
            }
            else {
                back = true;
            }

            var prev = (ISituation)Current;
            var next = (ISituation)situation;

            // 遷移の必要がなければキャンセル扱い
            if (prev == next) {
                return new TransitionHandle(new Exception($"Cancel transit. Situation:{nextName}"));
            }

            // 遷移情報の取得
            var transition = overrideTransition ?? GetDefaultTransition();

            // 遷移可能チェック
            if (!CheckTransition(next, transition)) {
                return new TransitionHandle(
                    new Exception($"Cant transition. Situation:{nextName} Transition:{transition}"));
            }
            
            // Stackのリセット
            if (option != null && option.resetStack) {
                // 1つを残して他はRelease
                for (var i = _stack.Count - 1; i > 0; i--) {
                    ((ISituation)_stack[i]).Release(this);
                    _stack.RemoveAt(i);
                }
                
                // 残った1つもStackからクリア
                _stack.Clear();
            }
            
            // リセットする場合
            if (reset && _stack.Count > 0) {
                // Stackの最後を入れ直す
                _stack[_stack.Count - 1] = situation;
            }
            // 戻る場合
            else if (back && _stack.Count > 0) {
                // 現在のSituationをStackから除外
                _stack.RemoveAt(_stack.Count - 1);

                // 戻り先までの間にあるSituationをリリースして、Stackクリア
                for (var i = _stack.Count - 1; i > backIndex; i--) {
                    ((ISituation)_stack[i]).Release(this);
                    _stack.RemoveAt(i);
                }
            }
            // 進む場合
            else {
                // スタックに登録
                _stack.Add(situation);
            }

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                container = this,
                back = back || (option != null && option.forceBack),
                prev = prev,
                next = next,
                state = TransitionState.Standby,
                effects = effects
            };

            // コルーチンの登録
            _coroutineRunner.StartCoroutine(transition.TransitRoutine(this), () => {
                // リセットか戻る時はここでRelease
                if ((reset || back) && prev != null) {
                    prev.Release(this);
                }

                _transitionInfo = null;
            });

            // スタンバイ状態
            _transitionInfo.next?.Standby(this);

            // ハンドルの返却
            return new TransitionHandle(_transitionInfo);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="situation">遷移先のシチュエーション(nullの場合、全部閉じる)</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition(Situation situation, ITransition overrideTransition, params ITransitionEffect[] effects) {
            return Transition(situation, null, overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="situation">遷移先のシチュエーション(nullの場合、全部閉じる)</param>
        /// <param name="option">遷移オプション</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition(Situation situation, TransitionOption option, params ITransitionEffect[] effects) {
            return Transition(situation, option, null, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="situation">遷移先のシチュエーション(nullの場合、全部閉じる)</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition(Situation situation, params ITransitionEffect[] effects) {
            return Transition(situation, null, null, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="option">遷移オプション</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(TransitionOption option, ITransition overrideTransition, params ITransitionEffect[] effects) {
            if (_stack.Count <= 0) {
                return new TransitionHandle(new Exception("Not found stack."));
            }

            var next = _stack.Count > 1 ? _stack[_stack.Count - 2] : null;
            return Transition(next, option, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(ITransition overrideTransition, params ITransitionEffect[] effects) {
            return Back(null, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="option">遷移オプション</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(TransitionOption option, params ITransitionEffect[] effects) {
            return Back(option, null, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(params ITransitionEffect[] effects) {
            return Back(null, null, effects);
        }

        /// <summary>
        /// シチュエーションのプリロード
        /// </summary>
        /// <param name="situation">プリロード対象のSituation</param>
        public void PreLoad(Situation situation) {
            var target = (ISituation)situation;
            if (!target.PreLoaded) {
                target.Standby(this);
                _coroutineRunner.StartCoroutine(target.PreLoadRoutine());
            }
        }

        /// <summary>
        /// シチュエーションのプリロード解除
        /// </summary>
        /// <param name="situation">プリロード解除対象のSituation</param>
        public void UnPreLoad(Situation situation) {
            var target = (ISituation)situation;
            if (target.PreLoaded) {
                target.UnPreLoad();

                // StackになければReleaseする
                if (!_stack.Contains(situation)) {
                    target.Release(this);
                }
            }
        }

        /// <summary>
        /// シチュエーションの除外
        /// </summary>
        /// <param name="situation">除外対象のSituation</param>
        /// <param name="overrideTransition">戻る遷移が発生した時のための遷移情報</param>
        /// <param name="effects">戻る遷移が発生した時のための遷移演出</param>
        public void Remove(Situation situation, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // Currentだった場合は戻る
            if (Current == situation) {
                Back(null, overrideTransition, effects);
                return;
            }
            
            // Stackから除外
            _stack.Remove(situation);
            
            // リリースする
            ((ISituation)situation).Release(this);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            // コルーチン更新
            _coroutineRunner.Update();

            if (_transitionInfo != null) {
                // 遷移中のシチュエーション更新
                if (_transitionInfo.prev is Situation prev) {
                    prev.Update();
                }

                if (_transitionInfo.next is Situation next) {
                    next.Update();
                }

                // エフェクト更新
                if (_transitionInfo.effectActive) {
                    for (var i = 0; i < _transitionInfo.effects.Length; i++) {
                        _transitionInfo.effects[i].Update();
                    }
                }
            }
            // カレントシチュエーションの更新
            else {
                var current = Current;
                if (current != null) {
                    current.Update();
                }
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            // 遷移中のシチュエーション更新
            if (_transitionInfo != null) {
                if (_transitionInfo.prev is Situation prev) {
                    prev.LateUpdate();
                }

                if (_transitionInfo.next is Situation next) {
                    next.LateUpdate();
                }
            }
            // カレントシチュエーションの更新
            else {
                var current = (ISituation)Current;
                if (current != null) {
                    current.LateUpdate();
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationContainer(Situation owner = null) {
            Owner = owner;

            if (Owner != null) {
                Owner.AddChildContainer(this);
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            DisposeInternal();

            // PreLoad毎解放する
            void ForceRelease(ISituation situation) {
                situation.UnPreLoad();
                situation.Release(this);
            }

            // Stackの中身を全部クリア
            while (_stack.Count > 0) {
                ForceRelease(_stack[_stack.Count - 1]);
                _stack.RemoveAt(_stack.Count - 1);
            }

            _coroutineRunner.Dispose();
            _transitionInfo = null;

            if (Owner != null) {
                Owner.RemoveChildContainer(this);
                Owner = null;
            }
        }

        /// <summary>
        /// 廃棄時処理(Override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// デフォルトの遷移を取得
        /// </summary>
        protected virtual ITransition GetDefaultTransition() {
            return new OutInTransition();
        }

        /// <summary>
        /// 遷移チェック
        /// </summary>
        private bool CheckTransition(ISituation childSituation, ITransition transition) {
            if (transition == null) {
                return false;
            }

            // null遷移は常に許可
            if (childSituation == null) {
                return true;
            }

            return CheckTransitionInternal((Situation)childSituation, transition);
        }

        /// <summary>
        /// 遷移チェック
        /// </summary>
        /// <param name="childSituation">遷移するの子シチュエーション</param>
        /// <param name="transition">遷移処理</param>
        /// <returns>遷移可能か</returns>
        protected virtual bool CheckTransitionInternal(Situation childSituation, ITransition transition) {
            return true;
        }

        /// <summary>
        /// 遷移開始
        /// </summary>
        void ITransitionResolver.Start() {
            _transitionInfo.state = TransitionState.Standby;
        }

        /// <summary>
        /// エフェクト開始コルーチン
        /// </summary>
        IEnumerator ITransitionResolver.EnterEffectRoutine() {
            yield return new MergedCoroutine(_transitionInfo.effects.Select(x => x.EnterRoutine()).ToArray());
            _transitionInfo.effectActive = true;
        }

        /// <summary>
        /// エフェクト終了コルーチン
        /// </summary>
        IEnumerator ITransitionResolver.ExitEffectRoutine() {
            _transitionInfo.effectActive = false;
            yield return new MergedCoroutine(_transitionInfo.effects.Select(x => x.ExitRoutine()).ToArray());
        }

        /// <summary>
        /// ディアクティベート
        /// </summary>
        void ITransitionResolver.DeactivatePrev() {
            if (_transitionInfo.prev == null) {
                return;
            }

            _transitionInfo.prev.Deactivate(new TransitionHandle(_transitionInfo));
        }

        /// <summary>
        /// 閉じるコルーチン
        /// </summary>
        IEnumerator ITransitionResolver.ClosePrevRoutine() {
            if (_transitionInfo.prev == null) {
                yield break;
            }

            yield return _transitionInfo.prev.CloseRoutine(new TransitionHandle(_transitionInfo));
        }

        /// <summary>
        /// 解放コルーチン
        /// </summary>
        IEnumerator ITransitionResolver.UnloadPrevRoutine() {
            if (_transitionInfo.prev == null) {
                yield break;
            }

            var handle = new TransitionHandle(_transitionInfo);
            _transitionInfo.prev.Cleanup(handle);
            _transitionInfo.prev.Unload(handle);
            yield return null;
        }

        /// <summary>
        /// 読み込みコルーチン
        /// </summary>
        IEnumerator ITransitionResolver.LoadNextRoutine() {
            _transitionInfo.state = TransitionState.Initializing;
            if (_transitionInfo.next == null) {
                yield break;
            }

            var handle = new TransitionHandle(_transitionInfo);
            yield return _transitionInfo.next.LoadRoutine(handle);
            yield return _transitionInfo.next.SetupRoutine(handle);
        }

        /// <summary>
        /// 開くコルーチン
        /// </summary>
        IEnumerator ITransitionResolver.OpenNextRoutine() {
            _transitionInfo.state = TransitionState.Opening;
            if (_transitionInfo.next == null) {
                yield break;
            }

            yield return _transitionInfo.next.OpenRoutine(new TransitionHandle(_transitionInfo));
        }

        /// <summary>
        /// アクティベート
        /// </summary>
        void ITransitionResolver.ActivateNext() {
            if (_transitionInfo.next == null) {
                return;
            }

            _transitionInfo.next.Activate(new TransitionHandle(_transitionInfo));
        }

        /// <summary>
        /// 遷移完了
        /// </summary>
        void ITransitionResolver.Finish() {
            _transitionInfo.state = TransitionState.Completed;
        }
    }
}