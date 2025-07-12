using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Infrastructure;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyModelViewerDomainService {
        /// <summary>モデルビューア全体管理用モデル</summary>
        IReadOnlyModelViewerModel ModelViewerModel { get; }
        /// <summary>表示用オブジェクトのモデル</summary>
        IReadOnlyPreviewActorModel PreviewActorModel { get; }
        /// <summary>録画用モデル</summary>
        IReadOnlyRecordingModel RecordingModel { get; }
        /// <summary>設定用モデル</summary>
        IReadOnlySettingsModel SettingsModel { get; }
    }

    /// <summary>
    /// モデルビューア用のドメインモデル
    /// </summary>
    public class ModelViewerDomainService : IDisposable, IReadOnlyModelViewerDomainService {
        private readonly IModelRepository _modelRepository;
        private readonly IEnvironmentActorFactory _environmentActorFactory;
        private readonly IPreviewActorFactory _previewActorFactory;
        
        private DisposableScope _scope;

        /// <summary>モデルビューア全体管理用モデル</summary>
        public IReadOnlyModelViewerModel ModelViewerModel => ModelViewerModelInternal;
        /// <summary>表示用オブジェクトのモデル</summary>
        public IReadOnlyPreviewActorModel PreviewActorModel => PreviewInternal;
        /// <summary>録画用モデル</summary>
        public IReadOnlyRecordingModel RecordingModel => RecordingModelInternal;
        /// <summary>設定用モデル</summary>
        public IReadOnlySettingsModel SettingsModel => SettingsModelInternal;
        
        /// <summary>モデルビューア全体管理用モデル</summary>
        internal ModelViewerModel ModelViewerModelInternal { get; private set; }
        /// <summary>表示用オブジェクトのモデル</summary>
        internal PreviewActorModel PreviewInternal => ModelViewerModelInternal.PreviewActorModelInternal;
        /// <summary>録画用モデル</summary>
        internal RecordingModel RecordingModelInternal { get; private set; }
        /// <summary>設定用モデル</summary>
        internal SettingsModel SettingsModelInternal { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerDomainService() {
            _modelRepository = Services.Resolve<IModelRepository>();
            _environmentActorFactory = Services.Resolve<IEnvironmentActorFactory>();
            _previewActorFactory = Services.Resolve<IPreviewActorFactory>();
            
            _scope = new DisposableScope();

            ModelViewerModelInternal = _modelRepository.GetSingleModel<ModelViewerModel>();
            RecordingModelInternal = _modelRepository.GetSingleModel<RecordingModel>();
            SettingsModelInternal = _modelRepository.GetSingleModel<SettingsModel>();
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
        public void Setup(IModelViewerMaster master) {
            ModelViewerModelInternal.Setup(master);
        }

        /// <summary>
        /// プレビューアクターの変更
        /// </summary>
        public async UniTask ChangePreviewActorAsync(IActorMaster master, CancellationToken ct) {
            // 既存モデルの削除
            var model = ModelViewerModelInternal.PreviewActorModelInternal;
            if (model != null) {
                _previewActorFactory.Destroy(model.Id);
                ModelViewerModelInternal.ChangePreviewActor(null);
                _modelRepository.DeleteAutoIdModel(model);
            }

            if (master == null) {
                return;
            }
            
            // モデルの生成
            model = _modelRepository.CreateAutoIdModel<PreviewActorModel>();
            model.Setup(master);
            
            // 初期化
            var port = await _previewActorFactory.CreateAsync(model, SettingsModel.LayeredTime, ct);
            model.SetPort(port);
            
            // 反映
            ModelViewerModelInternal.ChangePreviewActor(model);
        }

        /// <summary>
        /// アクターのリセット
        /// </summary>
        public void ResetActor() {
            if (PreviewActorModel == null) {
                return;
            }
            
            PreviewInternal.ResetActor();
        }

        /// <summary>
        /// アニメーションクリップの変更
        /// </summary>
        public void ChangeAnimationClip(int clipIndex) {
            if (PreviewActorModel == null) {
                return;
            }
            
            PreviewInternal.ChangeAnimationClip(clipIndex, SettingsModel.ResetOnPlay);
        }

        /// <summary>
        /// 加算用アニメーションクリップの変更
        /// ※同じClipを設定したらトグル
        /// </summary>
        public void ToggleAdditiveAnimationClip(int clipIndex) {
            if (PreviewActorModel == null) {
                return;
            }
            
            PreviewInternal.ToggleAdditiveAnimationClip(clipIndex);
        }

        /// <summary>
        /// MeshAvatarの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, int index) {
            if (PreviewActorModel == null) {
                return;
            }
            
            PreviewInternal.ChangeMeshAvatar(key, index);
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public async UniTask ChangeEnvironmentAsync(IEnvironmentMaster master, CancellationToken ct) {
            // 既存モデルの削除
            var model = ModelViewerModelInternal.EnvironmentActorModelInternal;
            if (model != null) {
                _environmentActorFactory.Destroy(model);
                ModelViewerModelInternal.ChangeEnvironmentActor(null);
                _modelRepository.DeleteAutoIdModel(model);
            }

            if (master == null) {
                return;
            }
            
            // モデルの生成
            model = _modelRepository.CreateAutoIdModel<EnvironmentActorModel>(1000);
            model.Setup(master);
            
            // 初期化
            var port = await _environmentActorFactory.CreateAsync(model, ct);
            model.SetPort(port);
            
            // 反映
            ModelViewerModelInternal.ChangeEnvironmentActor(model);
        }

        /// <summary>
        /// ディレクショナルライトのY角度の設定
        /// </summary>
        public void SetDirectionalLightAngleY(float angleY) {
            ModelViewerModelInternal.EnvironmentActorModelInternal.SetLightAngleY(angleY);
        }

        /// <summary>
        /// カメラ制御タイプの変更
        /// </summary>
        public void ChangeCameraControlType(CameraControlType type) {
            SettingsModelInternal.ChangeCameraControlType(type);
        }

        /// <summary>
        /// タイムスケールの変更
        /// </summary>
        public void SetTimeScale(float timeScale) {
            SettingsModelInternal.SetTimeScale(timeScale);
        }

        /// <summary>
        /// ResetOnPlayの設定
        /// </summary>
        public void SetResetOnPlay(bool reset) {
            SettingsModelInternal.SetResetOnPlay(reset);
        }
        
        /// <summary>
        /// 録画オプションの変更
        /// </summary>
        public void SetRecordingOptions(RecordingOptions recordingOptions) {
            RecordingModelInternal.SetOptions(recordingOptions);
        }

        /// <summary>
        /// 回転時間の設定
        /// </summary>
        public void SetRecordingRotationDuration(float duration) {
            RecordingModelInternal.SetRotationDuration(duration);
        }
    }
}