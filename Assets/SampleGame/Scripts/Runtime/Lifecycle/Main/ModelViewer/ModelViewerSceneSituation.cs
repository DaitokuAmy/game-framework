using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.ActorSystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using R3;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using SampleGame.Infrastructure;
using SampleGame.Infrastructure.ModelViewer;
using SampleGame.Presentation.ModelViewer;
using ThirdPersonEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// モデルビューアシーン
    /// </summary>
    public class ModelViewerSceneSituation : SceneSituation {
        private ModelViewerConfigData _configData;
        private ModelViewerAppService _appService;
        private ModelViewerDomainService _domainService;

        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/Develop/model_viewer.unity";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);

            async UniTask LoadAsync(CancellationToken ct) {
                // Config読み込み
                var assetManager = ServiceResolver.Resolve<AssetManager>();
                await new ModelViewerConfigDataRequest()
                    .LoadAsync(assetManager, scope, cancellationToken: ct)
                    .ContinueWith(x => {
                        _configData = x;
                        ServiceContainer.RegisterInstance(x).RegisterTo(scope);
                    });
            }

            yield return LoadAsync(scope.Token).ToCoroutine();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            SetupInfrastructures(scope);
            SetupManagers(scope);
            SetupFactories(scope);
            SetupDomains(scope);
            SetupApplications(scope);

            // アプリケーション初期化
            yield return _appService.SetupAsync(scope.Token).ToCoroutine();

            // カメラ操作用Controllerの設定
            var cameraManager = ServiceResolver.Resolve<CameraManager>();
            cameraManager.SetCameraHandler("Default", new PreviewCameraHandler(_configData.camera));

            // Recorderのセットアップ
            var recorder = ServiceResolver.Resolve<ModelRecorder>();
            recorder.ActorSlot = ServiceResolver.Resolve<ActorEntityManager>().RootTransform;

            // プレゼンテーション初期化
            SetupPresentations(scope);

            // Debug用
            ServiceResolver.Inject(new ModelViewerDebugServiceResolver().RegisterTo(scope));
        }

        /// <summary>
        /// Infrastructure層の初期化
        /// </summary>
        private void SetupInfrastructures(IScope scope) {
            ServiceContainer.Register<IModelRepository, ModelRepository>().RegisterTo(scope);
            ServiceContainer.Register<IModelViewerTableRepository, ModelViewerTableRepository>().RegisterTo(scope);
            ServiceContainer.Register<ModelViewerAssetRepository>().RegisterTo(scope);
            ServiceContainer.Register<EnvironmentSceneRepository>().RegisterTo(scope);
        }

        /// <summary>
        /// Managerの初期化
        /// </summary>
        private void SetupManagers(IScope scope) {
            var actorManager = new ActorEntityManager();
            ServiceContainer.RegisterInstance(actorManager).RegisterTo(scope);

            var cameraManager = ServiceResolver.Resolve<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);
        }

        /// <summary>
        /// Domain層の初期化
        /// </summary>
        private void SetupDomains(IScope scope) {
            _domainService = new ModelViewerDomainService();
            ServiceContainer.RegisterInstance(_domainService).RegisterTo(scope);
        }

        /// <summary>
        /// Application層の初期化
        /// </summary>
        private void SetupApplications(IScope scope) {
            _appService = new ModelViewerAppService();
            ServiceContainer.RegisterInstance(_appService).RegisterTo(scope);
        }

        /// <summary>
        /// Factoryの初期化
        /// </summary>
        private void SetupFactories(IScope scope) {
            ServiceContainer.Register<IPreviewActorFactory, PreviewActorFactory>().RegisterTo(scope);
            ServiceContainer.Register<IEnvironmentActorFactory, EnvironmentActorFactory>().RegisterTo(scope);
        }

        /// <summary>
        /// Presentation層の初期化
        /// </summary>
        private void SetupPresentations(IScope scope) {
            T AddLogic<T>(T logic, bool activate, IScope scp)
                where T : Logic {
                logic.RegisterTask(TaskOrder.Logic);
                logic.RegisterTo(scp);

                ServiceResolver.Inject(logic);

                if (activate) {
                    logic.Activate();
                }

                return logic;
            }

            AddLogic(new ModelViewerPresenter(), true, scope);
        }
    }
}