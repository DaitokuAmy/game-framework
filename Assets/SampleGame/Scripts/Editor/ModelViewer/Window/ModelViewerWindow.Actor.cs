using System;
using System.Linq;
using GameFramework.Core;
using GameFramework.DebugSystems.Editor;
using SampleGame.Application.ModelViewer;
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

            private SearchableList<string> _modelList;
            private SearchableList<AnimationClip> _motionList;

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void StartInternal(ModelViewerWindow window, IScope scope) {
                _modelList = new SearchableList<string>();
                _motionList = new SearchableList<AnimationClip>();
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void DrawGuiInternal(ModelViewerWindow window) {
                var appService = window.Resolver.Resolve<ModelViewerAppService>();
                var viewerModel = appService.DomainService.ModelViewerModel;
                var actorModel = appService.DomainService.PreviewActorModel;
                var settingsModel = appService.DomainService.SettingsModel;

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
                    var actorAssetKeys = viewerModel.Master.ActorAssetKeys.ToArray();
                    _modelList.OnGUI(actorAssetKeys, x => x, (key, index) => {
                        var current = actorModel != null && actorModel.Master.DisplayName == key;
                        GUI.color = current ? Color.green : Color.gray;
                        if (GUILayout.Button(key)) {
                            appService.ChangePreviewActor(index);
                        }
                    }, GUILayout.Width(window.position.width * 0.3f));

                    GUI.color = prevColor;

                    if (actorModel != null) {
                        // ActorMotionの変更
                        var animationClips = actorModel.Master != null ? actorModel.Master.AnimationClips.ToArray() : Array.Empty<AnimationClip>();
                        _motionList.OnGUI(animationClips, x => x.name, (clip, index) => {
                            if (clip == null) {
                                return;
                            }

                            var clipName = clip.name;
                            var current = actorModel.CurrentAnimationClip == clip;
                            var currentAdditive = actorModel.CurrentAdditiveAnimationClip == clip;

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
                    }

                    GUI.color = prevColor;
                }

                GUI.color = prevColor;
            }
        }
    }
}