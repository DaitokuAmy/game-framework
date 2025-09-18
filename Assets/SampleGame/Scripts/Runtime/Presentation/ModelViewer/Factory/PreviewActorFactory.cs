using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Domain.ModelViewer;
using SampleGame.Infrastructure.ModelViewer;
using ThirdPersonEngine;
using ThirdPersonEngine.ModelViewer;
using UnityEngine;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// プレビュー用のActor生成クラス
    /// </summary>
    public class PreviewActorFactory : IPreviewActorFactory, IServiceUser {
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
        
        private ActorEntityManager _actorEntityManager;
        private ModelViewerAssetRepository _assetRepository;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActorFactory() {
        }

        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver resolver) {
            _actorEntityManager = resolver.Resolve<ActorEntityManager>();
            _assetRepository = resolver.Resolve<ModelViewerAssetRepository>();
        }
        
        /// <summary>
        /// アクターの生成
        /// </summary>
        async UniTask<IPreviewActorPort> IPreviewActorFactory.CreateAsync(IReadOnlyPreviewActorModel model, LayeredTime layeredTime, CancellationToken ct) {
            if (model == null) {
                return null;
            }
            
            // actorData読み込み
            var actorData = await _assetRepository.LoadPreviewActorDataAsync(model.Master.AssetKey, ct);

            // body生成
            var prefab = actorData.prefab;
            var body = new Body(Object.Instantiate(prefab, _actorEntityManager.RootTransform), new BodyBuilder());
            body.LayeredTime.SetParent(layeredTime);
            body.RegisterTask(TaskOrder.Body);
            
            // actor生成
            var actor = new PreviewActor(body, actorData);
            actor.RegisterTask(TaskOrder.Actor);
            
            // adapter生成
            var adapter = new PreviewActorAdapter(model, actor);
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