using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// シーンアセットリクエスト用ハンドル
    /// 代入では同じleaseを共有するため、独立した寿命が必要な場合はAcquireを使う
    /// </summary>
    public sealed class SceneAssetHandle : IProcess<Scene> {
        /// <summary>
        /// 共有状態
        /// </summary>
        private sealed class State {
            public ISceneAssetInfo Info;
            private readonly HashSet<int> _leaseIds = new() { 0 };
            private int _nextLeaseId;

            public int AddLease() {
                if (Info == null) {
                    return -1;
                }

                var leaseId = ++_nextLeaseId;
                _leaseIds.Add(leaseId);
                return leaseId;
            }

            public bool HasLease(int leaseId) {
                return Info != null && _leaseIds.Contains(leaseId);
            }

            public void ReleaseLease(int leaseId) {
                if (Info == null) {
                    return;
                }

                if (!_leaseIds.Remove(leaseId)) {
                    return;
                }

                if (_leaseIds.Count > 0) {
                    return;
                }

                Info.Dispose();
                Info = null;
            }
        }

        /// <summary>無効なSceneAssetHandle</summary>
        public static readonly SceneAssetHandle Empty = new SceneAssetHandle();

        // 読み込み情報
        private State _state;
        private int _leaseId;

        private ISceneAssetInfo Info => _state != null && _state.HasLease(_leaseId) ? _state.Info : null;

        /// <summary>読み込み完了しているか</summary>
        public bool IsDone => Info == null || Info.IsDone;
        /// <summary>シーン</summary>
        public Scene Scene => Info?.Scene ?? new Scene();
        /// <summary>エラー</summary>
        public Exception Exception => Info?.Exception;
        /// <summary>有効なハンドルか</summary>
        public bool IsValid => Info != null;
        // 結果
        Scene IProcess<Scene>.Result => Scene;
        // IEnumerator用
        object IEnumerator.Current => null;

        private SceneAssetHandle() {
            _state = null;
            _leaseId = -1;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">読み込み管理用情報</param>
        public SceneAssetHandle(ISceneAssetInfo info) {
            _state = info != null ? new State {
                Info = info
            } : null;
            _leaseId = _state != null ? 0 : -1;
        }

        private SceneAssetHandle(State state, int leaseId) {
            _state = state;
            _leaseId = leaseId;
        }

        /// <summary>
        /// 参照用のハンドルを複製
        /// </summary>
        public SceneAssetHandle Acquire() {
            if (!IsValid) {
                return Empty;
            }

            var leaseId = _state.AddLease();
            return leaseId >= 0 ? new SceneAssetHandle(_state, leaseId) : Empty;
        }

        /// <summary>
        /// アクティブ化
        /// </summary>
        public AsyncOperationHandle ActivateAsync() {
            if (Info == null) {
                return AsyncOperationHandle.CanceledHandle;
            }

            var asyncOperation = Info.ActivateAsync();
            if (asyncOperation == null) {
                return AsyncOperationHandle.CompletedHandle;
            }

            var asyncOperator = new AsyncOperator();
            asyncOperation.completed += _ => asyncOperator.Completed();
            return asyncOperator;
        }

        /// <summary>
        /// 読み込んだアセットの解放
        /// </summary>
        public void Release() {
            if (_state == null) {
                return;
            }

            _state.ReleaseLease(_leaseId);
            _state = null;
            _leaseId = -1;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }
    }
}
