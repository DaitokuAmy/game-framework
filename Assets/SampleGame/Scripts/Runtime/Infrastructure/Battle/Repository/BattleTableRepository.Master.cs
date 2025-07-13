using SampleGame.Domain.Battle;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// バトル用テーブルデータ管理クラス - マスター定義
    /// </summary>
    partial class BattleTableRepository {
        /// <summary>
        /// バトルマスター
        /// </summary>
        private class BattleMaster : IBattleMaster {
            private readonly BattleTableData.Element _battle;
            private readonly BattleFieldTableData.Element _battleField;
            
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public BattleMaster(BattleTableData.Element battle, BattleFieldTableData.Element battleField) {
                _battle = battle;
                _battleField = battleField;
            }

            int IBattleMaster.Id => _battle.Id;
            string IBattleMaster.Name => _battle.Name;
            IFieldMaster IBattleMaster.FieldMaster => _battleField;
        }

        /// <summary>
        /// バトルマスターの生成
        /// </summary>
        private BattleMaster CreateBattleMaster(BattleTableData.Element battle) {
            var fieldId = battle.FieldId;
            var battleField = _battleFieldTableData.FindById(fieldId);
            return new BattleMaster(battle, battleField);
        }
    }
}