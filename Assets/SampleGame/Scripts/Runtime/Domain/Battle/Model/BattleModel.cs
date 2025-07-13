using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyBattleModel {
        /// <summary>マスター</summary>
        IBattleMaster Master { get; }
        /// <summary>プレイヤー用モデル</summary>
        IReadOnlyPlayerModel PlayerModel { get; }
        /// <summary>フィールド用モデル</summary>
        IReadOnlyFieldModel FieldModel { get; }
    }

    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class BattleModel : SingleModel<BattleModel>, IReadOnlyBattleModel {
        /// <inheritdoc/>
        public IBattleMaster Master { get; private set; }
        /// <inheritdoc/>
        public IReadOnlyPlayerModel PlayerModel => PlayerModelInternal;
        /// <inheritdoc/>
        public IReadOnlyFieldModel FieldModel => FieldModelInternal;
        
        /// <summary>プレイヤー用モデル</summary>
        internal PlayerModel PlayerModelInternal { get; private set; }
        /// <summary>フィールド用モデル</summary>
        internal FieldModel FieldModelInternal { get; private set; }

        /// <summary>
        /// セットアップ
        /// </summary>
        public void Setup(IBattleMaster master) {
            Master = master;
        }

        /// <summary>
        /// プレイヤーモデルの設定
        /// </summary>
        public void SetPlayerModel(PlayerModel playerModel) {
            PlayerModelInternal = playerModel;
        }

        /// <summary>
        /// フィールドモデルの設定
        /// </summary>
        public void SetFieldModel(FieldModel fieldModel) {
            FieldModelInternal = fieldModel;
        }
    }
}