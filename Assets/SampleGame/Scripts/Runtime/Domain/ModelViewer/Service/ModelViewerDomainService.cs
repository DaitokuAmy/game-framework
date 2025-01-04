using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;

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
        private DisposableScope _scope;

        /// <summary>モデルビューア全体管理用モデル</summary>
        public IReadOnlyModelViewerModel ModelViewerModel => ModelViewerModelInternal;
        /// <summary>表示用オブジェクトのモデル</summary>
        public IReadOnlyPreviewActorModel PreviewActorModel => PreviewActorModelInternal;
        /// <summary>録画用モデル</summary>
        public IReadOnlyRecordingModel RecordingModel => RecordingModelInternal;
        /// <summary>設定用モデル</summary>
        public IReadOnlySettingsModel SettingsModel => SettingsModelInternal;
        
        /// <summary>モデルビューア全体管理用モデル</summary>
        internal ModelViewerModel ModelViewerModelInternal { get; private set; }
        /// <summary>表示用オブジェクトのモデル</summary>
        internal PreviewActorModel PreviewActorModelInternal => ModelViewerModelInternal.ActorModelInternal;
        /// <summary>録画用モデル</summary>
        internal RecordingModel RecordingModelInternal { get; private set; }
        /// <summary>設定用モデル</summary>
        internal SettingsModel SettingsModelInternal { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerDomainService() {
            _scope = new DisposableScope();

            ModelViewerModelInternal = ModelViewer.ModelViewerModel.Get();
            RecordingModelInternal = ModelViewer.RecordingModel.Get();
            SettingsModelInternal = ModelViewer.SettingsModel.Get();
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
        /// ファクトリーの設定
        /// </summary>
        public void SetFactory(IPreviewActorFactory actorFactory) {
            ModelViewerModelInternal.SetFactory(actorFactory);
        }

        /// <summary>
        /// 表示モデルの変更
        /// </summary>
        public void ChangePreviewActor(IPreviewActorMaster master) {
            ModelViewerModelInternal.ChangeActorModelAsync(master, _scope.Token).Forget();
        }

        /// <summary>
        /// アクターのリセット
        /// </summary>
        public void ResetActor() {
            if (PreviewActorModel == null) {
                return;
            }
            
            PreviewActorModelInternal.ResetActor();
        }

        /// <summary>
        /// アニメーションクリップの変更
        /// </summary>
        public void ChangeAnimationClip(int clipIndex) {
            if (PreviewActorModel == null) {
                return;
            }
            
            PreviewActorModelInternal.ChangeAnimationClip(clipIndex);
        }

        /// <summary>
        /// 加算用アニメーションクリップの変更
        /// ※同じClipを設定したらトグル
        /// </summary>
        public void ToggleAdditiveAnimationClip(int clipIndex) {
            if (PreviewActorModel == null) {
                return;
            }
            
            PreviewActorModelInternal.ToggleAdditiveAnimationClip(clipIndex);
        }

        /// <summary>
        /// MeshAvatarの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, int index) {
            if (PreviewActorModel == null) {
                return;
            }
            
            PreviewActorModelInternal.ChangeMeshAvatar(key, index);
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public void ChangeEnvironment(int index) {
            if (index < 0 || index >= ModelViewerModel.MasterData.EnvironmentAssetKeys.Count) {
                return;
            }

            var assetKey = ModelViewerModel.MasterData.EnvironmentAssetKeys[index];
            ModelViewerModelInternal.ChangeEnvironmentAssetKey(assetKey);
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
    }
}