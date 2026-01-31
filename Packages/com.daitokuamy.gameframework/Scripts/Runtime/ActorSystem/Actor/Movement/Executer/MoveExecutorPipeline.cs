using System;
using System.Collections.Generic;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// 同一リクエスト型の移動実行器を管理するパイプライン
    /// </summary>
    internal sealed class MoveExecutorPipeline<TRequest> : IMoveExecutorPipeline
        where TRequest : struct, IMoveRequest {
        /// <summary>登録されている実行器一覧</summary>
        private readonly List<IMoveExecutor<TRequest>> _executors = new();
        /// <summary>優先度順にソート済みの実行器配列</summary>
        private IMoveExecutor<TRequest>[] _sorted = Array.Empty<IMoveExecutor<TRequest>>();
        /// <summary>ソート状態が古いかどうか</summary>
        private bool _dirty = true;

        /// <summary>現在実行中の実行器</summary>
        private IMoveExecutor<TRequest> _active;
        /// <summary>現在実行中実行器のインデックス</summary>
        private int _activeIndex;
        /// <summary>最後に開始したリクエスト</summary>
        private TRequest _lastRequest;

        /// <summary>現在実行中かどうか</summary>
        public bool IsRunning => _active != null;

        /// <summary>
        /// 実行器を追加する
        /// </summary>
        /// <param name="executor">追加する実行器</param>
        public void Add(IMoveExecutor<TRequest> executor) {
            _executors.Add(executor);
            _dirty = true;
        }

        /// <summary>
        /// リクエストを開始する
        /// </summary>
        /// <param name="request">移動リクエスト</param>
        /// <returns>開始できた場合 true</returns>
        public bool Start(in TRequest request) {
            Cancel();
            _lastRequest = request;
            EnsureSorted();
            for (var i = 0; i < _sorted.Length; i++) {
                if (_sorted[i].TryStart(in request) != StartResult.Accepted) {
                    continue;
                }

                _active = _sorted[i];
                _activeIndex = i;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 実行中リクエストを更新する
        /// </summary>
        /// <param name="request">更新後の移動リクエスト</param>
        /// <returns>更新できた場合 true</returns>
        public bool Update(in TRequest request) {
            if (_active is not IUpdatableMoveExecutor<TRequest> updatable) {
                return false;
            }

            _lastRequest = request;
            updatable.Update(in request);
            return true;
        }

        /// <summary>
        /// 実行中処理を進行する
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        /// <returns>実行結果</returns>
        public RunResult Tick(float deltaTime) {
            if (_active == null) {
                return RunResult.Cancelled;
            }

            var result = _active.Tick(deltaTime);
            if (result == RunResult.Running) {
                return result;
            }

            if (result == RunResult.FailedRecoverable) {
                _active.Cancel();
                _active = null;
                EnsureSorted();
                for (var i = _activeIndex + 1; i < _sorted.Length; i++) {
                    if (_sorted[i].TryStart(in _lastRequest) != StartResult.Accepted) continue;
                    _active = _sorted[i];
                    _activeIndex = i;
                    return RunResult.Running;
                }

                return RunResult.FailedHard;
            }

            _active = null;
            return result;
        }

        /// <summary>
        /// 実行中処理をキャンセルする
        /// </summary>
        public void Cancel() {
            if (_active == null) {
                return;
            }

            _active.Cancel();
            _active = null;
        }

        private void EnsureSorted() {
            if (!_dirty) {
                return;
            }

            _sorted = _executors.ToArray();
            Array.Sort(_sorted, static (a, b) => b.Priority.CompareTo(a.Priority));
            _dirty = false;
        }
    }
}