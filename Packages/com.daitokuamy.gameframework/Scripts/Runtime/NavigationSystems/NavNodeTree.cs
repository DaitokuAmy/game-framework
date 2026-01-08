using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.NavigationSystems {
    /// <summary>
    /// NavNode構造を管理するツリー
    /// </summary>
    public sealed class NavNodeTree : ITransitionResolver, IStateContainer<Type, INavNode, NavNodeTree.TransitionOption> {
        /// <summary>
        /// 遷移オプション
        /// </summary>
        public class TransitionOption {
            /// <summary>Rootから再構築して遷移するか</summary>
            public bool Refresh = false;
        }

        /// <summary>
        /// PreLoadの状態
        /// </summary>
        private enum PreLoadState {
            /// <summary>PreLoad無し</summary>
            None,
            /// <summary>PreLoad中</summary>
            PreLoading,
            /// <summary>PreLoad済み</summary>
            PreLoaded,
        }

        /// <summary>
        /// 遷移情報
        /// </summary>
        private class TransitionInfo : ITransitionInfo<INavNode> {
            /// <summary>遷移時に閉じるNodeリスト</summary>
            public IReadOnlyList<INavNode> PrevNodes { get; set; }
            /// <summary>遷移時に開くNodeリスト</summary>
            public IReadOnlyList<INavNode> NextNodes { get; set; }
            /// <summary>遷移演出リスト</summary>
            public IReadOnlyList<ITransitionEffect> Effects { get; set; }
            /// <summary>遷移演出アクティブ状態</summary>
            public bool EffectActive { get; set; }

            /// <inheritdoc/>
            public TransitionDirection Direction { get; set; }
            /// <inheritdoc/>
            public TransitionState State { get; set; }
            /// <inheritdoc/>
            public INavNode Prev => PrevNodes.FirstOrDefault();
            /// <inheritdoc/>
            public INavNode Next => NextNodes.LastOrDefault();

            /// <inheritdoc/>
            public event Action<INavNode> FinishedEvent;

            /// <summary>
            /// 終了送信
            /// </summary>
            public void SendFinish() {
                FinishedEvent?.Invoke(Next);
                FinishedEvent = null;
            }
        }

        /// <summary>
        /// プリロード情報
        /// </summary>
        private class PreLoadInfo {
            public INavNode Node;
            public PreLoadState State;
            public AsyncOperator AsyncOperator;
        }

        private readonly IRootNode _rootNode;
        private readonly IReadOnlyDictionary<Type, INavNode> _nodeMap;
        private readonly List<INavNode> _runningNodes = new();
        private readonly Dictionary<Type, PreLoadInfo> _preLoadInfos = new();

        private TransitionInfo _transitionInfo;
        private CoroutineRunner _coroutineRunner;

        /// <inheritdoc/>
        public INavNode Current => _runningNodes.Count > 0 ? _runningNodes[^1] : null;
        /// <inheritdoc/>
        public bool IsTransitioning => _transitionInfo != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rootNode">ルートとして登録するノード</param>
        /// <param name="nodeMap">RootNode以下に登録されているNodeのマッピング情報</param>
        /// <param name="engine">Navigation制御用エンジン</param>
        public NavNodeTree(IRootNode rootNode, IReadOnlyDictionary<Type, INavNode> nodeMap, NavigationEngine engine) {
            _coroutineRunner = new();
            _rootNode = rootNode;
            _nodeMap = nodeMap;
            foreach (var node in _nodeMap.Values) {
                node.Standby(engine);
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // 非同期処理停止
            _coroutineRunner.StopAllCoroutines();
            _coroutineRunner.Dispose();
            _coroutineRunner = null;

            var emptyHandle = TransitionHandle<INavNode>.Empty;

            // PreLoad分をUnload
            var preLoadInfos = _preLoadInfos.Values.ToArray();
            _preLoadInfos.Clear();
            foreach (var info in preLoadInfos) {
                if (_runningNodes.Contains(info.Node)) {
                    continue;
                }

                info.Node.Unload(emptyHandle);
            }

            // 有効なNodeを廃棄
            for (var i = _runningNodes.Count - 1; i >= 0; i--) {
                var node = _runningNodes[i];
                node.Shutdown(emptyHandle);
            }
        }


        /// <inheritdoc/>
        INavNode IStateContainer<Type, INavNode, TransitionOption>.FindState(Type key) {
            return _nodeMap.GetValueOrDefault(key);
        }

        /// <inheritdoc/>
        Type[] IStateContainer<Type, INavNode, TransitionOption>.GetStateKeys() {
            return _nodeMap.Keys.ToArray();
        }

        /// <inheritdoc/>
        void ITransitionResolver.Start() {
            _transitionInfo.State = TransitionState.Standby;
            foreach (var effect in _transitionInfo.Effects) {
                effect.BeginTransition();
            }

            // 遷移開始時に全部のフォーカスを外す
            foreach (var node in _transitionInfo.PrevNodes) {
                node.SetFocus(false);
            }

            foreach (var node in _runningNodes) {
                node.SetFocus(false);
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

            var handle = new TransitionHandle<INavNode>(_transitionInfo);
            var routines = new List<IEnumerator>();
            var preLoadingInfos = new List<PreLoadInfo>();

            for (var i = 0; i < _transitionInfo.NextNodes.Count; i++) {
                var node = _transitionInfo.NextNodes[i];
                // PreLoad対象はSkip
                if (_preLoadInfos.TryGetValue(node.GetType(), out var preLoadInfo)) {
                    if (preLoadInfo.State == PreLoadState.PreLoading) {
                        preLoadingInfos.Add(preLoadInfo);
                    }

                    continue;
                }
                
                // 並列ロードしない物は順番にロード
                if (!node.IsParallelLoading) {
                    yield return node.LoadRoutine(handle);
                    continue;
                }

                // 並列ロード可能な物はまとめる
                routines.Add(node.LoadRoutine(handle));
            }

            // 並列読み込み待ち
            yield return new MergedCoroutine(routines);

            // PreLoad中の物があれば待つ
            foreach (var info in preLoadingInfos) {
                while (info.State == PreLoadState.PreLoading) {
                    yield return null;
                }
            }

            for (var i = 0; i < _transitionInfo.NextNodes.Count; i++) {
                yield return _transitionInfo.NextNodes[i].InitializeRoutine(handle);
            }
        }

        /// <inheritdoc/>
        void ITransitionResolver.ActivateNext() {
            var handle = new TransitionHandle<INavNode>(_transitionInfo);
            for (var i = 0; i < _transitionInfo.NextNodes.Count; i++) {
                _transitionInfo.NextNodes[i].Activate(handle);
            }
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.OpenNextRoutine(bool immediate) {
            _transitionInfo.State = TransitionState.Opening;

            var handle = new TransitionHandle<INavNode>(_transitionInfo);
            for (var i = 0; i < _transitionInfo.NextNodes.Count; i++) {
                if (_transitionInfo.NextNodes[i] is IScreenNode screenNode) {
                    screenNode.PreOpen(handle);
                }
            }

            if (!immediate) {
                var routines = new List<IEnumerator>();
                for (var i = 0; i < _transitionInfo.NextNodes.Count; i++) {
                    if (_transitionInfo.NextNodes[i] is IScreenNode screenNode) {
                        routines.Add(screenNode.OpenRoutine(handle));
                    }
                }

                yield return new MergedCoroutine(routines);
            }

            for (var i = 0; i < _transitionInfo.NextNodes.Count; i++) {
                if (_transitionInfo.NextNodes[i] is IScreenNode screenNode) {
                    screenNode.PostOpen(handle);
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator ITransitionResolver.ClosePrevRoutine(bool immediate) {
            var handle = new TransitionHandle<INavNode>(_transitionInfo);
            for (var i = 0; i < _transitionInfo.PrevNodes.Count; i++) {
                if (_transitionInfo.PrevNodes[i] is IScreenNode screenNode) {
                    screenNode.PreClose(handle);
                }
            }

            if (!immediate) {
                var routines = new List<IEnumerator>();
                for (var i = 0; i < _transitionInfo.PrevNodes.Count; i++) {
                    if (_transitionInfo.PrevNodes[i] is IScreenNode screenNode) {
                        routines.Add(screenNode.CloseRoutine(handle));
                    }
                }

                yield return new MergedCoroutine(routines);
            }

            for (var i = 0; i < _transitionInfo.PrevNodes.Count; i++) {
                if (_transitionInfo.PrevNodes[i] is IScreenNode screenNode) {
                    screenNode.PostClose(handle);
                }
            }
        }

        /// <inheritdoc/>
        void ITransitionResolver.DeactivatePrev() {
            var handle = new TransitionHandle<INavNode>(_transitionInfo);
            for (var i = 0; i < _transitionInfo.PrevNodes.Count; i++) {
                _transitionInfo.PrevNodes[i].Deactivate(handle);
            }
        }

        /// <inheritdoc/>
        void ITransitionResolver.UnloadPrev() {
            var handle = new TransitionHandle<INavNode>(_transitionInfo);

            // 終了
            for (var i = 0; i < _transitionInfo.PrevNodes.Count; i++) {
                _transitionInfo.PrevNodes[i].Terminate(handle);
            }

            // アンロード
            for (var i = 0; i < _transitionInfo.PrevNodes.Count; i++) {
                var node = _transitionInfo.PrevNodes[i];
                // PreLoad対象はSkip
                if (_preLoadInfos.TryGetValue(node.GetType(), out _)) {
                    continue;
                }

                _transitionInfo.PrevNodes[i].Unload(handle);
            }
        }

        /// <inheritdoc/>
        void ITransitionResolver.Finish() {
            foreach (var effect in _transitionInfo.Effects) {
                effect.EndTransition();
            }

            // 遷移完了時に先頭にいる物にフォーカスを充てる
            if (_runningNodes.Count > 0) {
                _runningNodes[^1].SetFocus(true);
            }

            _transitionInfo.State = TransitionState.Completed;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            _coroutineRunner?.Update();
        }

        /// <inheritdoc/>
        public TransitionHandle<INavNode> TransitionTo(Type nodeType, TransitionOption option, bool back, Action<INavNode> setupAction, ITransition transition, params ITransitionEffect[] effects) {
            if (IsTransitioning) {
                throw new Exception("In transitioning");
            }

            if (!_nodeMap.TryGetValue(nodeType, out var nextNode)) {
                throw new KeyNotFoundException($"Not found nodeType:{nodeType.Name}");
            }

            // 遷移する必要がなければ無視
            if (Current == nextNode) {
                return TransitionHandle<INavNode>.Empty;
            }

            var prevNode = Current;

            // 遷移先の共通親を探す
            var baseParent = default(INavNode);
            if (option == null || !option.Refresh) {
                baseParent = prevNode;
                while (baseParent != null) {
                    var p = nextNode;
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

            // 閉じるNodeリスト
            var prevNodes = new List<INavNode>();
            if (prevNode != null) {
                var p = prevNode;
                while (p != baseParent) {
                    prevNodes.Add(p);
                    p = p.Parent;
                }
            }

            // 開くNodeリスト
            var nextNodes = new List<INavNode>();
            if (nextNode != null) {
                var p = nextNode;
                while (p != baseParent) {
                    nextNodes.Insert(0, p);
                    p = p.Parent;
                }
            }

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                Direction = back ? TransitionDirection.Back : TransitionDirection.Forward,
                PrevNodes = prevNodes,
                NextNodes = nextNodes,
                State = TransitionState.Standby,
                Effects = effects
            };

            // コルーチンの登録
            void FinishTransition() {
                var transitionInfo = _transitionInfo;
                _transitionInfo = null;
                transitionInfo.SendFinish();
            }

            _coroutineRunner.StartCoroutine(TransitionRoutine(nextNode, setupAction, transition),
                FinishTransition,
                FinishTransition,
                ex => {
                    Debug.LogException(ex);
                    FinishTransition();
                });

            // ハンドルの返却
            return new TransitionHandle<INavNode>(_transitionInfo);
        }

        /// <inheritdoc/>
        public TransitionHandle<INavNode> Reset(Action<INavNode> setupAction, params ITransitionEffect[] effects) {
            if (IsTransitioning) {
                throw new Exception("In transitioning");
            }

            if (Current == null) {
                throw new Exception("Current situation is null");
            }

            // 閉じるNodeリスト
            var prevNodes = new List<INavNode>();
            var node = Current;
            while (node != null) {
                prevNodes.Add(node);
                node = node.Parent;
            }

            // 開くNodeリスト
            var nextNodes = new List<INavNode>();
            for (var i = prevNodes.Count - 1; i >= 0; i--) {
                nextNodes.Add(prevNodes[i]);
            }

            // 遷移はOutIn専用
            var transition = (ITransition)new OutInTransition();

            // 遷移情報を生成            
            _transitionInfo = new TransitionInfo {
                Direction = TransitionDirection.Forward,
                PrevNodes = prevNodes,
                NextNodes = nextNodes,
                State = TransitionState.Standby,
                Effects = effects
            };

            // 初期化通知
            setupAction?.Invoke(Current);

            // コルーチンの登録
            void FinishTransition() {
                var transitionInfo = _transitionInfo;
                _transitionInfo = null;
                transitionInfo.SendFinish();
            }

            _coroutineRunner.StartCoroutine(transition.TransitionRoutine(this),
                FinishTransition,
                FinishTransition,
                ex => {
                    Debug.LogException(ex);
                    FinishTransition();
                });

            // ハンドルの返却
            return new TransitionHandle<INavNode>(_transitionInfo);
        }

        /// <summary>
        /// カレントNodeの階層に特定のNavNode型が存在するかチェック
        /// ※カレントもチェック対象
        /// </summary>
        public bool CheckNodeTypeInParent<T>()
            where T : INavNode {
            var searchType = typeof(T);
            var node = Current;
            while (node != null) {
                if (node.GetType().IsAssignableFrom(searchType)) {
                    return true;
                }

                node = node.Parent;
            }

            return false;
        }

        /// <summary>
        /// カレントNodeの階層の中で特定の型のNodeを取得
        /// ※カレントもチェック対象
        /// </summary>
        public T GetNodeInParent<T>()
            where T : INavNode {
            var searchType = typeof(T);
            var node = Current;
            while (node != null) {
                if (searchType.IsAssignableFrom(node.GetType())) {
                    return (T)node;
                }

                node = node.Parent;
            }

            return default;
        }

        /// <summary>
        /// 特定Nodeの階層の中で特定の型のNodeを取得
        /// ※指定Nodeもチェック対象
        /// </summary>
        public T GetNodeInParent<T>(Type targetNodeType)
            where T : INavNode {
            if (!_nodeMap.TryGetValue(targetNodeType, out var node)) {
                return default;
            }
            
            var searchType = typeof(T);
            while (node != null) {
                if (searchType.IsAssignableFrom(node.GetType())) {
                    return (T)node;
                }

                node = node.Parent;
            }

            return default;
        }

        /// <summary>
        /// NavNodeのプリロード
        /// </summary>
        public AsyncOperationHandle PreLoad(Type nodeType) {
            if (IsTransitioning) {
                throw new Exception("In transitioning");
            }

            if (!_nodeMap.TryGetValue(nodeType, out var node)) {
                return AsyncOperationHandle.CanceledHandle;
            }

            // PreLoad情報を確認
            if (!_preLoadInfos.TryGetValue(nodeType, out var preLoadInfo)) {
                preLoadInfo = new PreLoadInfo { Node = node, State = PreLoadState.None, AsyncOperator = new AsyncOperator() };
                _preLoadInfos.Add(nodeType, preLoadInfo);
            }

            // PreLoad実行済み
            if (preLoadInfo.State != PreLoadState.None) {
                return preLoadInfo.AsyncOperator;
            }

            // Load実行
            _coroutineRunner.StartCoroutine(PreLoadRoutine(preLoadInfo),
                () => { preLoadInfo.AsyncOperator.Completed(); },
                () => { preLoadInfo.AsyncOperator.Aborted(); },
                ex => { preLoadInfo.AsyncOperator.Aborted(ex); });
            return preLoadInfo.AsyncOperator;
        }

        /// <summary>
        /// NavNodeのプリロード状態解除
        /// </summary>
        public void UnPreLoad(Type nodeType) {
            if (IsTransitioning) {
                throw new Exception("In transitioning");
            }

            if (!_nodeMap.TryGetValue(nodeType, out _)) {
                return;
            }

            // PreLoad情報から取り出し
            if (!_preLoadInfos.Remove(nodeType, out var preLoadInfo)) {
                return;
            }

            // Running中の物は無視
            if (!_runningNodes.Contains(preLoadInfo.Node)) {
                return;
            }

            // Unload実行
            preLoadInfo.State = PreLoadState.None;
            if (!preLoadInfo.AsyncOperator.IsDone) {
                preLoadInfo.AsyncOperator.Completed();
            }

            preLoadInfo.Node.Unload(TransitionHandle<INavNode>.Empty);
        }

        /// <summary>
        /// 遷移ルーチン
        /// </summary>
        private IEnumerator TransitionRoutine(INavNode nextNode, Action<INavNode> setupAction, ITransition transition) {
            // アクティブなNodeの更新
            _runningNodes.Clear();
            {
                var n = nextNode;
                while (n != null) {
                    _runningNodes.Insert(0, n);
                    n = n.Parent;
                }
            }

            // 初期化処理
            setupAction?.Invoke(Current);

            yield return transition.TransitionRoutine(this);
        }

        /// <summary>
        /// プリロードコルーチン
        /// </summary>
        private IEnumerator PreLoadRoutine(PreLoadInfo preLoadInfo) {
            if (preLoadInfo.State == PreLoadState.PreLoaded) {
                yield break;
            }

            preLoadInfo.State = PreLoadState.PreLoading;
            yield return preLoadInfo.Node.LoadRoutine(TransitionHandle<INavNode>.Empty);
            preLoadInfo.State = PreLoadState.PreLoaded;
        }
    }
}