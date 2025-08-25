namespace GameFramework.Core {
    /// <summary>
    /// Unity用のDeltaTimeProvider
    /// </summary>
    public class FixedDeltaTimeProvider : IDeltaTimeProvider {
        private readonly float _deltaTime;
        
        /// <inheritdoc/>
        float IDeltaTimeProvider.DeltaTime => _deltaTime;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fps">フレームレート</param>
        public FixedDeltaTimeProvider(int fps) {
            _deltaTime = 1.0f / fps;
        }
    }
}
