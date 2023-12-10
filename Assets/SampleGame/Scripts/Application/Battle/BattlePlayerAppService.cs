using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SampleGame.Domain.Battle;
using SampleGame.Domain.Common;
using SampleGame.Domain.User;
using UnityEngine;
using GameFramework.Core;

namespace SampleGame.Application.Battle {
    /// <summary>
    /// BattlePlayer用のアプリケーションサービス
    /// </summary>
    public class BattlePlayerAppService : IDisposable {
        private IBattlePlayerMasterDataRepository _masterDataRepository;
        private IActorSetupDataRepository _actorSetupDataRepository;

        private BattlePlayerModel _battlePlayerModel;

        /// <summary>参照用のBattlePlayerModel</summary>
        public IReadOnlyBattlePlayerModel PlayerModel => _battlePlayerModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerAppService(IBattlePlayerMasterDataRepository masterDataRepository, IActorSetupDataRepository actorSetupDataRepository) {
            _masterDataRepository = masterDataRepository;
            _actorSetupDataRepository = actorSetupDataRepository;
        }

        /// <summary>
        /// BattlePlayerの作成
        /// </summary>
        public async UniTask<int> CreateBattlePlayerAsync(UserPlayerModel userPlayerModel, int battlePlayerMasterId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var masterData = await _masterDataRepository.LoadPlayer(battlePlayerMasterId).ToUniTask(cancellationToken: ct);
            var actorSetupData = await _actorSetupDataRepository.LoadBattleCharacterActorSetupData(masterData.ActorSetupDataAssetKey).ToUniTask(cancellationToken: ct);
            var model = BattlePlayerModel.Create();
            model.Setup(userPlayerModel, masterData, actorSetupData);
            _battlePlayerModel = model;
            
            return model.Id;
        }

        /// <summary>
        /// 汎用アクションの再生
        /// </summary>
        /// <param name="id">BattlePlayerModelのID</param>
        /// <param name="actionIndex">アクションのIndex</param>
        public void PlayGeneralAction(int id, int actionIndex) {
            var model = BattlePlayerModel.Get(id);
            if (model == null) {
                return;
            }
        }

        /// <summary>
        /// ジャンプの再生
        /// </summary>
        /// <param name="id">BattlePlayerModelのID</param>
        public void PlayJumpAction(int id) {
            var model = BattlePlayerModel.Get(id);
            if (model == null) {
                return;
            }
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
            BattlePlayerModel.Reset();
        }
    }
}