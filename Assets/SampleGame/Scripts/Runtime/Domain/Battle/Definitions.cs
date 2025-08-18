namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトルシーケンスタイプ
    /// </summary>
    public enum BattleSequenceType {
        /// <summary>無効値</summary>
        Invalid = -1,
        /// <summary>開始演出</summary>
        Enter,
        /// <summary>プレイ中</summary>
        Playing,
        /// <summary>一時停止中</summary>
        Pausing,
        /// <summary>終了演出</summary>
        Finish,
    }

    /// <summary>
    /// バトル内時間軸タイプ
    /// </summary>
    public enum BattleTimeType {
        System,
        Base,
        Logic,
        ViewPrimary,
        ViewSecondary,
    }
    
    /// <summary>
    /// 各種共通定義
    /// </summary>
    public static class Definitions {
    }
}