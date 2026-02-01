using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクター移動制御の統合クラス
    /// </summary>
    public sealed class ActorMover {
        private readonly MoveExecutorPipeline<MoveToRequest> _moveTo = new();
        private readonly MoveExecutorPipeline<ApproachRequest> _approach = new();
        private readonly MoveExecutorPipeline<WarpRequest> _warp = new();
        private readonly MoveExecutorPipeline<DriveRequest> _drive = new();

        private IMoveExecutorPipeline _active;

        /// <summary>現在移動中かどうか</summary>
        public bool IsMoving => _active?.IsRunning ?? false;

        /// <summary>
        /// 毎フレーム呼び出して移動処理を進行する
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        public void Tick(float deltaTime) {
            if (_active == null) {
                return;
            }

            var result = _active.Tick(deltaTime);
            if (result != RunResult.Running) {
                _active = null;
            }
        }

        /// <summary>
        /// 現在の移動を停止する
        /// </summary>
        public void Stop() {
            if (_active == null) {
                return;
            }

            _active.Cancel();
            _active = null;
        }

        /// <summary>
        /// 現在の移動をスキップする
        /// </summary>
        public void Skip() {
            if (_active == null) {
                return;
            }
            
            _active.Cancel();
            _active = null;
        }

        /// <summary>
        /// MoveTo用の実行器を追加する
        /// </summary>
        /// <param name="executor">追加する実行器</param>
        public void AddExecutor(IMoveExecutor<MoveToRequest> executor) => _moveTo.Add(executor);

        /// <summary>
        /// Approach用の実行器を追加する
        /// </summary>
        /// <param name="executor">追加する実行器</param>
        public void AddExecutor(IMoveExecutor<ApproachRequest> executor) => _approach.Add(executor);

        /// <summary>
        /// Warp用の実行器を追加する
        /// </summary>
        /// <param name="executor">追加する実行器</param>
        public void AddExecutor(IMoveExecutor<WarpRequest> executor) => _warp.Add(executor);

        /// <summary>
        /// Drive用の実行器を追加する
        /// </summary>
        /// <param name="executor">追加する実行器</param>
        public void AddExecutor(IMoveExecutor<DriveRequest> executor) => _drive.Add(executor);

        /// <summary>
        /// ワールド座標へ移動する
        /// </summary>
        /// <param name="worldPosition">移動先ワールド座標</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="stopDistance">到達判定距離</param>
        /// <param name="options">移動オプション</param>
        /// <returns>開始できた場合 true</returns>
        public bool MoveTo(Vector3 worldPosition, float speedMultiplier = 1.0f, float stopDistance = 0.1f, MoveOptions options = default) {
            Stop();
            var request = new MoveToRequest(MoveTarget.FromWorld(worldPosition), speedMultiplier, stopDistance, options);
            var ok = _moveTo.Start(in request);
            _active = ok ? _moveTo : null;
            return ok;
        }

        /// <summary>
        /// Transform + 相対オフセットへ移動する
        /// </summary>
        /// <param name="target">基準 Transform</param>
        /// <param name="offset">相対オフセット</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="stopDistance">到達判定距離</param>
        /// <param name="options">移動オプション</param>
        /// <returns>開始できた場合 true</returns>
        public bool MoveTo(Transform target, Vector3 offset, float speedMultiplier = 1.0f, float stopDistance = 0.1f, MoveOptions options = default) {
            Stop();
            var request = new MoveToRequest(MoveTarget.FromRelative(target, offset), speedMultiplier, stopDistance, options);
            var ok = _moveTo.Start(in request);
            _active = ok ? _moveTo : null;
            return ok;
        }

        /// <summary>
        /// Transform の現在位置へ移動する
        /// </summary>
        /// <param name="target">移動先 Transform</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="stopDistance">到達判定距離</param>
        /// <param name="options">移動オプション</param>
        /// <returns>開始できた場合 true</returns>
        public bool MoveTo(Transform target, float speedMultiplier = 1.0f, float stopDistance = 0.1f, MoveOptions options = default) {
            return MoveTo(target, Vector3.zero, speedMultiplier, stopDistance, options);
        }

        /// <summary>
        /// Transform + 相対オフセットへ接近する
        /// </summary>
        /// <param name="target">基準 Transform</param>
        /// <param name="offset">相対オフセット</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="stopDistance">到達判定距離</param>
        /// <param name="options">移動オプション</param>
        /// <returns>開始できた場合 true</returns>
        public bool Approach(Transform target, Vector3 offset, float speedMultiplier = 1.0f, float stopDistance = 1.0f, MoveOptions options = default) {
            Stop();
            var request = new ApproachRequest(MoveTarget.FromRelative(target, offset), speedMultiplier, stopDistance, options);
            var ok = _approach.Start(in request);
            _active = ok ? _approach : null;
            return ok;
        }

        /// <summary>
        /// ワールド座標へ接近する
        /// </summary>
        /// <param name="worldPosition">接近先ワールド座標</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="stopDistance">到達判定距離</param>
        /// <param name="options">移動オプション</param>
        /// <returns>開始できた場合 true</returns>
        public bool Approach(Vector3 worldPosition, float speedMultiplier = 1.0f, float stopDistance = 1.0f, MoveOptions options = default) {
            Stop();
            var request = new ApproachRequest(MoveTarget.FromWorld(worldPosition), speedMultiplier, stopDistance, options);
            var ok = _approach.Start(in request);
            _active = ok ? _approach : null;
            return ok;
        }

        /// <summary>
        /// Transform の現在位置へ接近する
        /// </summary>
        /// <param name="target">基準 Transform</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="stopDistance">到達判定距離</param>
        /// <param name="options">移動オプション</param>
        /// <returns>開始できた場合 true</returns>
        public bool Approach(Transform target, float speedMultiplier = 1.0f, float stopDistance = 1.0f, MoveOptions options = default) {
            return Approach(target, Vector3.zero, speedMultiplier, stopDistance, options);
        }

        /// <summary>
        /// ワールド座標へワープする
        /// </summary>
        /// <param name="worldPosition">ワープ先ワールド座標</param>
        /// <returns>開始できた場合 true</returns>
        public bool Warp(Vector3 worldPosition) {
            Stop();
            var request = new WarpRequest(MoveTarget.FromWorld(worldPosition));
            var ok = _warp.Start(in request);
            _active = ok ? _warp : null;
            return ok;
        }

        /// <summary>
        /// Transform + 相対オフセットへワープする
        /// </summary>
        /// <param name="target">基準 Transform</param>
        /// <param name="offset">相対オフセット</param>
        /// <returns>開始できた場合 true</returns>
        public bool Warp(Transform target, Vector3 offset) {
            Stop();
            var request = new WarpRequest(MoveTarget.FromRelative(target, offset));
            var ok = _warp.Start(in request);
            _active = ok ? _warp : null;
            return ok;
        }

        /// <summary>
        /// Transform の現在位置へワープする
        /// </summary>
        /// <param name="target">ワープ先 Transform</param>
        /// <returns>開始できた場合 true</returns>
        public bool Warp(Transform target) {
            return Warp(target, Vector3.zero);
        }

        /// <summary>
        /// 継続入力で移動する（毎フレ呼び出し想定）
        /// </summary>
        /// <param name="move">入力ベクトル（-1〜1 正規化想定）</param>
        /// <param name="run">走行フラグ</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="options">移動オプション</param>
        /// <returns>実行継続できた場合 true</returns>
        public bool Drive(Vector2 move, bool run = false, float speedMultiplier = 1.0f, MoveOptions options = default) {
            var request = new DriveRequest(move, run, speedMultiplier, options);
            if (_active != _drive) {
                Stop();
                var ok = _drive.Start(in request);
                _active = ok ? _drive : null;
                return ok;
            }

            return _drive.Update(in request);
        }

        /// <summary>
        /// 継続入力で移動する（入力成分指定）
        /// </summary>
        /// <param name="x">入力X（-1〜1）</param>
        /// <param name="y">入力Y（-1〜1）</param>
        /// <param name="run">走行フラグ</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="options">移動オプション</param>
        /// <returns>実行継続できた場合 true</returns>
        public bool Drive(float x, float y, bool run = false, float speedMultiplier = 1.0f, MoveOptions options = default) {
            return Drive(new Vector2(x, y), run, speedMultiplier, options);
        }
    }
}