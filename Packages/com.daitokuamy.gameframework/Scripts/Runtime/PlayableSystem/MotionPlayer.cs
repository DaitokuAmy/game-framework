using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystem {
    /// <summary>
    /// Motionを再生させるためのクラス
    /// </summary>
    public sealed class MotionPlayer : IDisposable {
        // Playable情報
        private PlayableGraph _graph;
        // 再生速度
        private float _speed = 1.0f;
        // ルートとなる再生レイヤー
        private MotionLayerHandler _rootLayerHandler;
        // DSPClock 用の前回サンプル時刻
        private double _previousClockTime;
        // DSPClock のサンプル取得済みか
        private bool _hasClockTimeSample;
        // Simulation 更新待ちの経過時間
        private float _pendingSimulationDelta;
        // Graph 反映待ちの経過時間
        private float _pendingEvaluateDelta;

        /// <summary>再生に使うAnimator</summary>
        public Animator Animator { get; private set; }
        /// <summary>アニメーションの更新をSkipするフレーム数(0以上)</summary>
        public int SkipFrame { get; set; } = 0;
        /// <summary>アニメーションの更新をSkipするかのフレーム数に対するOffset</summary>
        public int SkipFrameOffset { get; set; } = 0;
        /// <summary>AnimationJob差し込み用</summary>
        public AnimationJobConnector JobConnector { get; private set; }
        /// <summary>再生に使うハンドル</summary>
        public MotionHandle Handle => _rootLayerHandler.BaseHandle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="animator">Outputを反映させるAnimator</param>
        /// <param name="updateMode">更新モード</param>
        /// <param name="outputSortingOrder">Outputの出力オーダー</param>
        public MotionPlayer(Animator animator, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime, ushort outputSortingOrder = 0) {
            Animator = animator;
            
            _graph = PlayableGraph.Create($"{nameof(MotionPlayer)}({animator.name})");
            
            var output = AnimationPlayableOutput.Create(_graph, "Output", animator);

            _graph.SetTimeUpdateMode(updateMode);
            output.SetSortingOrder(outputSortingOrder);
            
            // RootMotionLayerを生成して接続
            _rootLayerHandler = new MotionLayerHandler(animator);
            output.SetSourcePlayable(_rootLayerHandler.CreatePlayable(_graph));
            
            // JobPlayerの生成
            JobConnector = new AnimationJobConnector(animator, _graph);

            // 時間源切り替え直後のジャンプを防ぐ
            ResetClockTimeSample();

            // Manual は自前で Evaluate し、それ以外は Unity の更新サイクルに任せる
            SetGraphPlaying(updateMode != DirectorUpdateMode.Manual && ShouldEvaluateThisFrame());
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // JobConnector削除
            JobConnector?.Dispose();
            JobConnector = null;

            // RootComponent
            _rootLayerHandler?.Dispose();
            _rootLayerHandler = null;

            // Graphを削除
            if (_graph.IsValid()) {
                _graph.Destroy();
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            if (!_graph.IsValid()) {
                return;
            }
            
            // 変位時間取得
            var updateMode = _graph.GetTimeUpdateMode();
            var deltaTime = GetElapsedTime(updateMode);
            var simulationDeltaTime = deltaTime * _speed;
            var shouldEvaluateThisFrame = ShouldEvaluateThisFrame();

            if (updateMode == DirectorUpdateMode.Manual) {
                _pendingSimulationDelta += simulationDeltaTime;
                _pendingEvaluateDelta += deltaTime;
                if (!shouldEvaluateThisFrame) {
                    return;
                }

                FlushPendingTime();
                return;
            }

            if (!shouldEvaluateThisFrame) {
                SetGraphPlaying(false);
                _pendingSimulationDelta += simulationDeltaTime;
                _pendingEvaluateDelta += deltaTime;
                return;
            }

            if (!_graph.IsPlaying() && _pendingEvaluateDelta > 0.0f) {
                // 停止中に進んだ分だけ先に追いつかせてから、今フレームは Unity の自動評価に任せる
                FlushPendingTime();
            }

            UpdateSimulation(simulationDeltaTime);
            SetGraphPlaying(true);
        }

        /// <summary>
        /// 更新モードの変更
        /// </summary>
        public void SetUpdateMode(DirectorUpdateMode updateMode) {
            FlushPendingTime();
            _graph.SetTimeUpdateMode(updateMode);

            // モードごとに参照する時間源が変わるためサンプルを破棄する
            ResetClockTimeSample();
            _pendingSimulationDelta = 0.0f;
            _pendingEvaluateDelta = 0.0f;
            SetGraphPlaying(updateMode != DirectorUpdateMode.Manual && ShouldEvaluateThisFrame());
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        public void SetSpeed(float speed) {
            speed = Mathf.Max(0.0f, speed);

            if (Mathf.Abs(speed - _speed) <= float.Epsilon) {
                return;
            }

            FlushPendingTime();
            
            _speed = speed;
            JobConnector.SetSpeed(speed);
            _rootLayerHandler.SetSpeed(speed);
        }

        /// <summary>
        /// 拡張レイヤーの生成
        /// </summary>
        /// <param name="additive">加算レイヤーか</param>
        /// <param name="avatarMask">アバターマスク</param>
        /// <param name="weight">初期ウェイト</param>
        public MotionHandle CreateExtensionLayer(bool additive = false, AvatarMask avatarMask = null, float weight = 1.0f) {
            return _rootLayerHandler.CreateExtensionLayer(additive, avatarMask, weight);
        }

        /// <summary>
        /// 拡張レイヤーの取得
        /// </summary>
        /// <param name="index">拡張レイヤーのIndex</param>
        public MotionHandle GetExtensionLayer(int index) {
            return _rootLayerHandler.GetExtensionLayer(index);
        }

        /// <summary>
        /// 拡張レイヤーの削除
        /// </summary>
        /// <param name="handle">対象を表すハンドル</param>
        public void RemoveExtensionLayer(MotionHandle handle) {
            _rootLayerHandler.RemoveExtensionLayer(handle);
        }

        /// <summary>
        /// 拡張レイヤーの削除
        /// </summary>
        public void RemoveExtensionLayers() {
            _rootLayerHandler.RemoveExtensionLayers();
        }

        /// <summary>
        /// 現在フレーム分のシミュレーション更新
        /// </summary>
        private void UpdateSimulation(float deltaTime) {
            _rootLayerHandler.Update(deltaTime);
            JobConnector.Update(deltaTime);
        }

        /// <summary>
        /// 蓄積済みの時間を反映
        /// </summary>
        private void FlushPendingTime() {
            if (_pendingSimulationDelta <= 0.0f && _pendingEvaluateDelta <= 0.0f) {
                return;
            }

            UpdateSimulation(_pendingSimulationDelta);
            _graph.Evaluate(_pendingEvaluateDelta);
            _pendingSimulationDelta = 0.0f;
            _pendingEvaluateDelta = 0.0f;
        }

        /// <summary>
        /// 更新モードに応じた経過時間の取得
        /// </summary>
        private float GetElapsedTime(DirectorUpdateMode updateMode) {
            if (updateMode != DirectorUpdateMode.DSPClock) {
                var deltaTime = updateMode == DirectorUpdateMode.UnscaledGameTime ? Time.unscaledDeltaTime : Time.deltaTime;
                return deltaTime;
            }

            // DSPClock は Graph が DSP 時刻で進むため、同じ時間源で差分を取って同期を保つ
            var currentClockTime = AudioSettings.dspTime;
            if (!_hasClockTimeSample) {
                _previousClockTime = currentClockTime;
                _hasClockTimeSample = true;
                return 0.0f;
            }

            var delta = Math.Max(0.0, currentClockTime - _previousClockTime);
            _previousClockTime = currentClockTime;
            return (float)delta;
        }

        /// <summary>
        /// DSPClock 用のサンプル状態をリセット
        /// </summary>
        private void ResetClockTimeSample() {
            _previousClockTime = 0.0;
            _hasClockTimeSample = false;
        }

        /// <summary>
        /// Graph の再生状態を切り替え
        /// </summary>
        private void SetGraphPlaying(bool isPlaying) {
            if (isPlaying) {
                if (!_graph.IsPlaying()) {
                    _graph.Play();
                }
            }
            else {
                if (_graph.IsPlaying()) {
                    _graph.Stop();
                }
            }
        }

        /// <summary>
        /// 現在のフレームで Graph を反映するか
        /// </summary>
        private bool ShouldEvaluateThisFrame() {
            return SkipFrame <= 0 || (Time.frameCount + SkipFrameOffset) % (SkipFrame + 1) == 0;
        }
    }
}
