using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework {
    /// <summary>
    /// ステート遷移情報格納用ツリー用ノード
    /// </summary>
    public class StateTreeNode<TKey> : IDisposable
        where TKey : IEquatable<TKey> {
        /// <summary>
        /// 遷移情報
        /// </summary>
        public struct TransitionInfo {
            public StateTreeNode<TKey> PrevNode;
            public StateTreeNode<TKey> NextNode;
            public bool Back;
        }

        /// <summary>
        /// 接続情報
        /// </summary>
        private class ConnectInfo {
            public Action<TransitionInfo> TransitionEvent;
            public StateTreeNode<TKey> NextNode;
        }

        private readonly Dictionary<TKey, ConnectInfo> _connectInfos = new();

        private TKey _key;
        private StateTreeNode<TKey> _previous;
        private bool _disposed;

        /// <summary>有効か</summary>
        public bool IsValid => !_disposed;
        /// <summary>遷移時に使うキー</summary>
        public TKey Key => _key;
        /// <summary>ルートノードか</summary>
        public bool IsRoot => _key.Equals(default);

        /// <summary>遷移先のノードリスト</summary>
        internal StateTreeNode<TKey>[] NextNodes => _connectInfos.Select(x => x.Value.NextNode).ToArray();

        /// <summary>フォールバック経由で遷移された時の通知</summary>
        public event Action<TransitionInfo> TransitionByFallbackEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="key">遷移時に指定するキー</param>
        /// <param name="prev">接続元のNode</param>
        internal StateTreeNode(TKey key, StateTreeNode<TKey> prev) {
            _key = key;
            _previous = prev;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            // 前要素から参照を削除
            if (_previous != null) {
                _previous._connectInfos.Remove(Key);
            }

            // 次要素を削除
            var infos = _connectInfos.Values.ToArray();
            foreach (var info in infos) {
                if (info == null) {
                    continue;
                }

                if (info.NextNode != null) {
                    info.NextNode.Dispose();
                }
            }

            _previous = null;
            _connectInfos.Clear();
        }

        /// <summary>
        /// ノードの接続
        /// </summary>
        /// <param name="key">State遷移するためのキー</param>
        /// <param name="overridePrevNode">戻り先ノードの上書き指定</param>
        /// <param name="onTransition">遷移時の通知</param>
        public StateTreeNode<TKey> Connect(TKey key, StateTreeNode<TKey> overridePrevNode, Action<TransitionInfo> onTransition = null) {
            if (!IsValid) {
                return null;
            }
            
            // 既に同じKeyがあった場合は何もしない
            if (_connectInfos.TryGetValue(key, out var connectInfo)) {
                return connectInfo.NextNode;
            }

            // ノードの追加
            var nextNode = new StateTreeNode<TKey>(key, overridePrevNode ?? this);
            connectInfo = new ConnectInfo {
                TransitionEvent = onTransition,
                NextNode = nextNode
            };
            _connectInfos.Add(key, connectInfo);
            return nextNode;
        }

        /// <summary>
        /// ノードの接続
        /// </summary>
        /// <param name="key">State遷移するためのキー</param>
        /// <param name="onTransition">遷移時の通知</param>
        public StateTreeNode<TKey> Connect(TKey key, Action<TransitionInfo> onTransition = null) {
            return Connect(key, null, onTransition);
        }

        /// <summary>
        /// ノードの接続解除
        /// </summary>
        public bool Disconnect(TKey key) {
            if (!IsValid) {
                return false;
            }
            
            // 該当のTypeがなければ何もしない
            if (!_connectInfos.TryGetValue(key, out var connectInfo)) {
                return false;
            }

            // ノードの削除
            connectInfo.NextNode.Dispose();
            _connectInfos.Remove(key);
            return true;
        }

        /// <summary>
        /// 前のNodeを取得
        /// </summary>
        internal StateTreeNode<TKey> GetPrevious() {
            return _previous;
        }

        /// <summary>
        /// 型をもとに接続先のNodeを取得
        /// </summary>
        internal StateTreeNode<TKey> TryGetNext(TKey key) {
            if (!_connectInfos.TryGetValue(key, out var connectInfo)) {
                return null;
            }

            return connectInfo.NextNode;
        }

        /// <summary>
        /// 遷移通知
        /// </summary>
        /// <param name="prevNode">遷移前のノード</param>
        /// <param name="back">戻り遷移か</param>
        internal void OnTransition(StateTreeNode<TKey> prevNode, bool back) {
            // 遷移元がなければ何もしない
            if (prevNode == null) {
                return;
            }

            // State間の遷移以外は何もしない
            if (prevNode.Key == null || Key == null) {
                return;
            }

            var transitionInfo = new TransitionInfo {
                PrevNode = prevNode,
                NextNode = this,
                Back = back
            };

            // 戻り遷移の場合はConnect情報からイベントを探す
            if (back) {
                if (_connectInfos.TryGetValue(prevNode.Key, out var connectInfo)) {
                    connectInfo.TransitionEvent?.Invoke(transitionInfo);
                }
            }
            // 進む遷移の場合は、前のノードのConnect情報からイベントを探す
            else {
                if (prevNode._connectInfos.TryGetValue(Key, out var connectInfo)) {
                    connectInfo.TransitionEvent?.Invoke(transitionInfo);
                }
                // 接続情報がなく遷移した場合は、Fallback遷移とみなして通知する
                else {
                    TransitionByFallbackEvent?.Invoke(transitionInfo);
                }
            }
        }
    }
}