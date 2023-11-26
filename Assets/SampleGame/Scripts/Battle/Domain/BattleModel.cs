using GameFramework.Core;
using GameFramework.ModelSystems;
using SampleGame.Domain.Battle;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用モデル
    /// </summary>
    public class BattleModel : SingleModel<BattleModel> {
        public BattleAngleModel AngleModel { get; private set; }
        public BattlePlayerModel PlayerModel { get; private set; }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            PlayerModel = BattlePlayerModel.Create()
                .ScopeTo(scope);
            AngleModel = BattleAngleModel.Create()
                .ScopeTo(scope);
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        protected override void OnDeletedInternal() {
            if (PlayerModel != null) {
                BattlePlayerModel.Delete(PlayerModel.Id);
                PlayerModel = null;
            }
        }
        
        private BattleModel(object empty) : base(empty) {}
    }
}