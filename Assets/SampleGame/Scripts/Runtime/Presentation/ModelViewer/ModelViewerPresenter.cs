using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.LogicSystems;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using UniRx;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// ModelViewer全体のPresenter
    /// </summary>
    public class ModelViewerPresenter : Logic {
        private ModelViewerAppService _appService;
        private ActorEntityManager _actorEntityManager;
        private EnvironmentManager _environmentManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerPresenter() {
            _appService = Services.Get<ModelViewerAppService>();
            _actorEntityManager = Services.Get<ActorEntityManager>();
            _environmentManager = Services.Get<EnvironmentManager>();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            var ct = scope.Token;

            var cameraManager = Services.Get<CameraManager>();
            var domainService = _appService.DomainService;
            var modelViewerModel = domainService.ModelViewerModel;
            var settingsModel = domainService.SettingsModel;

            // Actorの切り替え
            modelViewerModel.CreatedPreviewActorSubject
                .TakeUntil(scope)
                .StartWith(() => new CreatedPreviewActorDto {
                    ActorModel = modelViewerModel.ActorModel
                })
                .Subscribe(dto => {
                    if (dto.ActorModel == null) {
                        return;
                    }

                    var master = dto.ActorModel.Master;
                    var entity = _actorEntityManager.CreatePreviewActorEntity(1, master, _appService.DomainService.SettingsModel.LayeredTime);
                    var actor = entity.GetActor<PreviewActor>();
                    var controller = new PreviewActorController(domainService.PreviewActorModel, actor);
                    controller.RegisterTask(TaskOrder.Logic);
                    entity.AddLogic(controller);
                });

            modelViewerModel.DeletedPreviewActorSubject
                .TakeUntil(scope)
                .Subscribe(dto => {
                    if (dto.ActorModel == null) {
                        return;
                    }

                    _actorEntityManager.DestroyEntity(1);
                });

            // Environmentの切り替え
            modelViewerModel.EnvironmentAssetKey
                .TakeUntil(scope)
                .Subscribe(assetKey => { _environmentManager.ChangeEnvironmentAsync(assetKey, ct).Forget(); });

            // カメラの切り替え
            settingsModel.CameraControlType
                .TakeUntil(scope)
                .Subscribe(type => {
                    switch (type) {
                        case CameraControlType.Default:
                            cameraManager.ForceDeactivate("SceneView");
                            cameraManager.ForceActivate("Default");
                            break;
                        case CameraControlType.SceneView:
                            cameraManager.ForceActivate("SceneView");
                            cameraManager.ForceDeactivate("Default");
                            break;
                    }
                });
        }
    }
}