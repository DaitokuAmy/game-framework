using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyBattleTimeModel {
        /// <summary>
        /// 管理されたLayeredTimeの取得
        /// </summary>
        IReadOnlyLayeredTime GetLayeredTime(BattleTimeType type);

        /// <summary>
        /// DeltaTimeの取得
        /// </summary>
        float GetDeltaTime(BattleTimeType type);
    }

    /// <summary>
    /// バトル時間軸管理用モデル
    /// </summary>
    public class BattleTimeModel : LocalModel<BattleTimeModel>, IReadOnlyBattleTimeModel {
        private LayeredTime _system;
        private LayeredTime _base;
        private LayeredTime _logic;
        private LayeredTime _viewPrimary;
        private LayeredTime _viewSecondary;

        /// <inheritdoc/>
        public IReadOnlyLayeredTime GetLayeredTime(BattleTimeType type) {
            return GetLayeredTimeInternal(type);
        }

        /// <inheritdoc/>
        public float GetDeltaTime(BattleTimeType type) {
            return GetLayeredTime(type)?.DeltaTime ?? LayeredTime.DefaultProvider.DeltaTime;
        }

        /// <inheritdoc/>
        protected override void OnCreatedInternal(IScope scope) {
            _system = new LayeredTime();
            _base = new LayeredTime(_system);
            _logic = new LayeredTime(_base);
            _viewPrimary = new LayeredTime(_base);
            _viewSecondary = new LayeredTime(_base);
        }

        /// <summary>
        /// TimeScaleの設定
        /// </summary>
        public void SetTimeScale(BattleTimeType type, float timeScale) {
            var layeredTime = GetLayeredTimeInternal(type);
            if (layeredTime != null) {
                layeredTime.LocalTimeScale = timeScale;
            }
        }

        /// <summary>
        /// TimeScaleのリセット
        /// </summary>
        public void ResetTimeScale(BattleTimeType type) {
            SetTimeScale(type, 1.0f);
        }

        /// <summary>
        /// 管理されたLayeredTimeの取得
        /// </summary>
        private LayeredTime GetLayeredTimeInternal(BattleTimeType type) {
            switch (type) {
                case BattleTimeType.System:
                    return _system;
                case BattleTimeType.Base:
                    return _base;
                case BattleTimeType.Logic:
                    return _logic;
                case BattleTimeType.ViewPrimary:
                    return _viewPrimary;
                case BattleTimeType.ViewSecondary:
                    return _viewSecondary;
            }

            return null;
        }
    }
}