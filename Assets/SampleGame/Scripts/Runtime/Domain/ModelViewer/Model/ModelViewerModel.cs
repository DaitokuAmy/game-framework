using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyModelViewerModel {
        /// <summary>Actor初期化用データ</summary>
        IModelViewerMaster MasterData { get; }
        /// <summary>アクターモデル</summary>
        IReadOnlyPreviewActorModel ActorModel { get; }

        /// <summary>現在の環境アセットキー</summary>
        IReadOnlyReactiveProperty<string> EnvironmentAssetKey { get; }
        /// <summary>アクター生成通知</summary>
        IObservable<CreatedPreviewActorDto> CreatedPreviewActorSubject { get; }
        /// <summary>アクター削除通知</summary>
        IObservable<DeletedPreviewActorDto> DeletedPreviewActorSubject { get; }
    }

    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class ModelViewerModel : SingleModel<ModelViewerModel>, IReadOnlyModelViewerModel {
        private ReactiveProperty<string> _environmentAssetKey;
        private IPreviewActorFactory _actorFactory;

        private Subject<CreatedPreviewActorDto> _createdPreviewActorSubject;
        private Subject<DeletedPreviewActorDto> _deletedPreviewActorSubject;

        /// <summary>Actor初期化用データ</summary>
        public IModelViewerMaster MasterData { get; private set; }
        /// <summary>アクターモデル</summary>
        public IReadOnlyPreviewActorModel ActorModel => ActorModelInternal;

        /// <summary>現在の環境アセットキー</summary>
        public IReadOnlyReactiveProperty<string> EnvironmentAssetKey => _environmentAssetKey;
        /// <summary>アクター生成通知</summary>
        public IObservable<CreatedPreviewActorDto> CreatedPreviewActorSubject => _createdPreviewActorSubject;
        /// <summary>アクター削除通知</summary>
        public IObservable<DeletedPreviewActorDto> DeletedPreviewActorSubject => _deletedPreviewActorSubject;

        /// <summary>アクターモデル</summary>
        internal PreviewActorModel ActorModelInternal { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ModelViewerModel(object empty)
            : base(empty) {
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);

            _environmentAssetKey = new ReactiveProperty<string>().ScopeTo(scope);

            _createdPreviewActorSubject = new Subject<CreatedPreviewActorDto>().ScopeTo(scope);
            _deletedPreviewActorSubject = new Subject<DeletedPreviewActorDto>().ScopeTo(scope);
        }

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IModelViewerMaster master) {
            MasterData = master;
        }

        /// <summary>
        /// ファクトリーの設定
        /// </summary>
        public void SetFactory(IPreviewActorFactory actorFactory) {
            _actorFactory = actorFactory;
        }

        /// <summary>
        /// アクターモデルの変更
        /// </summary>
        public async UniTask ChangeActorModelAsync(IPreviewActorMaster master, CancellationToken ct) {
            DeleteActorModel();

            if (master == null || _actorFactory == null) {
                return;
            }

            // モデル生成
            ActorModelInternal = PreviewActorModel.Create();
            ActorModelInternal.Setup(master);
            
            // アクター生成
            var controller = await _actorFactory.CreateAsync(ActorModelInternal, ct);
            ActorModelInternal.SetController(controller);

            // 通知
            _createdPreviewActorSubject.OnNext(new CreatedPreviewActorDto {
                ActorModel = ActorModel
            });
        }

        /// <summary>
        /// アクターモデルの削除
        /// </summary>
        public void DeleteActorModel() {
            if (ActorModelInternal == null) {
                return;
            }
            
            // アクター削除
            if (_actorFactory != null) {
                _actorFactory.Destroy(ActorModelInternal.Id);
            }

            // 通知
            _deletedPreviewActorSubject.OnNext(new DeletedPreviewActorDto {
                ActorModel = ActorModel
            });

            // 削除
            PreviewActorModel.Delete(ActorModelInternal.Id);
            ActorModelInternal = null;
        }

        /// <summary>
        /// 環境アセットキーの変更
        /// </summary>
        public void ChangeEnvironmentAssetKey(string assetKey) {
            _environmentAssetKey.Value = assetKey;
        }
    }
}