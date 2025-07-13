using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// TableDataを管理するRepositoryのインタフェース
    /// </summary>
    public interface IBattleTableRepository {
        /// <summary>
        /// 関連テーブルの読み込み
        /// </summary>
        UniTask LoadTablesAsync(CancellationToken ct);
        
        /// <summary>
        /// BattleMasterの取得
        /// </summary>
        IBattleMaster FindBattleById(int id);

        /// <summary>
        /// PlayerMasterの取得
        /// </summary>
        IPlayerMaster FindPlayerById(int id);
    }
}