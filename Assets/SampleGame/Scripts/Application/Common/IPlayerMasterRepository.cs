using GameFramework.Core;
using SampleGame.Domain.Common;

namespace SampleGame.Application.Common {
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
