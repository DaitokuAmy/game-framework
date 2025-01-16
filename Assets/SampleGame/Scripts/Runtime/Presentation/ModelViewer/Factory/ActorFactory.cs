using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Application.ModelViewer;
using SampleGame.Presentation;
using SampleGame.Presentation.ModelViewer;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// プレビュー用のActor生成クラス
    /// </summary>
    public class ActorFactory : IActorFactory {
        private readonly ModelViewerAppService _appService;
        private readonly ActorEntityManager _actorEntityManager;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorFactory() {
            _appService = Services.Get<ModelViewerAppService>();
            _actorEntityManager = Services.Get<ActorEntityManager>();
        }
        
        /// <summary>
        /// アクターの生成
        /// </summary>
        async UniTask<IActorController> IActorFactory.CreateAsync(IReadOnlyActorModel model, CancellationToken ct) {
            if (model == null) {
                return null;
            }

            var domainService = _appService.DomainService;
            var entity = await _actorEntityManager.CreatePreviewActorEntityAsync(1, model.Master.Prefab, domainService.SettingsModel.LayeredTime, ct);
            var actor = entity.GetActor<PreviewActor>();
            var controller = new ActorController(model, actor);
            controller.RegisterTask(TaskOrder.Logic);
            entity.AddLogic(controller);
            return controller;
        }

        /// <summary>
        /// アクターの削除
        /// </summary>
        void IActorFactory.Destroy(int id) {
            _actorEntityManager.DestroyEntity(id);
        }
    }
}