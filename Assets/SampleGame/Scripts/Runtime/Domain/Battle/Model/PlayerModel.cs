using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyPlayerModel : IReadOnlyCharacterModel {
        /// <summary>Playerマスター</summary>
        IPlayerMaster PlayerMaster { get; }
    }

    /// <summary>
    /// キャラモデル
    /// </summary>
    public sealed class PlayerModel : CharacterModel, IReadOnlyPlayerModel {
        /// <inheritdoc/>
        public override ICharacterMaster Master => PlayerMaster;
        
        /// <inheritdoc/>
        public IPlayerMaster PlayerMaster { get; private set; }
        
        /// <summary>
        /// 生成処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(IPlayerMaster master) {
            PlayerMaster = master;
        }
    }
}