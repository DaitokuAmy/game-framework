using GameFramework.Core;
using GameFramework.ModelSystems;
using SampleGame.Domain.Battle;
using SampleGame.Domain.User;
using UniRx;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// PlayerModelの読み取り用インターフェース
    /// </summary>
    public interface IReadOnlyBattlePlayerModel {
        /// <summary>管理ID</summary>
        int Id { get; }
        
        /// <summary>名前</summary>
        string Name { get; }
        /// <summary>Prefab用AssetKey</summary>
        string PrefabAssetKey { get; }
        /// <summary>UserPlayerModelへの参照</summary>
        IReadOnlyUserPlayerModel UserPlayerModel { get; }
        /// <summary>マスターデータへの参照</summary>
        IBattlePlayerMasterData MasterData { get; }
        /// <summary>ステータス管理用モデルへの参照</summary>
        IReadOnlyBattleCharacterStatusModel StatusModel { get; }
        /// <summary>アクターモデルへの参照</summary>
        IReadOnlyBattleCharacterActorModel ActorModel { get; }
    }
    
    /// <summary>
    /// バトル用プレイヤーモデル
    /// </summary>
    public class BattlePlayerModel : AutoIdModel<BattlePlayerModel> {
        private UserPlayerModel _userPlayerModel;
        private IBattlePlayerMasterData _masterData;
        private BattleCharacterStatusModel _statusModel;
        private BattleCharacterActorModel _actorModel;
        
        private Subject<(BattlePlayerModel, int)> _onUpdatedHealthSubject = new();
        private Subject<(BattlePlayerModel, int)> _onDamagedSubject = new();
        private Subject<BattlePlayerModel> _onUpdatedSubject = new();
        private Subject<BattlePlayerModel> _onDeadSubject = new();
        private Subject<(BattlePlayerModel, string)> _onAttackSubject = new();

        /// <summary>名前</summary>
        public string Name => UserPlayerModel.PlayerModel.Name;
        /// <summary>Prefab用AssetKey</summary>
        public string PrefabAssetKey => UserPlayerModel.PlayerModel?.PrefabAssetKey;
        /// <summary>UserPlayerModelへの参照</summary>
        public IReadOnlyUserPlayerModel UserPlayerModel => _userPlayerModel;
        /// <summary>マスターデータへの参照</summary>
        public IBattlePlayerMasterData MasterData => _masterData;
        /// <summary>ステータス管理用モデルへの参照</summary>
        public IReadOnlyBattleCharacterStatusModel StatusModel => _statusModel;
        /// <summary>アクターモデルへの参照</summary>
        public IReadOnlyBattleCharacterActorModel ActorModel => _actorModel;

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            _actorModel = BattleCharacterActorModel.Create().ScopeTo(scope);
            _statusModel = BattleCharacterStatusModel.Create().ScopeTo(scope);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(UserPlayerModel userPlayerModel, IBattlePlayerMasterData masterData, BattleCharacterActorSetupData actorSetupData) {
            _userPlayerModel = userPlayerModel;
            _masterData = masterData;
            _statusModel.Setup(masterData.HealthMax);
            _actorModel.Setup(actorSetupData);
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattlePlayerModel(int id) : base(id) {
        }
    }
}