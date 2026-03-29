using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystem {    
    /// <summary>
    /// アセット用ハンドル
    /// 代入では同じleaseを共有するため、独立した寿命が必要な場合はAcquireを使う
    /// </summary>
    public sealed class AssetHandle<T> : IProcess<T>
        where T : Object {
        /// <summary>
        /// 共有状態
        /// </summary>
        private sealed class State {
            public IAssetInfo<T> Info;
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

        /// <summary>無効なAssetHandle</summary>
        public static readonly AssetHandle<T> Empty = new AssetHandle<T>();
        // IEnumerator用
        object IEnumerator.Current => null;

        // 読み込みリクエスト情報
        private State _state;
        private int _leaseId;

        private IAssetInfo<T> Info => _state != null && _state.HasLease(_leaseId) ? _state.Info : null;

        /// <summary>読み込み完了しているか</summary>
        public bool IsDone => Info == null || Info.IsDone;
        /// <summary>読み込んだアセット</summary>
        public T Asset => Info?.Asset;
        /// <summary>エラー</summary>
        public Exception Exception => Info?.Exception;
        /// <summary>有効なハンドルか</summary>
        public bool IsValid => Info != null;
        // 結果
        T IProcess<T>.Result => Asset;

        private AssetHandle() {
            _state = null;
            _leaseId = -1;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">読み込み管理用情報</param>
        public AssetHandle(IAssetInfo<T> info) {
            _state = info != null ? new State {
                Info = info
            } : null;
            _leaseId = _state != null ? 0 : -1;
        }

        private AssetHandle(State state, int leaseId) {
            _state = state;
            _leaseId = leaseId;
        }

        /// <summary>
        /// 参照用のハンドルを複製
        /// </summary>
        public AssetHandle<T> Acquire() {
            if (!IsValid) {
                return Empty;
            }

            var leaseId = _state.AddLease();
            return leaseId >= 0 ? new AssetHandle<T>(_state, leaseId) : Empty;
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
