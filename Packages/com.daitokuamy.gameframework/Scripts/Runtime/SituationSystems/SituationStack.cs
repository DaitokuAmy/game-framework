using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// Situationスタック遷移
    /// </summary>
    public class SituationStack : ISituationFlow {
        private readonly SituationContainer _situationContainer;
        private readonly List<Situation> _stack = new();
        private readonly string _label;

        private bool _disposed;

        /// <inheritdoc/>
        string IMonitoredFlow.Label => _label;
        /// <inheritdoc/>
        Situation IMonitoredFlow.BackTarget {
            get {
                if (_stack.Count <= 1) {
                    return null;
                }

                return _stack[^2];
            }
        }

        /// <inheritdoc/>
        public Situation Current => _stack.LastOrDefault();
        /// <inheritdoc/>
        public bool IsTransitioning => _situationContainer.IsTransitioning;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationStack(SituationContainer container, string label = "", [CallerFilePath] string caller = "") {
            _label = string.IsNullOrEmpty(label) ? caller : label;
            SituationMonitor.AddFlow(this);
            _situationContainer = container;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            SituationMonitor.RemoveFlow(this);
            _stack.Clear();
        }

        /// <inheritdoc/>
        void IMonitoredFlow.GetDetails(List<(string label, string text)> lines) {
            // Stack情報の返却
            for (var i = 0; i < _stack.Count; i++) {
                lines.Add((i == 0 ? "Stack": "", _stack[i].GetType().Name));
            }
        }

        /// <inheritdoc/>
        public Type[] GetSituations() {
            var types = new List<Type>();

            void AddType(Situation situation, List<Type> list) {
                if (situation == null) {
                    return;
                }

                list.Add(situation.GetType());
                foreach (var child in situation.Children) {
                    AddType(child, list);
                }
            }

            AddType(_situationContainer.RootSituation, types);
            return types.ToArray();
        }

        /// <inheritdoc/>
        public TransitionHandle Transition<TSituation>(params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(null, null, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Transition<TSituation>(ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(null, overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Transition<TSituation>(Action<TSituation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            var type = typeof(TSituation);
            return Transition(type, situation => onSetup?.Invoke((TSituation)situation), overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Transition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 同じ型なら何もしない
            if (Current != null && Current.GetType() == type) {
                return TransitionHandle.Empty;
            }

            // 遷移先の取得
            var next = _situationContainer.FindSituation(type);

            if (next == null) {
                return new TransitionHandle(new Exception($"Not found situation tree node. [{type.Name}]"));
            }

            return TransitionInternal(next, false, onSetup, overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle RefreshTransition<TSituation>(params ITransitionEffect[] effects)
            where TSituation : Situation {
            return RefreshTransition<TSituation>(null, null, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle RefreshTransition<TSituation>(ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return RefreshTransition<TSituation>(null, overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle RefreshTransition<TSituation>(Action<TSituation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            var type = typeof(TSituation);
            return RefreshTransition(type, situation => onSetup?.Invoke((TSituation)situation), overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle RefreshTransition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 同じ型なら何もしない
            if (Current != null && Current.GetType() == type) {
                return TransitionHandle.Empty;
            }

            // 遷移先の取得
            var next = _situationContainer.FindSituation(type);

            if (next == null) {
                return new TransitionHandle(new Exception($"Not found situation tree node. [{type.Name}]"));
            }

            return TransitionInternal(next, true, onSetup, overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Back(int depth, params ITransitionEffect[] effects) {
            return Back(depth, null, null, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Back(params ITransitionEffect[] effects) {
            return Back(null, null, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Back(int depth, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return Back(depth, null, overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Back(ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return Back(null, overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Back(Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return Back(1, onSetup, overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Back(int depth, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return BackInternal(depth, onSetup, overrideTransition, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Reset(params ITransitionEffect[] effects) {
            return Reset(null, effects);
        }

        /// <inheritdoc/>
        public TransitionHandle Reset(Action<Situation> onSetup = null, params ITransitionEffect[] effects) {
            return ResetInternal(onSetup, effects);
        }

        /// <summary>
        /// スタックのクリア
        /// </summary>
        public void ClearStack() {
            _stack.Clear();
        }

        /// <summary>
        /// 接続可能なSituationを探す
        /// </summary>
        internal Situation FindSituation(Type type) {
            return _situationContainer.FindSituation(type);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="next">遷移先のSituation</param>
        /// <param name="refresh">RootSituationから再構築するか</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        private TransitionHandle TransitionInternal(Situation next, bool refresh = false, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                return new TransitionHandle(new Exception("In transitioning"));
            }

            // 同じ場所なら何もしない
            if (next == Current) {
                return TransitionHandle.Empty;
            }

            // NextNodeがない
            if (next == null) {
                return new TransitionHandle(new Exception("Next node is null."));
            }

            // スタックの中に含まれていたらそこまでスタックを戻す
            var foundIndex = _stack.IndexOf(next);
            if (foundIndex >= 0) {
                _stack.RemoveRange(foundIndex, _stack.Count - foundIndex);
            }

            // スタックに追加
            _stack.Add(next);

            // 遷移実行
            var option = new SituationContainer.TransitionOption();
            option.Refresh = refresh;
            return _situationContainer.Transition(next.GetType(), onSetup, option, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="depth">何階層戻るか</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        private TransitionHandle BackInternal(int depth, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 階層が無効なら無視
            if (depth <= 0) {
                return TransitionHandle.Empty;
            }

            // 戻り先がない
            if (_stack.Count <= 1) {
                return TransitionHandle.Empty;
            }

            // 既に遷移中なら失敗
            if (IsTransitioning) {
                return new TransitionHandle(new Exception("In transitioning"));
            }

            // スタックを戻す
            for (var i = 0; i < depth; i++) {
                if (_stack.Count <= 1) {
                    break;
                }

                _stack.RemoveAt(_stack.Count - 1);
            }

            // 遷移実行
            return _situationContainer.Transition(Current.GetType(), onSetup, new SituationContainer.TransitionOption { Back = true }, overrideTransition, effects);
        }

        /// <summary>
        /// 現在のSituationをリセットする
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="effects">遷移演出</param>
        private TransitionHandle ResetInternal(Action<Situation> onSetup = null, params ITransitionEffect[] effects) {
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                return new TransitionHandle(new Exception("In transitioning"));
            }

            if (Current == null) {
                return new TransitionHandle(new Exception("Current situation is null."));
            }

            // リセット実行
            return _situationContainer.Reset(onSetup, effects);
        }
    }
}