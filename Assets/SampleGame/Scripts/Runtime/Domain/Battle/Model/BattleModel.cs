using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyBattleModel {
        /// <summary>マスター</summary>
        IBattleMaster Master { get; }
    }

    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class BattleModel : SingleModel<BattleModel>, IReadOnlyBattleModel {
        /// <inheritdoc/>
        public IBattleMaster Master { get; private set; }
        
        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        public void Setup(IBattleMaster master) {
            Master = master;
        }
    }
}