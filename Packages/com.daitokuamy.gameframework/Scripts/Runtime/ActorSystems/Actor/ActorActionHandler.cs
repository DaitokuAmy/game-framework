using System.Collections;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorActionハンドリングクラスの基底
    /// </summary>
    public abstract class ActorActionHandler<TAction> : IActorActionHandler
        where TAction : IActorAction {
        /// <summary>現在の変移時間</summary>
        protected float DeltaTime { get; private set; }

        /// <inheritdoc/>
        void IActorActionHandler.SetDeltaTime(float deltaTime) {
            DeltaTime = deltaTime;
        }

        /// <inheritdoc/>
        IEnumerator IActorActionHandler.PlayRoutine(IActorAction action) {
            return PlayRoutineInternal((TAction)action);
        }

        /// <inheritdoc/>
        void IActorActionHandler.Exit(IActorAction action) {
            ExitInternal((TAction)action);
        }

        /// <inheritdoc/>
        void IActorActionHandler.Cancel(IActorAction action) {
            CancelInternal((TAction)action);
        }

        /// <inheritdoc/>
        bool IActorActionHandler.Next(object[] args) {
            return NextInternal(args);
        }

        /// <inheritdoc/>
        float IActorActionHandler.GetOutBlendDuration(IActorAction action) {
            return GetOutBlendDurationInternal((TAction)action);
        }

        /// <summary>
        /// 再生処理
        /// </summary>
        protected abstract IEnumerator PlayRoutineInternal(TAction action);

        /// <summary>
        /// 終了処理
        /// </summary>
        protected virtual void ExitInternal(TAction action) {
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected virtual void CancelInternal(TAction action) {
        }

        /// <summary>
        /// アクションの遷移
        /// </summary>
        protected virtual bool NextInternal(object[] args) {
            return false;
        }

        /// <summary>
        /// 戻りブレンド時間の取得
        /// </summary>
        protected virtual float GetOutBlendDurationInternal(TAction action) {
            return 0.0f;
        }

        /// <summary>
        /// 指定時間待ち
        /// </summary>
        protected IEnumerator WaitForSeconds(float seconds) {
            var timer = seconds;
            while (timer > 0.0f) {
                yield return null;
                timer -= DeltaTime;
            }
        }
    }
}