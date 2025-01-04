using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Domain.ModelViewer;

namespace SampleGame.Application.ModelViewer {
    /// <summary>
    /// モデルビューア用のアプリケーション層サービス
    /// </summary>
    public class ModelViewerAppService : IDisposable {
        private readonly ModelViewerDomainService _domainService;
        private readonly IModelViewerRepository _repository;

        private DisposableScope _scope;

        public IReadOnlyModelViewerDomainService DomainService => _domainService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerAppService() {
            _domainService = Services.Get<ModelViewerDomainService>();
            _repository = Services.Get<IModelViewerRepository>();
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
        /// 初期化
        /// </summary>
        public async UniTask SetupAsync(IModelViewerMaster master, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            // 設定ファイルで初期化
            _domainService.Setup(master);
        }

        /// <summary>
        /// ファクトリーの設定
        /// </summary>
        public void SetFactory(IPreviewActorFactory actorFactory) {
            _domainService.SetFactory(actorFactory);
        }

        /// <summary>
        /// 表示モデルの変更
        /// </summary>
        public void ChangePreviewActor(int index) {
            var assetKeys = _domainService.ModelViewerModel.MasterData.ActorAssetKeys;
            if (index < 0 || index >= assetKeys.Count) {
                return;
            }

            // 設定ファイルを読み込み
            _repository.LoadActorMasterAsync(assetKeys[index], _scope.Token)
                .ContinueWith(result => { _domainService.ChangePreviewActor(result); })
                .Forget();
        }

        /// <summary>
        /// アクターのリセット
        /// </summary>
        public void ResetActor() {
            _domainService.ResetActor();
        }

        /// <summary>
        /// アニメーションクリップの変更
        /// </summary>
        public void ChangeAnimationClip(int clipIndex) {
            _domainService.ChangeAnimationClip(clipIndex);
        }

        /// <summary>
        /// アニメーションクリップのリプレイ
        /// </summary>
        public void ReplayAnimationClip() {
            var model = _domainService.PreviewActorModel;
            if (model == null) {
                return;
            }
            
            _domainService.ChangeAnimationClip(model.CurrentAnimationClipIndex);
        }

        /// <summary>
        /// アニメーションクリップを先に進める
        /// </summary>
        public void NextAnimationClip() {
            var model = _domainService.PreviewActorModel;
            if (model == null) {
                return;
            }

            var index = model.CurrentAnimationClipIndex;
            index = (index + 1) % model.AnimationClipCount;
            _domainService.ChangeAnimationClip(index);
        }

        /// <summary>
        /// アニメーションクリップを前に戻す
        /// </summary>
        public void PreviousAnimationClip() {
            var model = _domainService.PreviewActorModel;
            if (model == null) {
                return;
            }

            var index = model.CurrentAnimationClipIndex;
            index = (index - 1 + model.AnimationClipCount) % model.AnimationClipCount;
            _domainService.ChangeAnimationClip(index);
        }

        /// <summary>
        /// 加算用アニメーションクリップの変更
        /// ※同じClipを設定したらトグル
        /// </summary>
        public void ToggleAdditiveAnimationClip(int clipIndex) {
            _domainService.ToggleAdditiveAnimationClip(clipIndex);
        }

        /// <summary>
        /// MeshAvatarの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, int index) {
            _domainService.ChangeMeshAvatar(key, index);
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public void ChangeEnvironment(int index) {
            _domainService.ChangeEnvironment(index);
        }

        /// <summary>
        /// カメラ制御タイプの変更
        /// </summary>
        public void ChangeCameraControlType(CameraControlType type) {
            _domainService.ChangeCameraControlType(type);
        }

        /// <summary>
        /// タイムスケールの変更
        /// </summary>
        public void SetTimeScale(float timeScale) {
            _domainService.SetTimeScale(timeScale);
        }
    }
}