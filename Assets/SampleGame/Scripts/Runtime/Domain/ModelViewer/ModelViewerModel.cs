using System;
using GameFramework.Core;
using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyModelViewerModel {
        /// <summary>Actor初期化用データ</summary>
        IModelViewerMaster Master { get; }
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
    public class ModelViewerModel : AutoIdModel<ModelViewerModel>, IReadOnlyModelViewerModel {
        private ReactiveProperty<string> _environmentAssetKey;

        private Subject<CreatedPreviewActorDto> _createdPreviewActorSubject;
        private Subject<DeletedPreviewActorDto> _deletedPreviewActorSubject;

        /// <summary>Actor初期化用データ</summary>
        public IModelViewerMaster Master { get; private set; }
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
        private ModelViewerModel(int id)
            : base(id) {
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
            Master = master;
        }

        /// <summary>
        /// アクターモデルの変更
        /// </summary>
        public void ChangeActorModel(IPreviewActorMaster master) {
            DeleteActorModel();

            if (master != null) {
                // 生成
                ActorModelInternal = PreviewActorModel.Create();
                ActorModelInternal.Setup(master);

                // 通知
                _createdPreviewActorSubject.OnNext(new CreatedPreviewActorDto {
                    ActorModel = ActorModel
                });
            }
        }

        /// <summary>
        /// アクターモデルの削除
        /// </summary>
        public void DeleteActorModel() {
            if (ActorModelInternal == null) {
                return;
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