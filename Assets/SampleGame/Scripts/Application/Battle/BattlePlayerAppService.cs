using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SampleGame.Domain.Battle;
using SampleGame.Application.Common;
using SampleGame.Domain.User;
using UnityEngine;
using GameFramework.Core;

namespace SampleGame.Application.Battle {
    /// <summary>
    /// BattlePlayer用のアプリケーションサービス
    /// </summary>
    public class BattlePlayerAppService : IDisposable {
        private IBattlePlayerMasterRepository _masterRepository;
        private IActorSetupDataRepository _actorSetupDataRepository;

        private BattlePlayerModel _battlePlayerModel;

        /// <summary>参照用のBattlePlayerModel</summary>
        public IReadOnlyBattlePlayerModel PlayerModel => _battlePlayerModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerAppService(IBattlePlayerMasterRepository masterRepository, IActorSetupDataRepository actorSetupDataRepository) {
            _masterRepository = masterRepository;
            _actorSetupDataRepository = actorSetupDataRepository;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            _battlePlayerModel.Update(deltaTime);
        }

        /// <summary>
        /// BattlePlayerの作成
        /// </summary>
        public async UniTask<int> CreateBattlePlayerAsync(UserPlayerModel userPlayerModel, int battlePlayerMasterId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var masterData = await _masterRepository.LoadPlayer(battlePlayerMasterId).ToUniTask(cancellationToken: ct);
            var actorSetupData = await _actorSetupDataRepository.LoadBattleCharacterActorSetupData(masterData.ActorSetupDataAssetKey).ToUniTask(cancellationToken: ct);
            var model = BattlePlayerModel.Create();
            model.Setup(userPlayerModel, masterData, actorSetupData);
            _battlePlayerModel = model;
            
            return model.Id;
        }

        /// <summary>
        /// スキル実行
        /// </summary>
        /// <param name="id">BattlePlayerModelのID</param>
        /// <param name="skillIndex">スキルのIndex</param>
        public void ExecuteSkill(int id, int skillIndex) {
            var model = BattlePlayerModel.Get(id);
            if (model == null) {
                return;
            }

            model.AddCommand(new PlayerSkillCommand(model, skillIndex));
        }

        /// <summary>
        /// Transformの設定
        /// </summary>
        /// <param name="id">BattlePlayerModelのID</param>
        /// <param name="position">座標</param>
        /// <param name="rotation">向き</param>
        public void SetTransform(int id, Vector3 position, Quaternion rotation) {
            var model = BattlePlayerModel.Get(id);
            if (model == null) {
                return;
            }

            if (model.ActorModel is BattleCharacterActorModel actorModel) {
                actorModel.SetPosition(position);
                actorModel.SetRotation(rotation);
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _battlePlayerModel?.Dispose();
        }
    }
}