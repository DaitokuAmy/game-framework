using GameFramework.ActorSystem;
using GameFramework;
using GameFramework.PlayableSystem;

namespace SampleGameEngine {
    /// <summary>
    /// アクション可能なアクタービュー基底
    /// </summary>
    public abstract class ActionableActorView : ActorView {
        /// <summary>モーション制御クラス</summary>
        protected MotionComponent MotionComponent { get; private set; }
        /// <summary>アクション再生用クラス</summary>
        protected ActorActionPlayer ActionPlayer { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected ActionableActorView(Body body) : base(body) {
            MotionComponent = body.GetComponent<MotionComponent>();
            ActionPlayer = new ActorActionPlayer();
            ActionPlayer.FinishedEvent += FinishedActorAction;
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            ActionPlayer.Dispose();

            base.DisposeInternal();
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            // ActionPlayerResolverの登録
            AddActionPlayerHandlers(ActionPlayer, MotionComponent.Handle);
        }

        /// <inheritdoc/>

        protected override void DeactivateInternal() {
            ActionPlayer.StopCurrentAction();
            ActionPlayer.ClearHandlers();
            base.DeactivateInternal();
        }

        /// <inheritdoc/>

        protected override void UpdateActionInternal(float deltaTime) {
            // アクションの更新
            ActionPlayer.Update(deltaTime);
        }

        /// <summary>
        /// ActionPlayerHandlerの追加
        /// </summary>
        protected virtual void AddActionPlayerHandlers(ActorActionPlayer actionPlayer, MotionHandle motionHandle) {
            actionPlayer.SetHandler<AnimationClipActorAction, AnimationClipActorActionHandler>(new AnimationClipActorActionHandler(motionHandle, SequenceControllerInternal));
            actionPlayer.SetHandler<ControllerActorAction, ControllerActorActionHandler>(new ControllerActorActionHandler(motionHandle, SequenceControllerInternal));
            actionPlayer.SetHandler<TimelineActorAction, TimelineActorActionHandler>(new TimelineActorActionHandler(motionHandle, SequenceControllerInternal));
            actionPlayer.SetHandler<SequentialClipActorAction, SequentialClipActorActionHandler>(new SequentialClipActorActionHandler(motionHandle, SequenceControllerInternal));
            actionPlayer.SetHandler<ReactionLoopClipActorAction, ReactionLoopClipActorActionHandler>(new ReactionLoopClipActorActionHandler(motionHandle, SequenceControllerInternal));
            actionPlayer.SetHandler<TimerActorAction, TimerActorActionHandler>(new TimerActorActionHandler(SequenceControllerInternal));
        }

        /// <summary>
        /// アクション終了時処理
        /// </summary>
        protected virtual void FinishedActorAction(IActorAction action, float outBlend) {
        }
    }
}