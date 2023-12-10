using GameFramework.ModelSystems;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// BattleSkillModelの読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyBattleSkillModel {
        /// <summary>名称</summary>
        public string Name { get; }
        /// <summary>アクションキー</summary>
        public string ActionKey { get; }
        /// <summary>実行可能か</summary>
        public bool CanExecute { get; }
    }
    
    /// <summary>
    /// バトル用スキルモデル
    /// </summary>
    public class BattleSkillModel : AutoIdModel<BattleSkillModel>, IReadOnlyBattleSkillModel {
        private IBattleSkillMaster _master;

        /// <summary>名称</summary>
        public string Name => _master.Name;
        /// <summary>アクションキー</summary>
        public string ActionKey => _master.ActionKey;
        /// <summary>実行可能か</summary>
        public bool CanExecute => true;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattleSkillModel(int id) : base(id) {}

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(IBattleSkillMaster master) {
            _master = master;
        }

        /// <summary>
        /// 発動
        /// </summary>
        public bool Invoke() {
            if (!CanExecute) {
                return false;
            }

            return true;
        }
    }
}