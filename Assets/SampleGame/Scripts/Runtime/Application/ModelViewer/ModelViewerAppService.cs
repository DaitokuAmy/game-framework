using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Domain.ModelViewer;
using ThirdPersonEngine;

namespace SampleGame.Application.ModelViewer {
    /// <summary>
    /// モデルビューア用のアプリケーション層サービス
    /// </summary>
    public class ModelViewerAppService : IDisposable, IServiceUser {
        /// <summary>
        /// マスター
        /// </summary>
        private class ModelViewerMaster : IModelViewerMaster {
            /// <inheritdoc/>
            public int DefaultActorId { get; set; }
            /// <inheritdoc/>
            public int DefaultEnvironmentId { get; set; }
            /// <inheritdoc/>
            public IReadOnlyList<int> ActorIds { get; set; }
            /// <inheritdoc/>
            public IReadOnlyList<int> EnvironmentIds { get; set; }
        }

        private DisposableScope _scope;

        private ModelViewerDomainService _domainService;
        private IModelViewerTableRepository _tableRepository;

        public IReadOnlyModelViewerDomainService DomainService => _domainService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerAppService() {
            _scope = new DisposableScope();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _scope?.Dispose();
            _scope = null;
        }

        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver resolver) {
            _domainService = resolver.Resolve<ModelViewerDomainService>();
            _tableRepository = resolver.Resolve<IModelViewerTableRepository>();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public async UniTask SetupAsync(CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            // マスター読み込み
            await _tableRepository.LoadTablesAsync(ct);

            // マスター情報構築
            var master = CreateMaster(1, 1);

            // 基本機能の初期化
            _domainService.Setup(master);
        }

        /// <summary>
        /// 表示モデルの変更
        /// </summary>
        public void ChangeActor(int index) {
            var actorIds = _domainService.ModelViewerModel.Master.ActorIds;
            if (index < 0 || index >= actorIds.Count) {
                return;
            }

            // マスターを読み込んで、初期化
            var master = _tableRepository.FindModelViewerActorById(actorIds[index]);
            _domainService.ChangePreviewActorAsync(master, _scope.Token).Forget();
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
            var environmentIds = _domainService.ModelViewerModel.Master.EnvironmentIds;
            if (index < 0 || index >= environmentIds.Count) {
                return;
            }

            // マスターを取得して初期化
            var master = _tableRepository.FindModelViewerEnvironmentById(environmentIds[index]);
            _domainService.ChangeEnvironmentAsync(master, _scope.Token).Forget();
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

        /// <summary>
        /// ResetOnPlayの設定
        /// </summary>
        public void SetResetOnPlay(bool reset) {
            _domainService.SetResetOnPlay(reset);
        }

        /// <summary>
        /// 録画オプションの変更
        /// </summary>
        public void SetRecordingOptions(ModelRecorder.Options recordingOptions) {
            // todo:
            //_domainService.SetRecordingOptions(recordingOptions);
        }

        /// <summary>
        /// 回転時間の設定
        /// </summary>
        public void SetRecordingRotationDuration(float duration) {
            _domainService.SetRecordingRotationDuration(duration);
        }

        /// <summary>
        /// マスターの構築
        /// </summary>
        private IModelViewerMaster CreateMaster(int defaultActorId, int defaultEnvironmentId) {
            var master = new ModelViewerMaster();
            master.DefaultActorId = 1;
            master.DefaultEnvironmentId = 1;
            master.ActorIds = _tableRepository.GetModelViewerActorIds();
            master.EnvironmentIds = _tableRepository.GetModelViewerEnvironmentIds();
            return master;
        }
    }
}