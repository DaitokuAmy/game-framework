using System.Collections.Generic;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用プレイヤーマスターデータ用インターフェース
    /// </summary>
    public interface IBattlePlayerMaster {
        /// <summary>アクター制御用データアセットキー</summary>
        string ActorSetupDataAssetKey { get; }
        /// <summary>最大体力</summary>
        int HealthMax { get; }
        /// <summary>スキルマスターリスト</summary>
        IReadOnlyList<IBattleSkillMaster> SkillMasters { get; }
    }
}