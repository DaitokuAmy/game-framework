namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用スキルマスターデータ用インターフェース
    /// </summary>
    public interface IBattleSkillMaster {
        /// <summary>名称</summary>
        string Name { get; }
        /// <summary>再生するアクションキー</summary>
        string ActionKey { get; }
    }
}