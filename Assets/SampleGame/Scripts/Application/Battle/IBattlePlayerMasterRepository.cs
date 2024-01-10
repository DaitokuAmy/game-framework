using GameFramework.Core;
using SampleGame.Domain.Battle;

namespace SampleGame.Application.Battle {
    /// <summary>
    /// マスターデータ読み込み用リポジトリ
    /// </summary>
    public interface IBattlePlayerMasterRepository {
        /// <summary>
        /// BattlePlayerのマスターデータ読み込み
        /// </summary>
        IProcess<IBattlePlayerMaster> LoadPlayer(int id);
    }
}
