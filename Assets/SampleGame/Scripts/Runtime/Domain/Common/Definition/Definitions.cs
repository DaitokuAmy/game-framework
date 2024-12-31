using UnityEngine;

namespace SampleGame.Domain {
    /// <summary>
    /// 各種共通定義
    /// </summary>
    public static class Definitions {
    }

    /// <summary>
    /// 座標提供用のプロバイダー
    /// </summary>
    public interface IPositionProvider {
        Vector3 Position { get; }
    }

    /// <summary>
    /// メインクエストのステートタイプ
    /// </summary>
    public enum MainQuestStateType {
        /// <summary>メインクエストを実行していない状態</summary>
        NotPlaying = 0,
        /// <summary>メインクエストを開始している状態</summary>
        Started,
        /// <summary>メインクエストのエリア選択の状態</summary>
        AreaSelecting,
    }

    /// <summary>
    /// メインクエストのエリアのステートタイプ
    /// </summary>
    public enum MainQuestAreaStateType {
        /// <summary>無効値</summary>
        Invalid = -1,
        /// <summary>未解放</summary>
        NotReleased,
        /// <summary>解放演出中</summary>
        BeingRelease,
        /// <summary>解放済</summary>
        Released,
        /// <summary>クリア演出中</summary>
        BeingClear,
        /// <summary>クリア済</summary>
        Cleared,
        /// <summary>無効済</summary>
        Disabled,
    }
    
    /// <summary>
    /// バトルユニットの役割タイプ
    /// </summary>
    public enum UnitRoleType {
        /// <summary>なし</summary>
        None = -1,
        /// <summary>タンク</summary>
        Tank,
        /// <summary>アサルト</summary>
        Assault,
        /// <summary>サポート</summary>
        Support,
        /// <summary>シューター</summary>
        Shooter,
    }

    /// <summary>
    /// バトルユニットのポジショニング
    /// </summary>
    public enum UnitPositionType {
        /// <summary>前衛</summary>
        Front,
        /// <summary>中衛</summary>
        Middle,
        /// <summary>後衛</summary>
        Back,
    }

    /// <summary>
    /// スキルの攻撃タイプ
    /// </summary>
    public enum SkillActionType {
        /// <summary>流し切りスキル</summary>
        OneShot,
        /// <summary>ループスキル</summary>
        Loop,
        /// <summary>スペシャルスキル</summary>
        SpecialSkill,
    }

    /// <summary>
    /// スキルの攻撃タイプ
    /// </summary>
    public enum SkillAttackType {
        /// <summary>物理</summary>
        Physical,
        /// <summary>エネルギー</summary>
        Energy,
    }

    /// <summary>
    /// スキルの射程タイプ
    /// </summary>
    public enum SkillRangeType {
        /// <summary>近距離</summary>
        Short,
        /// <summary>中距離</summary>
        Middle,
        /// <summary>遠距離</summary>
        Long,
    }

    /// <summary>
    /// スキルの実行対象
    /// </summary>
    public enum SkillTargetType {
        /// <summary>自分自身</summary>
        Self,
        /// <summary>攻撃対象</summary>
        AttackTarget,
        /// <summary>支援対象</summary>
        SupportTarget,
        /// <summary>自陣</summary>
        Owns,
        /// <summary>敵陣</summary>
        Opponents,
    }

    /// <summary>
    /// スキル効果タイプ
    /// </summary>
    public enum SkillEffectType {
        /// <summary>固定値回復</summary>
        FixedHeal,
        /// <summary>割合回復</summary>
        RatioHeal,
        /// <summary>ステータス効果</summary>
        StatusEffect,
        /// <summary>ガード</summary>
        Guard,
        /// <summary>スキル中パッシブの付与</summary>
        PassiveSkill,
    }

    /// <summary>
    /// スキルタグ
    /// </summary>
    public enum SkillTag {
        /// <summary>斬撃</summary>
        Blade,
        /// <summary>刺突</summary>
        Thrust,
        /// <summary>弾</summary>
        Bullet,
        /// <summary>爆発</summary>
        Explosion,
    }
}