using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Motionを再生させるためのクラス
    /// </summary>
    public class MotionPlayer : IDisposable {
        // Playable情報
        private PlayableGraph _graph;

        // 再生速度
        private float _speed = 1.0f;

        // ルートコンポーネント
        private LayerMixerPlayableComponent _rootComponent;

        /// <summary></summary>
        public Animator Animator { get; private set; }
        /// <summary>アニメーションの更新をSkipするフレーム数(0以上)</summary>
        public int SkipFrame { get; set; } = 0;
        /// <summary>アニメーションの更新をSkipするかのフレーム数に対するOffset</summary>
        public int SkipFrameOffset { get; set; } = 0;
        /// <summary>AnimationJob差し込み用</summary>
        public AnimationJobConnector JobConnector { get; private set; }
        /// <summary>再生に使うレイヤーハンドル</summary>
        public MotionHandle Handle { get; private set; }
        /// <summary>ルートに存在するLayerMixerComponent</summary>
        public LayerMixerPlayableComponent RootComponent => _rootComponent;

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
            
            // RootComponentを生成して接続
            _rootComponent = new LayerMixerPlayableComponent(animator, false);
            ((IPlayableComponent)_rootComponent).Initialize(_graph);
            Handle = _rootComponent.BaseHandle;
            output.SetSourcePlayable(_rootComponent.Playable);
            
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
            _rootComponent?.Dispose();
            _rootComponent = null;

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
            ((IPlayableComponent)_rootComponent).Update(deltaTime);

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
            ((IPlayableComponent)RootComponent).SetSpeed(speed);

            if (Math.Abs(speed - _speed) <= float.Epsilon) {
                return;
            }

            _speed = Mathf.Max(0.0f, speed);
        }
    }
}