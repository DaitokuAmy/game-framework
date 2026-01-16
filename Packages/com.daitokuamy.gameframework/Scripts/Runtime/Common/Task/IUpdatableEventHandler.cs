namespace GameFramework {
    /// <summary>
    /// Updatableイベント通知用インターフェース基底
    /// </summary>
    public interface IUpdatableEventHandlerBase<TUpdatable> {
        /// <summary>
        /// 登録時処理
        /// </summary>
        void OnRegistered(UpdateScheduler runner);

        /// <summary>
        /// 登録解除時処理
        /// </summary>
        void OnUnregistered(UpdateScheduler runner);
    }

    /// <summary>
    /// Updatableイベント通知用インターフェース
    /// </summary>
    public interface IUpdatableEventHandler : IUpdatableEventHandlerBase<IUpdatable> {
    }

    /// <summary>
    /// LateUpdatableイベント通知用インターフェース
    /// </summary>
    public interface ILateUpdatableEventHandler : IUpdatableEventHandlerBase<ILateUpdatable> {
    }

    /// <summary>
    /// FixedUpdatableイベント通知用インターフェース
    /// </summary>
    public interface IFixedUpdatableEventHandler : IUpdatableEventHandlerBase<IFixedUpdatable> {
    }
}