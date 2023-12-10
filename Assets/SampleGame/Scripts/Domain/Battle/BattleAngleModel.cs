using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル中のカメラアングル管理用モデル
    /// </summary>
    public class BattleAngleModel : AutoIdModel<BattleAngleModel> {
        private FloatReactiveProperty _angle = new FloatReactiveProperty();

        // カメラの回転角度(Y軸)
        public IReadOnlyReactiveProperty<float> Angle => _angle;

        /// <summary>
        /// カメラアングルの設定
        /// </summary>
        /// <param name="angle">角度(Euler)</param>
        public void SetAngle(float angle) {
            _angle.Value = angle;
        }

        /// <summary>
        /// カメラアングルの加算
        /// </summary>
        /// <param name="angle">加算角度(Euler)</param>
        public void AddAngle(float angle) {
            SetAngle(_angle.Value + angle);
        }

        private BattleAngleModel(int id) : base(id) {}
    }
}