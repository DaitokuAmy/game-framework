using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.PlayableSystems;

namespace SampleGame.Presentation {
    /// <summary>
    /// アクション可能なアクター基底
    /// </summary>
    public abstract class ActionableActor : Actor {
        /// <summary>モーション制御クラス</summary>
        protected MotionController MotionController { get; private set; }
        /// <summary>アクション再生用クラス</summary>
        protected ActorActionPlayer ActionPlayer { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected ActionableActor(Body body) : base(body) {
            MotionController = body.GetController<MotionController>();
            ActionPlayer = new ActorActionPlayer(body.LayeredTime);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            ActionPlayer.Dispose();
            
            base.DisposeInternal();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            
            // ActionPlayerResolverの登録
            AddActionPlayerResolvers(ActionPlayer, MotionController.Handle);
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            ActionPlayer.RemoveResolvers();
            
            base.DeactivateInternal();
        }

        /// <summary>
        /// アクションの更新タイミング
        /// </summary>
        protected override void UpdateActionInternal(float deltaTime) {
            // アクションの更新
            ActionPlayer.Update();
        }

        /// <summary>
        /// ActionPlayerResolverの追加
        /// </summary>
        protected virtual void AddActionPlayerResolvers(ActorActionPlayer actionPlayer, MotionHandle motionHandle) {
            actionPlayer.AddResolver(new ClipActorActionResolver(motionHandle, (SequenceController)SequenceController));
            actionPlayer.AddResolver(new ControllerActorActionResolver(motionHandle, (SequenceController)SequenceController));
            actionPlayer.AddResolver(new TimelineActorActionResolver(motionHandle, (SequenceController)SequenceController));
            actionPlayer.AddResolver(new SequentialClipActorActionResolver(motionHandle, (SequenceController)SequenceController));
            actionPlayer.AddResolver(new ReactionLoopClipActorActionResolver(motionHandle, (SequenceController)SequenceController));
            actionPlayer.AddResolver(new TimerActorActionResolver((SequenceController)SequenceController));
        }
    }
}