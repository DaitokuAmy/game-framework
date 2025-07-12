using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Presentation.ModelViewer;
using ThirdPersonEngine;
using ThirdPersonEngine.ModelViewer;
using UnityEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// プレビュー用のActor生成クラス
    /// </summary>
    public class PreviewActorFactory : IPreviewActorFactory {
        /// <summary>
        /// Body生成用のBuilder
        /// </summary>
        private class BodyBuilder : IBodyBuilder {
            public void Build(Body body, GameObject gameObject) {
                if (gameObject.GetComponent<AvatarComponent>() == null) {
                    body.AddSerializedBodyComponent<AvatarComponent>();
                }
            }
        }
        
        private readonly ActorEntityManager _actorEntityManager;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActorFactory() {
            _actorEntityManager = Services.Resolve<ActorEntityManager>();
        }
        
        /// <summary>
        /// アクターの生成
        /// </summary>
        async UniTask<IActorPort> IPreviewActorFactory.CreateAsync(IReadOnlyPreviewActorModel model, LayeredTime layeredTime, CancellationToken ct) {
            if (model == null) {
                return null;
            }

            // body生成
            var prefab = model.Master.Prefab;
            var body = new Body(Object.Instantiate(prefab, _actorEntityManager.RootTransform), new BodyBuilder());
            body.RegisterTask(TaskOrder.Body);
            
            // actor生成
            var actor = new PreviewActor(body);
            actor.RegisterTask(TaskOrder.Actor);
            
            // adapter生成
            var adapter = new ActorAdapter(model, actor);
            adapter.RegisterTask(TaskOrder.Logic);

            // entity構築
            var entity = _actorEntityManager.CreateEntity(model.Id);
            entity.SetBody(body);
            entity.AddActor(actor);
            entity.AddLogic(adapter);
            
            return adapter;
        }

        /// <summary>
        /// アクターの削除
        /// </summary>
        void IPreviewActorFactory.Destroy(int id) {
            _actorEntityManager.DestroyEntity(id);
        }
    }
}