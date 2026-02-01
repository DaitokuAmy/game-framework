using System.Collections;
using System.Threading;
using ActionSequencer;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystem;
using GameFramework;

namespace SampleGameEngine {
    /// <summary>
    /// アクタービュー基底
    /// </summary>
    public abstract class ActorView : GameFramework.ActorSystem.ActorView {
        private readonly CoroutineRunner _coroutineRunner;

        /// <summary>外部公開用のシーケンス制御クラス</summary>
        public IReadOnlySequenceController SequenceController => SequenceControllerInternal;
        
        /// <summary>シーケンス制御クラス</summary>
        protected SequenceController SequenceControllerInternal { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected ActorView(Body body) : base(body) {
            SequenceControllerInternal = new SequenceController();
            _coroutineRunner = new CoroutineRunner();
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            _coroutineRunner.Dispose();
            SequenceControllerInternal.Dispose();
            base.DisposeInternal();
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            SequenceControllerProvider.SetController(Body.GameObject, SequenceControllerInternal);
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal() {
            SequenceControllerProvider.SetController(Body.GameObject, null);
            base.DeactivateInternal();
        }

        /// <inheritdoc/>
        protected override void UpdateInternal(float deltaTime) {
            base.UpdateInternal(deltaTime);
            
            // コルーチンの更新
            _coroutineRunner.Update();
            // アクションの更新
            UpdateActionInternal(deltaTime);
            // シーケンスの再生
            SequenceControllerInternal.Update(deltaTime);
        }
        
        /// <summary>
        /// アクションの更新タイミング
        /// </summary>
        protected virtual void UpdateActionInternal(float deltaTime) {}

        /// <summary>
        /// シーケンスクリップの再生
        /// </summary>
        protected SequenceHandle PlaySequenceClip(SequenceClip clip, float startOffset = 0.0f) {
            return SequenceControllerInternal.Play(clip, startOffset);
        }

        /// <summary>
        /// コルーチンの開始
        /// </summary>
        protected UniTask StartCoroutineAsync(IEnumerator routine, CancellationToken ct) {
            return _coroutineRunner.StartCoroutineAsync(routine, ct);
        }
    }
}