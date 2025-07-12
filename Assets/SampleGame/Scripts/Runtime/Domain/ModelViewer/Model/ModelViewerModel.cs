using GameFramework;
using GameFramework.Core;
using R3;

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
        Observable<ChangedEnvironmentDto> ChangedEnvironmentSubject { get; }
        /// <summary>環境削除通知</summary>
        Observable<DeletedEnvironmentDto> DeletedEnvironmentSubject { get; }
        /// <summary>アクター生成通知</summary>
        Observable<ChangedActorDto> ChangedActorSubject { get; }
        /// <summary>アクター削除通知</summary>
        Observable<DeletedActorDto> DeletedActorSubject { get; }
    }

    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class ModelViewerModel : GameFramework.Core.SingleModel<ModelViewerModel>, IReadOnlyModelViewerModel {
        private IActorFactory _actorFactory;
        private IEnvironmentFactory _environmentFactory;

        private Subject<ChangedEnvironmentDto> _changedEnvironmentSubject;
        private Subject<DeletedEnvironmentDto> _deletedEnvironmentSubject;
        private Subject<ChangedActorDto> _changedActorSubject;
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
        public Observable<ChangedEnvironmentDto> ChangedEnvironmentSubject => _changedEnvironmentSubject;
        /// <summary>環境削除通知</summary>
        public Observable<DeletedEnvironmentDto> DeletedEnvironmentSubject => _deletedEnvironmentSubject;
        /// <summary>アクター生成通知</summary>
        public Observable<ChangedActorDto> ChangedActorSubject => _changedActorSubject;
        /// <summary>アクター削除通知</summary>
        public Observable<DeletedActorDto> DeletedActorSubject => _deletedActorSubject;

        /// <summary>環境モデル</summary>
        internal EnvironmentModel EnvironmentModelInternal { get; private set; }
        /// <summary>アクターモデル</summary>
        internal ActorModel ActorModelInternal { get; private set; }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);

            _changedEnvironmentSubject = new Subject<ChangedEnvironmentDto>().RegisterTo(scope);
            _deletedEnvironmentSubject = new Subject<DeletedEnvironmentDto>().RegisterTo(scope);
            _changedActorSubject = new Subject<ChangedActorDto>().RegisterTo(scope);
            _deletedActorSubject = new Subject<DeletedActorDto>().RegisterTo(scope);
        }

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IModelViewerMaster master) {
            Master = master;
        }

        /// <summary>
        /// アクターの変更
        /// </summary>
        public void ChangeActor(ActorModel model) {
            ActorModelInternal = model;

            // 通知
            _changedActorSubject.OnNext(new ChangedActorDto {
                ActorModel = ActorModel
            });
        }
        
        /// <summary>
        /// 環境の変更
        /// </summary>
        public void ChangeEnvironment(EnvironmentModel environmentModel) {
            EnvironmentModelInternal = environmentModel;

            // 通知
            _changedEnvironmentSubject.OnNext(new ChangedEnvironmentDto {
                EnvironmentModel = EnvironmentModel
            });
        }
    }
}