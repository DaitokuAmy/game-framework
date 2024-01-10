using GameFramework.Core;

namespace SampleGame.Domain.Common {
    /// <summary>
    /// マスターデータ読み込み用リポジトリ
    /// </summary>
    public interface IPlayerMasterRepository {
        /// <summary>
        /// Playerのマスターデータ読み込み
        /// </summary>
        IProcess<IPlayerMaster> LoadPlayer(int id);
    }
}
