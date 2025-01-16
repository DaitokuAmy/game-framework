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
        /// <summary>ビューア用マスター</summary>
        IModelViewerMaster Master { get; }
        /// <summary>環境モデル</summary>
        IReadOnlyEnvironmentModel EnvironmentModel { get; }
        /// <summary>アクターモデル</summary>
        IReadOnlyActorModel ActorModel { get; }
        
        /// <summary>環境生成通知</summary>
        IObservable<CreatedEnvironmentDto> CreatedEnvironmentSubject { get; }
        /// <summary>環境削除通知</summary>
        IObservable<DeletedEnvironmentDto> DeletedEnvironmentSubject { get; }
        /// <summary>アクター生成通知</summary>
        IObservable<CreatedActorDto> CreatedActorSubject { get; }
        /// <summary>アクター削除通知</summary>
        IObservable<DeletedActorDto> DeletedActorSubject { get; }
    }

    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class ModelViewerModel : SingleModel<ModelViewerModel>, IReadOnlyModelViewerModel {
        private IActorFactory _actorFactory;
        private IEnvironmentFactory _environmentFactory;

        private Subject<CreatedEnvironmentDto> _createdEnvironmentSubject;
        private Subject<DeletedEnvironmentDto> _deletedEnvironmentSubject;
        private Subject<CreatedActorDto> _createdActorSubject;
        private Subject<DeletedActorDto> _deletedActorSubject;

        /// <summary>ビューア用マスター</summary>
        public IModelViewerMaster Master { get; private set; }
        /// <summary>現在の環境マスター</summary>
        public IEnvironmentMaster EnvironmentMaster { get; private set; }
        /// <summary>環境モデル</summary>
        public IReadOnlyEnvironmentModel EnvironmentModel => EnvironmentModelInternal;
        /// <summary>アクターモデル</summary>
        public IReadOnlyActorModel ActorModel => ActorModelInternal;

        /// <summary>環境生成通知</summary>
        public IObservable<CreatedEnvironmentDto> CreatedEnvironmentSubject => _createdEnvironmentSubject;
        /// <summary>環境削除通知</summary>
        public IObservable<DeletedEnvironmentDto> DeletedEnvironmentSubject => _deletedEnvironmentSubject;
        /// <summary>アクター生成通知</summary>
        public IObservable<CreatedActorDto> CreatedActorSubject => _createdActorSubject;
        /// <summary>アクター削除通知</summary>
        public IObservable<DeletedActorDto> DeletedActorSubject => _deletedActorSubject;

        /// <summary>環境モデル</summary>
        internal EnvironmentModel EnvironmentModelInternal { get; private set; }
        /// <summary>アクターモデル</summary>
        internal ActorModel ActorModelInternal { get; private set; }

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

            _createdEnvironmentSubject = new Subject<CreatedEnvironmentDto>().ScopeTo(scope);
            _deletedEnvironmentSubject = new Subject<DeletedEnvironmentDto>().ScopeTo(scope);
            _createdActorSubject = new Subject<CreatedActorDto>().ScopeTo(scope);
            _deletedActorSubject = new Subject<DeletedActorDto>().ScopeTo(scope);
        }

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IModelViewerMaster master) {
            Master = master;
        }

        /// <summary>
        /// ファクトリーの設定
        /// </summary>
        public void SetFactory(IActorFactory actorFactory, IEnvironmentFactory environmentFactory) {
            _actorFactory = actorFactory;
            _environmentFactory = environmentFactory;
        }

        /// <summary>
        /// アクターの変更
        /// </summary>
        public async UniTask ChangeActorAsync(IActorMaster master, CancellationToken ct) {
            DeleteActor();

            if (master == null || _actorFactory == null) {
                return;
            }

            // モデル生成
            ActorModelInternal = ModelViewer.ActorModel.Create();
            ActorModelInternal.Setup(master);
            
            // アクター生成
            var controller = await _actorFactory.CreateAsync(ActorModelInternal, ct);
            ActorModelInternal.SetController(controller);

            // 通知
            _createdActorSubject.OnNext(new CreatedActorDto {
                ActorModel = ActorModel
            });
        }

        /// <summary>
        /// アクターの削除
        /// </summary>
        public void DeleteActor() {
            if (ActorModelInternal == null) {
                return;
            }
            
            // アクター削除
            if (_actorFactory != null) {
                _actorFactory.Destroy(ActorModelInternal.Id);
            }

            // 通知
            _deletedActorSubject.OnNext(new DeletedActorDto {
                ActorModel = ActorModel
            });

            // 削除
            ModelViewer.ActorModel.Delete(ActorModelInternal.Id);
            ActorModelInternal = null;
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public async UniTask ChangeEnvironmentAsync(IEnvironmentMaster master, CancellationToken ct) {
            DeleteEnvironment();

            if (master == null || _environmentFactory == null) {
                return;
            }

            // モデル生成
            EnvironmentModelInternal = ModelViewer.EnvironmentModel.Create();
            EnvironmentModelInternal.Setup(master);
            
            // アクター生成
            var controller = await _environmentFactory.CreateAsync(EnvironmentModelInternal, ct);
            EnvironmentModelInternal.SetController(controller);

            // 通知
            _createdEnvironmentSubject.OnNext(new CreatedEnvironmentDto {
                EnvironmentModel = EnvironmentModel
            });
        }

        /// <summary>
        /// 環境の削除
        /// </summary>
        public void DeleteEnvironment() {
            if (EnvironmentModelInternal == null) {
                return;
            }
            
            // 環境削除
            if (_environmentFactory != null) {
                _environmentFactory.Destroy(EnvironmentModelInternal.Id);
            }

            // 通知
            _deletedEnvironmentSubject.OnNext(new DeletedEnvironmentDto {
                EnvironmentModel = EnvironmentModel
            });

            // 削除
            ModelViewer.EnvironmentModel.Delete(EnvironmentModelInternal.Id);
            EnvironmentModelInternal = null;
        }
    }
}