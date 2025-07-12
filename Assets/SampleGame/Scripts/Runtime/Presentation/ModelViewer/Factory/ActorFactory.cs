using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
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
        private readonly ActorEntityManager _actorEntityManager;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorFactory() {
            _actorEntityManager = Services.Resolve<ActorEntityManager>();
        }
        
        /// <summary>
        /// アクターの生成
        /// </summary>
        async UniTask<IActorPort> IActorFactory.CreateAsync(IReadOnlyActorModel model, LayeredTime layeredTime, CancellationToken ct) {
            if (model == null) {
                return null;
            }

            var entity = await _actorEntityManager.CreatePreviewActorEntityAsync(model.Id, model.Master.Prefab, layeredTime, ct);
            var actor = entity.GetActor<PreviewActor>();
            var controller = new ActorAdapter(model, actor);
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