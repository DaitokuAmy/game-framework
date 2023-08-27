namespace SampleGame {
    /// <summary>
    /// タスクの実行順
    /// </summary>
    public enum TaskOrder {
        PreSystem,
        Input,
        Scene,
        Logic,
        Actor,
        Cutscene,
        Body,
        Projectile,
        Collision,
        Camera,
        UI,
        Effect,
        PostSystem,
    }
    
    /// <summary>
    /// AssetProviderのタイプ
    /// </summary>
    public enum AssetProviderType {
        AssetDatabase,
        Resources,
    }

    /// <summary>
    /// 攻撃タイプ
    /// </summary>
    public enum AttackType {
        Physical,   // 物理
        Magical,    // 魔法
    }

    /// <summary>
    /// 属性タイプ
    /// </summary>
    public enum ElementType {
        None,
        Fire,   // 炎
        Water,  // 水
        Wind,   // 風
        Earth,  // 土
    }

    /// <summary>
    /// 防具タイプ
    /// </summary>
    public enum ArmorType {
        Helm,   // 頭
        Body,   // 体
        Arms,   // 腕
        Legs,   // 脚
    }

    /// <summary>
    /// ゲーム用の定義処理
    /// </summary>
    public static class Definitions {
    }
}