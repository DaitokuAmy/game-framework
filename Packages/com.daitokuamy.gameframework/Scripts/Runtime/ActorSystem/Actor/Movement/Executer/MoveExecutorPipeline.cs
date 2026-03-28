using System.Collections.Generic;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// 同一リクエスト型の移動実行器を管理するパイプライン
    /// </summary>
    internal sealed class MoveExecutorPipeline<TRequest> : IMoveExecutorPipeline
        where TRequest : struct, IMoveRequest {
        private readonly List<IMoveExecutor<TRequest>> _executors = new();

        private bool _dirty = true;
        private IMoveExecutor<TRequest> _active;
        private int _activeIndex;
        private TRequest _lastRequest;

        /// <inheritdoc/>
        bool IMoveExecutorPipeline.IsRunning => _active != null;

        /// <inheritdoc/>
        RunResult IMoveExecutorPipeline.Tick(float deltaTime) {
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
                for (var i = _activeIndex + 1; i < _executors.Count; i++) {
                    if (_executors[i].TryStart(in _lastRequest) != StartResult.Accepted) continue;
                    _active = _executors[i];
                    _activeIndex = i;
                    return RunResult.Running;
                }

                return RunResult.FailedHard;
            }

            _active = null;
            return result;
        }

        /// <inheritdoc/>
        public void Cancel(bool skip) {
            if (_active == null) {
                return;
            }

            _active.Cancel(skip);
            _active = null;
        }

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
            Cancel(false);
            _lastRequest = request;
            EnsureSorted();
            for (var i = 0; i < _executors.Count; i++) {
                if (_executors[i].TryStart(in request) != StartResult.Accepted) {
                    continue;
                }

                _active = _executors[i];
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
        /// Executorの並び替え
        /// </summary>
        private void EnsureSorted() {
            if (!_dirty) {
                return;
            }

            _executors.Sort(static (a, b) => a.Priority.CompareTo(b.Priority));
            _dirty = false;
        }
    }
}
