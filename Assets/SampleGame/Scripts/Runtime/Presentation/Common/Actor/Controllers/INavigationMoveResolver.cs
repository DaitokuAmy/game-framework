namespace SampleGame.Presentation {
    /// <summary>
    /// ナビゲーション移動解決用インターフェース
    /// </summary>
    public interface INavigationMoveResolver : IMoveResolver {
        /// <summary>
        /// ナビゲーター移動
        /// </summary>
        /// <param name="navigator">ナビゲーター</param>
        /// <param name="speedMultiplier">速度係数</param>
        /// <param name="arrivedDistance">到着判定距離</param>
        /// <param name="updateRotation">回転制御を行うか</param>
        void Move(IActorNavigator navigator, float speedMultiplier, float arrivedDistance, bool updateRotation);
    }
}
