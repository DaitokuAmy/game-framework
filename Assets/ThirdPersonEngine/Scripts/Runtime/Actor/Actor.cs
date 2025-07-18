using System.Collections;
using System.Threading;
using ActionSequencer;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework;

namespace ThirdPersonEngine {
    /// <summary>
    /// アクター基底
    /// </summary>
    public abstract class Actor : GameFramework.ActorSystems.Actor {
        private readonly CoroutineRunner _coroutineRunner;

        /// <summary>外部公開用のシーケンス制御クラス</summary>
        public IReadOnlySequenceController SequenceController => SequenceControllerInternal;
        
        /// <summary>シーケンス制御クラス</summary>
        protected SequenceController SequenceControllerInternal { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Actor(Body body) : base(body) {
            SequenceControllerInternal = new SequenceController();
            _coroutineRunner = new CoroutineRunner();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _coroutineRunner.Dispose();
            SequenceControllerInternal.Dispose();
            base.DisposeInternal();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();
            
            var deltaTime = Body.DeltaTime;
            
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
        protected SequenceHandle PlaySequenceAsync(SequenceClip clip, float startOffset = 0.0f) {
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