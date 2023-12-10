using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.ActorSystems;
using SampleGame.Domain.Battle;
using SampleGame.Presentation.Battle;

namespace SampleGame {
    /// <summary>
    /// Actor用のUtility
    /// </summary>
    public static class ActorExtensions {
        /// <summary>
        /// 基本的なEntityの初期化処理
        /// </summary>
        /// <param name="source">初期化対象のEntity</param>
        /// <param name="onCreateBody">Body生成</param>
        /// <param name="onSetupEntity">各種初期化処理</param>
        public static async UniTask<ActorEntity> SetupAsync(this ActorEntity source, Func<UniTask<Body>> onCreateBody, Action<ActorEntity> onSetupEntity) {
            // Bodyの生成
            if (onCreateBody != null) {
                var body = await onCreateBody.Invoke();
                source.SetBody(body);
            }

            // Body生成後の初期化
            if (onSetupEntity != null) {
                onSetupEntity.Invoke(source);
            }

            return source;
        }

        /// <summary>
        /// プレイヤーエンティティの初期化処理
        /// </summary>
        public static async UniTask<ActorEntity> SetupPlayerAsync(this ActorEntity source, IReadOnlyBattlePlayerModel model, LayeredTime parentLayeredTime, IScope unloadScope, CancellationToken ct) {
            return await source.SetupAsync(async () => {
                var prefab = await new PlayerPrefabAssetRequest(model.PrefabAssetKey)
                    .LoadAsync(unloadScope, ct);
                return Services.Get<BodyManager>().CreateFromPrefab(prefab);
            }, entity => {
                var body = entity.GetBody();
                body.LayeredTime.SetParent(parentLayeredTime);
                var actor = new BattleCharacterActor(body, model.ActorModel.SetupData);
                actor.RegisterTask(TaskOrder.Actor);
                var presenter = new BattlePlayerPresenter(model, actor);
                presenter.RegisterTask(TaskOrder.Logic);
                entity.AddActor(actor)
                    .AddLogic(presenter);
            });
        }
    }
}