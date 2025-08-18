using GameFramework;
using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyBattleModel {
        /// <summary>マスター</summary>
        IBattleMaster Master { get; }
        /// <summary>時間管理用モデル</summary>
        IReadOnlyBattleTimeModel TimeModel { get; }
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
        public IReadOnlyBattleTimeModel TimeModel => TimeModelInternal;
        /// <inheritdoc/>
        public IReadOnlyPlayerModel PlayerModel => PlayerModelInternal;
        /// <inheritdoc/>
        public IReadOnlyFieldModel FieldModel => FieldModelInternal;
        
        /// <summary>時間管理用モデル</summary>
        public BattleTimeModel TimeModelInternal { get; private set; }
        /// <summary>プレイヤー用モデル</summary>
        internal PlayerModel PlayerModelInternal { get; private set; }
        /// <summary>フィールド用モデル</summary>
        internal FieldModel FieldModelInternal { get; private set; }
        /// <summary>シーケンス管理用FSM</summary>
        internal FiniteStateMachine<BattleSequenceType> StateMachine { get; private set; }

        /// <inheritdoc/>
        protected override void OnCreatedInternal(IScope scope) {
            StateMachine = new FiniteStateMachine<BattleSequenceType>().RegisterTo(scope);
            
            TimeModelInternal = new BattleTimeModel().RegisterTo(scope);
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        public void Setup(IBattleMaster master, IState<BattleSequenceType>[] states) {
            Master = master;
            StateMachine.Setup(BattleSequenceType.Invalid, states);
        }

        /// <summary>
        /// ステート更新処理
        /// </summary>
        public void UpdateState(float deltaTime) {
            StateMachine.Update(deltaTime);
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