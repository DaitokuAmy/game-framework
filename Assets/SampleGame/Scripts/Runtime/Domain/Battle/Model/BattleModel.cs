using GameFramework;
using GameFramework.Core;
using R3;

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
        /// <summary>現在のシーケンスタイプ</summary>
        BattleSequenceType CurrentSequenceType { get; }
        
        /// <summary>シーケンスタイプの変更通知</summary>
        Observable<BattleSequenceType> ChangedCurrentSequenceTypeSubject { get; }
    }

    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class BattleModel : SingleModel<BattleModel>, IReadOnlyBattleModel {
        private FiniteStateMachine<BattleSequenceType> _stateMachine;
        
        /// <inheritdoc/>
        public IBattleMaster Master { get; private set; }
        /// <inheritdoc/>
        public IReadOnlyBattleTimeModel TimeModel => TimeModelInternal;
        /// <inheritdoc/>
        public IReadOnlyPlayerModel PlayerModel => PlayerModelInternal;
        /// <inheritdoc/>
        public IReadOnlyFieldModel FieldModel => FieldModelInternal;
        /// <inheritdoc/>
        public BattleSequenceType CurrentSequenceType => _stateMachine.CurrentKey;

        /// <inheritdoc/>
        public Observable<BattleSequenceType> ChangedCurrentSequenceTypeSubject => _stateMachine.ChangedStateAsObservable();
        
        /// <summary>時間管理用モデル</summary>
        internal BattleTimeModel TimeModelInternal { get; private set; }
        /// <summary>プレイヤー用モデル</summary>
        internal PlayerModel PlayerModelInternal { get; private set; }
        /// <summary>フィールド用モデル</summary>
        internal FieldModel FieldModelInternal { get; private set; }

        /// <inheritdoc/>
        protected override void OnCreatedInternal(IScope scope) {
            _stateMachine = new FiniteStateMachine<BattleSequenceType>().RegisterTo(scope);
            
            TimeModelInternal = new BattleTimeModel().RegisterTo(scope);
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        public void Setup(IBattleMaster master, IState<BattleSequenceType>[] states) {
            Master = master;
            _stateMachine.Setup(BattleSequenceType.Invalid, states);
        }

        /// <summary>
        /// ステート更新処理
        /// </summary>
        public void UpdateState(float deltaTime) {
            _stateMachine.Update(deltaTime);
        }

        /// <summary>
        /// ステートの変更
        /// </summary>
        public void ChangeState(BattleSequenceType sequenceType) {
            _stateMachine.Change(sequenceType);
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