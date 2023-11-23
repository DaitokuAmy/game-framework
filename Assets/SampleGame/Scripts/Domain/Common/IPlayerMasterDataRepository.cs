using GameFramework.Core;

namespace SampleGame.Domain.Common {
    /// <summary>
    /// マスターデータ読み込み用リポジトリ
    /// </summary>
    public interface IPlayerMasterDataRepository {
        /// <summary>
        /// Playerのマスターデータ読み込み
        /// </summary>
        IProcess<IPlayerMasterData> LoadPlayer(int id);
    }
}
