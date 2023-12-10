using System.Collections.Generic;
using GameFramework.CommandSystems;
using GameFramework.Core;
using GameFramework.ModelSystems;
using SampleGame.Domain.User;

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
        IBattlePlayerMaster MasterData { get; }
        /// <summary>ステータス管理用モデルへの参照</summary>
        IReadOnlyBattleStatusModel StatusModel { get; }
        /// <summary>スキルモデルリストへの参照</summary>
        IReadOnlyList<IReadOnlyBattleSkillModel> SkillModels { get; }
        /// <summary>アクターモデルへの参照</summary>
        IReadOnlyBattleCharacterActorModel ActorModel { get; }
    }

    /// <summary>
    /// バトル用プレイヤーモデル
    /// </summary>
    public class BattlePlayerModel : AutoIdModel<BattlePlayerModel>, IReadOnlyBattlePlayerModel {
        private UserPlayerModel _userPlayerModel;
        private IBattlePlayerMaster _masterData;
        private BattleStatusModel _statusModel;
        private BattleSkillModel[] _skillModels;
        private BattleCharacterActorModel _actorModel;

        private CommandManager _commandManager;

        /// <summary>名前</summary>
        public string Name => UserPlayerModel.PlayerModel.Name;
        /// <summary>Prefab用AssetKey</summary>
        public string PrefabAssetKey => UserPlayerModel.PlayerModel?.PrefabAssetKey;
        /// <summary>UserPlayerModelへの参照</summary>
        public IReadOnlyUserPlayerModel UserPlayerModel => _userPlayerModel;
        /// <summary>マスターデータへの参照</summary>
        public IBattlePlayerMaster MasterData => _masterData;
        /// <summary>ステータス管理用モデルへの参照</summary>
        public IReadOnlyBattleStatusModel StatusModel => _statusModel;
        /// <summary>スキルモデルリストへの参照</summary>
        public IReadOnlyList<IReadOnlyBattleSkillModel> SkillModels => _skillModels;
        /// <summary>アクターモデルへの参照</summary>
        public IReadOnlyBattleCharacterActorModel ActorModel => _actorModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattlePlayerModel(int id) : base(id) {
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            _commandManager = new CommandManager().ScopeTo(scope);
            _actorModel = BattleCharacterActorModel.Create().ScopeTo(scope);
            _statusModel = BattleStatusModel.Create().ScopeTo(scope);
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        protected override void OnDeletedInternal() {
            Cleanup();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(UserPlayerModel userPlayerModel, IBattlePlayerMaster masterData, BattleCharacterActorSetupData actorSetupData) {
            Cleanup();
            
            _userPlayerModel = userPlayerModel;
            _masterData = masterData;
            _statusModel.Setup(masterData.HealthMax);
            _actorModel.Setup(actorSetupData);
            _skillModels = new BattleSkillModel[masterData.SkillMasters.Count];
            for (var i = 0; i < _skillModels.Length; i++) {
                _skillModels[i] = BattleSkillModel.Create();
                _skillModels[i].Setup(masterData.SkillMasters[i]);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            _commandManager.Update();
        }

        /// <summary>
        /// コマンドの追加
        /// </summary>
        public CommandHandle AddCommand(ICommand command) {
            return _commandManager.Add(command);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        private void Cleanup() {
            if (_skillModels != null) {
                for (var i = 0; i < _skillModels.Length; i++) {
                    _skillModels[i].Dispose();
                }

                _skillModels = null;
            }
        }
    }
}