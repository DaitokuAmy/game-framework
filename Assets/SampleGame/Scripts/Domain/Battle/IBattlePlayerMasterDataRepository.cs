using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// マスターデータ読み込み用リポジトリ
    /// </summary>
    public interface IBattlePlayerMasterDataRepository {
        /// <summary>
        /// BattlePlayerのマスターデータ読み込み
        /// </summary>
        IProcess<IBattlePlayerMasterData> LoadPlayer(int id);
    }
}
