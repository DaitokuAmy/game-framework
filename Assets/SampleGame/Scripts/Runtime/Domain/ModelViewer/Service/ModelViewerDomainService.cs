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
        IReadOnlyActorModel ActorModel { get; }
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
        public IReadOnlyActorModel ActorModel => ActorModelInternal;
        /// <summary>録画用モデル</summary>
        public IReadOnlyRecordingModel RecordingModel => RecordingModelInternal;
        /// <summary>設定用モデル</summary>
        public IReadOnlySettingsModel SettingsModel => SettingsModelInternal;
        
        /// <summary>モデルビューア全体管理用モデル</summary>
        internal ModelViewerModel ModelViewerModelInternal { get; private set; }
        /// <summary>表示用オブジェクトのモデル</summary>
        internal ActorModel ActorModelInternal => ModelViewerModelInternal.ActorModelInternal;
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
        public void SetFactory(IActorFactory actorFactory, IEnvironmentFactory environmentFactory) {
            ModelViewerModelInternal.SetFactory(actorFactory, environmentFactory);
        }

        /// <summary>
        /// アクターの変更
        /// </summary>
        public void ChangeActor(IActorMaster master) {
            ModelViewerModelInternal.ChangeActorAsync(master, _scope.Token).Forget();
        }

        /// <summary>
        /// アクターのリセット
        /// </summary>
        public void ResetActor() {
            if (ActorModel == null) {
                return;
            }
            
            ActorModelInternal.ResetActor();
        }

        /// <summary>
        /// アニメーションクリップの変更
        /// </summary>
        public void ChangeAnimationClip(int clipIndex) {
            if (ActorModel == null) {
                return;
            }
            
            ActorModelInternal.ChangeAnimationClip(clipIndex, SettingsModel.ResetOnPlay);
        }

        /// <summary>
        /// 加算用アニメーションクリップの変更
        /// ※同じClipを設定したらトグル
        /// </summary>
        public void ToggleAdditiveAnimationClip(int clipIndex) {
            if (ActorModel == null) {
                return;
            }
            
            ActorModelInternal.ToggleAdditiveAnimationClip(clipIndex);
        }

        /// <summary>
        /// MeshAvatarの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, int index) {
            if (ActorModel == null) {
                return;
            }
            
            ActorModelInternal.ChangeMeshAvatar(key, index);
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public void ChangeEnvironment(IEnvironmentMaster master) {
            ModelViewerModelInternal.ChangeEnvironmentAsync(master, _scope.Token).Forget();
        }

        /// <summary>
        /// ディレクショナルライトのY角度の設定
        /// </summary>
        public void SetDirectionalLightAngleY(float angleY) {
            ModelViewerModelInternal.EnvironmentModelInternal.SetLightAngleY(angleY);
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