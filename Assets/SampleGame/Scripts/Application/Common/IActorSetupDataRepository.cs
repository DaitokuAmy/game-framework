using GameFramework.Core;
using SampleGame.Domain.Battle;

namespace SampleGame.Application.Common {
    /// <summary>
    /// アクター初期化データ読み込み用リポジトリ
    /// </summary>
    public interface IActorSetupDataRepository {
        /// <summary>
        /// Battle用のCharacterActorSetupData読み込み
        /// </summary>
        IProcess<BattleCharacterActorSetupData> LoadBattleCharacterActorSetupData(string assetKey);
    }
}
