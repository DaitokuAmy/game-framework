namespace SampleGame.Domain.Common {
    /// <summary>
    /// 属性タイプ
    /// </summary>
    public enum ElementType {
        None = -1,
        Fire,   // 炎
        Water,  // 水
        Wind,   // 風
        Earth,  // 土
    }
    
    /// <summary>
    /// 防具タイプ
    /// </summary>
    public enum ArmorType {
        None = -1,
        Helm,   // 頭防具
        Body,   // 体防具
        Arms,   // 腕防具
        Legs,   // 脚防具
    }
}
