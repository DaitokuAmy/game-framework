namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用のマスター
    /// </summary>
    public interface IBattleMaster {
        /// <summary>識別子</summary>
        int Id { get; }
        /// <summary>名称</summary>
        string Name { get; }
    }
}