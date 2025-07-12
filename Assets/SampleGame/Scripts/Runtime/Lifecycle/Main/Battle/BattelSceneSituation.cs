using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.ActorSystems;
using GameFramework.AssetSystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Infrastructure;
using SampleGame.Infrastructure.Battle;
using SampleGame.Presentation;
using SampleGame.Presentation.Battle;
using R3;
using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle用のSceneSituation
    /// </summary>
    public class BattleSceneSituation : SceneSituation {
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
            // yield return Services.Resolve<FieldManager>().ChangeFieldAsync("fld001", scope.Token).ToCoroutine();
            // yield return Services.Resolve<ActorEntityManager>().CreateBattleCharacterActorEntityAsync(1, "ch001_00", "ch001_00", null, scope.Token).ToCoroutine();
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield return base.OpenRoutineInternal(handle, animationScope);

            var uiManager = Services.Resolve<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            yield return battleHudUIService.BattleHudUIScreen.OpenAsync();
        }

        /// <summary>
        /// 開いた後の処理
        /// </summary>
        protected override void PostOpenInternal(TransitionHandle handle, IScope scope) {
            base.PostOpenInternal(handle, scope);

            var uiManager = Services.Resolve<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            battleHudUIService.BattleHudUIScreen.OpenAsync(immediate: true);
        }

        /// <summary>
        /// 閉く処理
        /// </summary>
        protected override IEnumerator CloseRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield return base.CloseRoutineInternal(handle, animationScope);

            var uiManager = Services.Resolve<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            yield return battleHudUIService.BattleHudUIScreen.CloseAsync();
        }

        /// <summary>
        /// 閉じた後の処理
        /// </summary>
        protected override void PostCloseInternal(TransitionHandle handle) {
            base.PostCloseInternal(handle);

            var uiManager = Services.Resolve<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            battleHudUIService.BattleHudUIScreen.CloseAsync(immediate: true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var situationService = Services.Resolve<SituationService>();
            var uiManager = Services.Resolve<UIManager>();
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
            var actorEntityManager = Services.Resolve<ActorEntityManager>();
            if (actorEntityManager != null) {
                if (actorEntityManager.Entities.TryGetValue(1, out var entity)) {
                    var actor = entity.GetActor<BattleCharacterActor>();
                    var direction = Vector2.zero;
                    if (Input.GetKey(KeyCode.UpArrow)) {
                        direction += Vector2.up;
                    }
                    if (Input.GetKey(KeyCode.DownArrow)) {
                        direction += Vector2.down;
                    }
                    if (Input.GetKey(KeyCode.RightArrow)) {
                        direction += Vector2.right;
                    }
                    if (Input.GetKey(KeyCode.LeftArrow)) {
                        direction += Vector2.left;
                    }
                    
                    actor.Move(direction);

                    if (Input.GetKeyDown(KeyCode.Space)) {
                        actor.JumpAsync(CancellationToken.None).Forget();
                    }
                }
            }
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            var uiManager = Services.Resolve<UIManager>();

            UniTask LoadAsync(string assetKey) {
                return uiManager.LoadSceneAsync(assetKey).RegisterTo(unloadScope).ToUniTask(cancellationToken: ct);
            }

            return UniTask.WhenAll(LoadAsync("ui_battle"));
        }

        /// <summary>
        /// Infrastructure初期化
        /// </summary>
        private IEnumerator SetupInfrastructureRoutine(IScope scope) {
            var assetManager = Services.Resolve<AssetManager>();
            var battleCharacterAssetRepository = new BattleCharacterAssetRepository(assetManager);
            ServiceContainer.RegisterInstance(battleCharacterAssetRepository).RegisterTo(scope);
            var bodyPrefabRepository = new BodyPrefabRepository(assetManager);
            ServiceContainer.RegisterInstance(bodyPrefabRepository).RegisterTo(scope);
            var environmentSceneRepository = new EnvironmentSceneRepository(assetManager);
            ServiceContainer.RegisterInstance(environmentSceneRepository).RegisterTo(scope);
            yield break;
        }

        /// <summary>
        /// Manager初期化
        /// </summary>
        private IEnumerator SetupManagerRoutine(IScope scope) {
            var fieldManager = new FieldManager();
            ServiceContainer.RegisterInstance(fieldManager).RegisterTo(scope);

            var actorEntityManager = new ActorEntityManager();
            ServiceContainer.RegisterInstance(actorEntityManager).RegisterTo(scope);

            var cameraManager = Services.Resolve<CameraManager>();
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