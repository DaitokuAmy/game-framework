using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用のドメインサービス
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class BattleDomainService : IDisposable {
        private DisposableScope _scope;

        private IModelRepository _modelRepository;
        private ICharacterActorFactory _characterActorFactory;
        private IFieldActorFactory _fieldActorFactory;

        /// <summary>バトルモデル</summary>
        public IReadOnlyBattleModel BattleModel => BattleModelInternal;

        /// <summary>バトルモデル</summary>
        internal BattleModel BattleModelInternal { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleDomainService() {
            _scope = new DisposableScope();
        }

        /// <summary>
        /// サービスのDI
        /// </summary>
        [ServiceInject]
        private void Inject(IModelRepository modelRepository, ICharacterActorFactory characterActorFactory, IFieldActorFactory fieldActorFactory) {
            _modelRepository = modelRepository;
            _characterActorFactory = characterActorFactory;
            _fieldActorFactory = fieldActorFactory;
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
            // モデル生成
            BattleModelInternal = _modelRepository.CreateSingleModel<BattleModel>();

            // バトル初期化
            BattleModelInternal.Setup(master, new IState<BattleSequenceType>[] { new EnterState(this), new PlayingState(this), new FinishState(this) });

            // フィールドの生成
            await CreateFieldAsync(master.FieldMaster, ct);

            // プレイヤーの生成
            await CreatePlayerAsync(playerMaster, ct);

            // 入り演出に遷移
            BattleModelInternal.ChangeState(BattleSequenceType.Enter);
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
        /// 更新処理
        /// </summary>
        public void Update() {
            var deltaTime = BattleModelInternal.TimeModel.GetDeltaTime(BattleTimeType.System);
            BattleModelInternal.UpdateState(deltaTime);
        }

        /// <summary>
        /// LayeredTimeの取得
        /// </summary>
        public IReadOnlyLayeredTime GetLayeredTime(BattleTimeType type) {
            return BattleModelInternal.TimeModelInternal.GetLayeredTime(type);
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
            var port = await _characterActorFactory.CreatePlayerAsync(playerModel, GetLayeredTime(BattleTimeType.ViewPrimary), ct);
            playerModel.ActorModelInternal.Setup(port);

            // モデルの登録
            BattleModelInternal.SetPlayerModel(playerModel);
        }

        /// <summary>
        /// 現在のプレイヤーの削除
        /// </summary>
        private void DeleteCurrentPlayer() {
            var playerModel = BattleModelInternal.PlayerModelInternal;
            if (playerModel == null) {
                return;
            }

            // 登録解除
            BattleModelInternal.SetPlayerModel(null);

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
            var port = await _fieldActorFactory.CreateAsync(fieldModel, GetLayeredTime(BattleTimeType.ViewPrimary), ct);
            fieldModel.ActorModelInternal.Setup(port);

            // モデルの登録
            BattleModelInternal.SetFieldModel(fieldModel);
        }

        /// <summary>
        /// 現在のフィールドの削除
        /// </summary>
        private void DeleteCurrentField() {
            var fieldModel = BattleModelInternal.FieldModelInternal;
            if (fieldModel == null) {
                return;
            }

            // 登録解除
            BattleModelInternal.SetFieldModel(null);

            // アクターの削除
            _fieldActorFactory.Destroy(fieldModel);

            // モデルの削除
            _modelRepository.DeleteAutoIdModel(fieldModel);
        }
    }
}