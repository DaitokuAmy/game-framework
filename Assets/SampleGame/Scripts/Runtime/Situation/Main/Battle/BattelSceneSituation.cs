using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.AssetSystems;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Infrastructure;
using SampleGame.Infrastructure.Battle;
using SampleGame.Presentation;
using SampleGame.Presentation.Battle;
using UniRx;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// Battle用のSceneSituation
    /// </summary>
    public class BattleSceneSituation : SceneSituation {
        /// <summary>
        /// Body生成用のBuilder
        /// </summary>
        private class BodyBuilder : IBodyBuilder {
            public void Build(IBody body, GameObject gameObject) {
                if (gameObject.GetComponent<AvatarController>() == null) {
                    body.AddController(gameObject.AddComponent<AvatarController>());
                }
            }
        }

        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/battle.unity";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);

            // UI読み込み
            var tasks = new List<UniTask>();
            tasks.Add(LoadUIAsync(scope, scope.Token));

            yield return UniTask.WhenAll(tasks).ToCoroutine();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            yield return SetupInfrastructureRoutine(scope);
            yield return SetupManagerRoutine(scope);
            yield return SetupDomainRoutine(scope);
            yield return SetupApplicationRoutine(scope);
            yield return SetupFactoryRoutine(scope);
            yield return SetupPresentationRoutine(scope);

            // todo:テスト
            yield return Services.Get<FieldManager>().ChangeFieldAsync("fld001", scope.Token).ToCoroutine();
            yield return Services.Get<ActorEntityManager>().CreateBattleCharacterActorEntityAsync(1, "ch001_00", "ch001_00", null, scope.Token).ToCoroutine();
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield return base.OpenRoutineInternal(handle, animationScope);

            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            yield return battleHudUIService.BattleHudUIScreen.OpenAsync();
        }

        /// <summary>
        /// 開いた後の処理
        /// </summary>
        protected override void PostOpenInternal(TransitionHandle handle, IScope scope) {
            base.PostOpenInternal(handle, scope);

            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            battleHudUIService.BattleHudUIScreen.OpenAsync(immediate: true);
        }

        /// <summary>
        /// 閉く処理
        /// </summary>
        protected override IEnumerator CloseRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield return base.CloseRoutineInternal(handle, animationScope);

            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            yield return battleHudUIService.BattleHudUIScreen.CloseAsync();
        }

        /// <summary>
        /// 閉じた後の処理
        /// </summary>
        protected override void PostCloseInternal(TransitionHandle handle) {
            base.PostCloseInternal(handle);

            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            battleHudUIService.BattleHudUIScreen.CloseAsync(immediate: true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var situationService = Services.Get<SituationService>();
            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();

            // メニューボタン
            battleHudUIService.BattleHudUIScreen.ClickedMenuButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { situationService.Transition<BattlePauseSituation>(); });
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            // todo:テスト
            var actorEntityManager = Services.Get<ActorEntityManager>();
            if (actorEntityManager != null) {
                if (actorEntityManager.Entities.TryGetValue(1, out var entity)) {
                    var actor = entity.GetActor<BattleCharacterActor>();
                    if (Input.GetKey(KeyCode.UpArrow)) {
                        actor.Move(Vector2.up);
                    }
                    else if (Input.GetKey(KeyCode.DownArrow)) {
                        actor.Move(Vector2.down);
                    }
                    else if (Input.GetKey(KeyCode.RightArrow)) {
                        actor.Move(Vector2.right);
                    }
                    else if (Input.GetKey(KeyCode.LeftArrow)) {
                        actor.Move(Vector2.left);
                    }
                    else {
                        actor.Move(Vector2.zero);
                    }
                }
            }
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            var uiManager = Services.Get<UIManager>();

            UniTask LoadAsync(string assetKey) {
                return uiManager.LoadSceneAsync(assetKey).ScopeTo(unloadScope).ToUniTask(cancellationToken: ct);
            }

            return UniTask.WhenAll(LoadAsync("ui_battle"));
        }

        /// <summary>
        /// Infrastructure初期化
        /// </summary>
        private IEnumerator SetupInfrastructureRoutine(IScope scope) {
            var assetManager = Services.Get<AssetManager>();
            var battleCharacterAssetRepository = new BattleCharacterAssetRepository(assetManager);
            ServiceContainer.Set(battleCharacterAssetRepository).ScopeTo(scope);
            var bodyPrefabRepository = new BodyPrefabRepository(assetManager);
            ServiceContainer.Set(bodyPrefabRepository).ScopeTo(scope);
            var environmentSceneRepository = new EnvironmentSceneRepository(assetManager);
            ServiceContainer.Set(environmentSceneRepository).ScopeTo(scope);
            yield break;
        }

        /// <summary>
        /// Manager初期化
        /// </summary>
        private IEnumerator SetupManagerRoutine(IScope scope) {
            var bodyBuilder = new BodyBuilder();
            var bodyManager = new BodyManager(bodyBuilder);
            bodyManager.RegisterTask(TaskOrder.Body);
            ServiceContainer.Set(bodyManager).ScopeTo(scope);

            var fieldManager = new FieldManager();
            ServiceContainer.Set(fieldManager).ScopeTo(scope);

            var actorEntityManager = new ActorEntityManager(bodyManager);
            ServiceContainer.Set(actorEntityManager).ScopeTo(scope);

            var cameraManager = Services.Get<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);
            yield break;
        }

        /// <summary>
        /// Domain初期化
        /// </summary>
        private IEnumerator SetupDomainRoutine(IScope scope) {
            yield return null;
        }

        /// <summary>
        /// Application初期化
        /// </summary>
        private IEnumerator SetupApplicationRoutine(IScope scope) {
            yield return null;
        }

        /// <summary>
        /// Factory初期化
        /// </summary>
        private IEnumerator SetupFactoryRoutine(IScope scope) {
            yield return null;
        }

        /// <summary>
        /// Presentation初期化
        /// </summary>
        private IEnumerator SetupPresentationRoutine(IScope scope) {
            yield return null;
        }
    }
}