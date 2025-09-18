using System;
using System.Linq;
using GameFramework.ActorSystems;
using GameFramework.Core;
using GameFramework.DebugSystems.Editor;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using ThirdPersonEngine.ModelViewer;
using UnityEditor;
using UnityEngine;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// ModelViewerのActorパネル
    /// </summary>
    partial class ModelViewerWindow {
        /// <summary>
        /// ModelPanel
        /// </summary>
        private class ActorPanel : PanelBase {
            public override string Label => "Actor";

            private SearchableList<IModelViewerActorMaster> _actorList;
            private SearchableList<AnimationClip> _motionClipList;

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void StartInternal(ModelViewerWindow window, IScope scope) {
                _actorList = new SearchableList<IModelViewerActorMaster>();
                _motionClipList = new SearchableList<AnimationClip>();
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void DrawGuiInternal(ModelViewerWindow window) {
                var appService = window.Resolver.Resolve<ModelViewerAppService>();
                var tableRepository = window.Resolver.Resolve<IModelViewerTableRepository>();
                var entityManager = window.Resolver.Resolve<ActorEntityManager>();
                var viewerModel = appService.DomainService.ModelViewerModel;
                var actorModel = appService.DomainService.PreviewActorModel;
                var settingsModel = appService.DomainService.SettingsModel;
                var actor = actorModel != null ? entityManager.FindEntity(actorModel.Id)?.GetActor<PreviewActor>() : null;
                if (actor == null) {
                    return;
                }
                
                var prevColor = GUI.color;

                if (GUILayout.Button("状態リセット", GUILayout.Width(300.0f))) {
                    appService.ResetActor();
                }

                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var resetOnPlay = settingsModel.ResetOnPlay;
                    resetOnPlay = GUILayout.Toggle(resetOnPlay, "再生時リセット");
                    if (scope.changed) {
                        appService.SetResetOnPlay(resetOnPlay);
                    }
                }

                using (new EditorGUILayout.HorizontalScope("Box")) {
                    // Actorの変更
                    var actorMasters = viewerModel.Master.ActorIds.Select(tableRepository.FindModelViewerActorById).ToArray();
                    _actorList.OnGUI(actorMasters, x => x.Name, (master, index) => {
                        var current = actorModel.Master == master;
                        GUI.color = current ? Color.green : Color.gray;
                        if (GUILayout.Button(master.Name)) {
                            appService.ChangeActor(index);
                        }
                    }, GUILayout.Width(window.position.width * 0.3f));

                    GUI.color = prevColor;

                    // ActorMotionの変更
                    var motionClips = actor.GetMotionClips();
                    _motionClipList.OnGUI(motionClips, x => x.name, (clip, index) => {
                        if (clip == null) {
                            return;
                        }

                        var clipName = clip.name;
                        var current = actorModel.CurrentAnimationClipIndex == index;
                        var currentAdditive = actorModel.CurrentAdditiveAnimationClipIndex == index;

                        using (new EditorGUILayout.HorizontalScope()) {
                            GUI.color = current ? Color.green : Color.gray;
                            if (GUILayout.Button(clipName)) {
                                appService.ChangeAnimationClip(index);
                            }

                            GUI.color = currentAdditive ? Color.green : Color.gray;
                            if (GUILayout.Button("Add", GUILayout.Width(100))) {
                                appService.ToggleAdditiveAnimationClip(index);
                            }
                        }
                    });

                    GUI.color = prevColor;
                }

                GUI.color = prevColor;
            }
        }
    }
}