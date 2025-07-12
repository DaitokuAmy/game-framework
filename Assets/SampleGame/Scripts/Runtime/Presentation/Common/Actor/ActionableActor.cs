using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.Core;
using GameFramework.PlayableSystems;
using SampleGame.Infrastructure;

namespace SampleGame.Presentation {
    /// <summary>
    /// アクション可能なアクター基底
    /// </summary>
    public abstract class ActionableActor : Actor {
        /// <summary>モーション制御クラス</summary>
        protected MotionComponent MotionComponent { get; private set; }
        /// <summary>アクション再生用クラス</summary>
        protected ActorActionPlayer ActionPlayer { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected ActionableActor(Body body) : base(body) {
            MotionComponent = body.GetComponent<MotionComponent>();
            ActionPlayer = new ActorActionPlayer();
            ActionPlayer.FinishedEvent += FinishedActorAction;
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
            AddActionPlayerHandlers(ActionPlayer, MotionComponent.Handle);
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            ActionPlayer.StopCurrentAction();
            ActionPlayer.ClearHandlers();
            base.DeactivateInternal();
        }

        /// <summary>
        /// アクションの更新タイミング
        /// </summary>
        protected override void UpdateActionInternal(float deltaTime) {
            // アクションの更新
            ActionPlayer.Update(deltaTime);
        }

        /// <summary>
        /// ActionPlayerHandlerの追加
        /// </summary>
        protected virtual void AddActionPlayerHandlers(ActorActionPlayer actionPlayer, MotionHandle motionHandle) {
            var sequenceController = (SequenceController)SequenceController;
            actionPlayer.SetHandler<AnimationClipActorAction, AnimationClipActorActionHandler>(new AnimationClipActorActionHandler(motionHandle, sequenceController));
            actionPlayer.SetHandler<ControllerActorAction, ControllerActorActionHandler>(new ControllerActorActionHandler(motionHandle, sequenceController));
            actionPlayer.SetHandler<TimelineActorAction, TimelineActorActionHandler>(new TimelineActorActionHandler(motionHandle, sequenceController));
            actionPlayer.SetHandler<SequentialClipActorAction, SequentialClipActorActionHandler>(new SequentialClipActorActionHandler(motionHandle, sequenceController));
            actionPlayer.SetHandler<ReactionLoopClipActorAction, ReactionLoopClipActorActionHandler>(new ReactionLoopClipActorActionHandler(motionHandle, sequenceController));
            actionPlayer.SetHandler<TimerActorAction, TimerActorActionHandler>(new TimerActorActionHandler(sequenceController));
        }

        /// <summary>
        /// アクション終了時処理
        /// </summary>
        protected virtual void FinishedActorAction(IActorAction action, float outBlend) {
        }
    }
}