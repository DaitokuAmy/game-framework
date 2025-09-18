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
        IReadOnlyEnvironmentModel EnvironmentActorModel { get; }
        /// <summary>アクターモデル</summary>
        IReadOnlyPreviewActorModel PreviewActorModel { get; }
        
        /// <summary>環境生成通知</summary>
        Observable<ChangedEnvironmentActorDto> ChangedEnvironmentActorSubject { get; }
        /// <summary>環境削除通知</summary>
        Observable<DeletedEnvironmentActorDto> DeletedEnvironmentActorSubject { get; }
        /// <summary>アクター生成通知</summary>
        Observable<ChangedPreviewActorDto> ChangedPreviewActorSubject { get; }
        /// <summary>アクター削除通知</summary>
        Observable<DeletedPreviewActorDto> DeletedPreviewActorSubject { get; }
    }

    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class ModelViewerModel : SingleModel<ModelViewerModel>, IReadOnlyModelViewerModel {
        private IPreviewActorFactory _previewActorFactory;
        private IEnvironmentActorFactory _environmentActorFactory;

        private Subject<ChangedEnvironmentActorDto> _changedEnvironmentActorSubject;
        private Subject<DeletedEnvironmentActorDto> _deletedEnvironmentActorSubject;
        private Subject<ChangedPreviewActorDto> _changedPreviewActorSubject;
        private Subject<DeletedPreviewActorDto> _deletedPreviewActorSubject;

        /// <summary>ビューア用マスター</summary>
        public IModelViewerMaster Master { get; private set; }
        /// <summary>環境モデル</summary>
        public IReadOnlyEnvironmentModel EnvironmentActorModel => EnvironmentActorModelInternal;
        /// <summary>プレビューアクターモデル</summary>
        public IReadOnlyPreviewActorModel PreviewActorModel => PreviewActorModelInternal;

        /// <summary>環境生成通知</summary>
        public Observable<ChangedEnvironmentActorDto> ChangedEnvironmentActorSubject => _changedEnvironmentActorSubject;
        /// <summary>環境削除通知</summary>
        public Observable<DeletedEnvironmentActorDto> DeletedEnvironmentActorSubject => _deletedEnvironmentActorSubject;
        /// <summary>プレビューアクター生成通知</summary>
        public Observable<ChangedPreviewActorDto> ChangedPreviewActorSubject => _changedPreviewActorSubject;
        /// <summary>プレビューアクター削除通知</summary>
        public Observable<DeletedPreviewActorDto> DeletedPreviewActorSubject => _deletedPreviewActorSubject;

        /// <summary>環境アクターモデル</summary>
        internal EnvironmentActorModel EnvironmentActorModelInternal { get; private set; }
        /// <summary>モデルアクターモデル</summary>
        internal PreviewActorModel PreviewActorModelInternal { get; private set; }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);

            _changedEnvironmentActorSubject = new Subject<ChangedEnvironmentActorDto>().RegisterTo(scope);
            _deletedEnvironmentActorSubject = new Subject<DeletedEnvironmentActorDto>().RegisterTo(scope);
            _changedPreviewActorSubject = new Subject<ChangedPreviewActorDto>().RegisterTo(scope);
            _deletedPreviewActorSubject = new Subject<DeletedPreviewActorDto>().RegisterTo(scope);
        }

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IModelViewerMaster master) {
            Master = master;
        }

        /// <summary>
        /// モデルアクターの変更
        /// </summary>
        public void ChangePreviewActor(PreviewActorModel previewActorModelPreview) {
            PreviewActorModelInternal = previewActorModelPreview;

            // 通知
            _changedPreviewActorSubject.OnNext(new ChangedPreviewActorDto {
                Model = PreviewActorModel
            });
        }
        
        /// <summary>
        /// 環境アクターの変更
        /// </summary>
        public void ChangeEnvironmentActor(EnvironmentActorModel model) {
            EnvironmentActorModelInternal = model;

            // 通知
            _changedEnvironmentActorSubject.OnNext(new ChangedEnvironmentActorDto {
                Model = EnvironmentActorModel
            });
        }
    }
}