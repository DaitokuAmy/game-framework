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

            var entity = _actorEntityManager.CreateEntity(model.Id);
            // todo:body生成
            // todo:actor生成
            var actor = default(PreviewActor);
            actor.RegisterTask(TaskOrder.Actor);
            
            var adapter = new ActorAdapter(model, actor);
            adapter.RegisterTask(TaskOrder.Logic);

            entity.AddActor(actor);
            entity.AddLogic(adapter);
            return adapter;
        }

        /// <summary>
        /// アクターの削除
        /// </summary>
        void IActorFactory.Destroy(int id) {
            _actorEntityManager.DestroyEntity(id);
        }
    }
}