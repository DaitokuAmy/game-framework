using GameFramework.Core;
using GameFramework.ModelSystems;
using SampleGame.Domain.Battle;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用モデル
    /// </summary>
    public class BattleModel : SingleModel<BattleModel> {
        public BattleAngleModel AngleModel { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattleModel(object empty) : base(empty) {}

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            AngleModel = BattleAngleModel.Create().ScopeTo(scope);
        }
    }
}