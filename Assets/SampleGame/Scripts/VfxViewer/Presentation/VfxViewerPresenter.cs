using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.CameraSystems;
using GameFramework.VfxSystems;
using GameFramework.Core;
using GameFramework.LogicSystems;
using GameFramework.ProjectileSystems;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// VfxViewer全体のPresenter
    /// </summary>
    public class VfxViewerPresenter : Logic {
        private VfxViewerModel _model;
        private EnvironmentManager _environmentManager;
        private VfxManager.Handle _vfxHandle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VfxViewerPresenter(VfxViewerModel model) {
            _model = model;
            _environmentManager = Services.Get<EnvironmentManager>();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            var ct = scope.Token;

            var cameraManager = Services.Get<CameraManager>();
            var vfxManager = Services.Get<VfxManager>();
            var projectileObjectManager = Services.Get<ProjectileObjectManager>();
            var settings = Services.Get<VfxViewerSettings>();
            var settingsModel = _model.SettingsModel;

            // Vfxの切り替え
            _model.PreviewVfxModel.SetupData
                .TakeUntil(scope)
                .Subscribe(setupData => {
                    _vfxHandle.Dispose();
                    if (setupData.vfxType == VfxType.Default) {
                        _vfxHandle = vfxManager.Get(new VfxManager.Context {
                            localScale = Vector3.one,
                            prefab = setupData.prefab,
                            relativePosition = settings.StartPoint != null ? settings.StartPoint.position : Vector3.zero,
                            relativeAngles = settings.StartPoint != null ? settings.StartPoint.eulerAngles : Vector3.zero
                        });
                        _vfxHandle.ScopeTo(scope);
                    }
                });
            
            // Vfxの再生
            _model.PreviewVfxModel.OnPlaySubject
                .TakeUntil(scope)
                .Subscribe(setupData => {
                    if (setupData.vfxType == VfxType.Default) {
                        if (_vfxHandle.IsValid) {
                            _vfxHandle.Play();
                        }
                    }
                    else if (setupData.vfxType == VfxType.Projectile) {
                        var context = settings.ProjectileContext;
                        if (settings.StartPoint != null) {
                            context.startPoint = settings.StartPoint.TransformPoint(context.startPoint);
                            context.startVelocity = settings.StartPoint.TransformVector(context.startVelocity);
                            context.acceleration = settings.StartPoint.TransformVector(context.acceleration);
                        }

                        var handle = projectileObjectManager.Play(setupData.prefab, new StraightBulletProjectile(settings.ProjectileContext), -1, 1);
                        handle.ScopeTo(scope);
                    }
                });
            
            // Vfxの停止
            _model.PreviewVfxModel.OnStopSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    if (_vfxHandle.IsValid) {
                        _vfxHandle.Stop();
                    }
                });

            // Environmentの切り替え
            _model.EnvironmentModel.AssetId
                .TakeUntil(scope)
                .Subscribe(id => { _environmentManager.ChangeEnvironmentAsync(id, ct).Forget(); });

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

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            var appService = Services.Get<VfxViewerApplicationService>();

            // VFXの再適用
            if (Keyboard.current[Key.Space].wasPressedThisFrame) {
                appService.PlayCurrentVfx();
            }
            
            // VFXのIndex更新
            var setupIds = _model.SetupData.vfxDataIds.ToList();
            var currentIndex = setupIds.IndexOf(_model.PreviewVfxModel.SetupDataId);
            if (Keyboard.current[Key.UpArrow].wasPressedThisFrame) {
                var prevIndex = (currentIndex - 1 + setupIds.Count) % setupIds.Count;
                appService.ChangePreviewVfxAsync(setupIds[prevIndex], CancellationToken.None).Forget();
            }
            if (Keyboard.current[Key.DownArrow].wasPressedThisFrame) {
                var nextIndex = (currentIndex + 1) % setupIds.Count;
                appService.ChangePreviewVfxAsync(setupIds[nextIndex], CancellationToken.None).Forget();
            }
        }
    }
}