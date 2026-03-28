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
        public MotionPlayer(Animator animator, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime,
            ushort outputSortingOrder = 0) {
            Animator = animator;
            
            _graph = PlayableGraph.Create($"{nameof(MotionPlayer)}({animator.name})");
            
            var output = AnimationPlayableOutput.Create(_graph, "Output", animator);

            _graph.SetTimeUpdateMode(updateMode);
            output.SetSortingOrder(outputSortingOrder);
            
            // RootMotionLayerを生成して接続
            _rootLayerHandler = new MotionLayerHandler(animator);
            output.SetSourcePlayable(_rootLayerHandler.CreatePlayable(_graph));
            
            // 再生状態にする
            _graph.Play();

            // JobPlayerの生成
            JobConnector = new AnimationJobConnector(animator, _graph);
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
            
            // AnimationSkip対応
            var graphPlaying = _graph.IsPlaying();
            if (SkipFrame <= 0 || (Time.frameCount + SkipFrameOffset) % (SkipFrame + 1) == 0) {
                if (!graphPlaying) {
                    _graph.Play();
                }
            }
            else {
                if (graphPlaying) {
                    _graph.Stop();
                }
            }

            // 変位時間取得
            var updateMode = _graph.GetTimeUpdateMode();
            var deltaTime = (updateMode == DirectorUpdateMode.UnscaledGameTime ? Time.unscaledDeltaTime : Time.deltaTime) * _speed;
            
            // レイヤーの更新
            _rootLayerHandler.Update(deltaTime);

            // JobProvider更新
            JobConnector.Update(deltaTime);

            // Manualモードの場合、ここで骨の更新を行う
            if (updateMode == DirectorUpdateMode.Manual) {
                _graph.Evaluate(deltaTime);
            }
        }

        /// <summary>
        /// 更新モードの変更
        /// </summary>
        public void SetUpdateMode(DirectorUpdateMode updateMode) {
            _graph.SetTimeUpdateMode(updateMode);
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        public void SetSpeed(float speed) {
            JobConnector.SetSpeed(speed);
            _rootLayerHandler.SetSpeed(speed);

            if (Math.Abs(speed - _speed) <= float.Epsilon) {
                return;
            }

            _speed = Mathf.Max(0.0f, speed);
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
    }
}
