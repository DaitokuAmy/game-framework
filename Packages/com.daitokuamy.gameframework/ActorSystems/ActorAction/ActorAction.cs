namespace GameFramework.ActorSystems {
    /// <summary>
    /// アクター用のアクションインターフェース
    /// </summary>
    public interface IActorAction {
        /// <summary>戻る際のブレンド時間</summary>
        float OutBlend { get; }
    }
}