using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Infrastructure;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用のドメインサービス
    /// </summary>
    public class BattleDomainService : IDisposable {
        private readonly IModelRepository _modelRepository;
        private readonly ICharacterActorFactory _characterActorFactory;
        private readonly IFieldActorFactory _fieldActorFactory;
        private readonly BattleModel _battleModel;

        private DisposableScope _scope;

        /// <summary>バトルモデル</summary>
        public IReadOnlyBattleModel BattleModel => _battleModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleDomainService() {
            _modelRepository = Services.Resolve<IModelRepository>();
            _characterActorFactory = Services.Resolve<ICharacterActorFactory>();
            _fieldActorFactory = Services.Resolve<IFieldActorFactory>();
            _battleModel = _modelRepository.GetSingleModel<BattleModel>();

            _scope = new DisposableScope();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _scope?.Dispose();
            _scope = null;
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        public async UniTask SetupAsync(IBattleMaster master, IPlayerMaster playerMaster, CancellationToken ct) {
            // バトル初期化
            _battleModel.Setup(master);

            // フィールドの生成
            await CreateFieldAsync(master.FieldMaster, ct);

            // プレイヤーの生成
            await CreatePlayerAsync(playerMaster, ct);
        }

        /// <summary>
        /// クリーンアップ
        /// </summary>
        public void Cleanup() {
            // プレイヤーの削除
            DeleteCurrentPlayer();
            
            // フィールドの削除
            DeleteCurrentField();
        }

        /// <summary>
        /// プレイヤーの生成
        /// </summary>
        private async UniTask CreatePlayerAsync(IPlayerMaster master, CancellationToken ct) {
            // 現在のPlayerを削除
            DeleteCurrentPlayer();

            // モデルの生成
            var playerModel = _modelRepository.CreateAutoIdModel<CharacterModel, PlayerModel>();
            playerModel.Setup(master);

            // アクターの生成
            var port = await _characterActorFactory.CreatePlayerAsync(playerModel, null, ct);
            playerModel.ActorModelInternal.Setup(port);

            // モデルの登録
            _battleModel.SetPlayerModel(playerModel);
        }

        /// <summary>
        /// 現在のプレイヤーの削除
        /// </summary>
        private void DeleteCurrentPlayer() {
            var playerModel = _battleModel.PlayerModelInternal;
            if (playerModel == null) {
                return;
            }

            // 登録解除
            _battleModel.SetPlayerModel(null);

            // アクターの削除
            _characterActorFactory.DestroyPlayer(playerModel);

            // モデルの削除
            _modelRepository.DeleteAutoIdModel<CharacterModel>(playerModel);
        }

        /// <summary>
        /// フィールドの生成
        /// </summary>
        private async UniTask CreateFieldAsync(IFieldMaster master, CancellationToken ct) {
            // 現在のFieldを削除
            DeleteCurrentField();

            // モデルの生成
            var fieldModel = _modelRepository.CreateAutoIdModel<FieldModel>(5001);
            fieldModel.Setup(master);

            // アクターの生成
            var port = await _fieldActorFactory.CreateAsync(fieldModel, ct);
            fieldModel.ActorModelInternal.Setup(port);

            // モデルの登録
            _battleModel.SetFieldModel(fieldModel);
        }

        /// <summary>
        /// 現在のフィールドの削除
        /// </summary>
        private void DeleteCurrentField() {
            var fieldModel = _battleModel.FieldModelInternal;
            if (fieldModel == null) {
                return;
            }

            // 登録解除
            _battleModel.SetFieldModel(null);

            // アクターの削除
            _fieldActorFactory.Destroy(fieldModel);

            // モデルの削除
            _modelRepository.DeleteAutoIdModel(fieldModel);
        }
    }
}