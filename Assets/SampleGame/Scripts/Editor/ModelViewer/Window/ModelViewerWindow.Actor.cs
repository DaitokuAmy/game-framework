using System;
using GameFramework.Core;
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
            public override string Title => "Actor";

            private SearchableList<string> _modelList;
            private SearchableList<AnimationClip> _motionList;

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal(IScope scope) {
                _modelList = new SearchableList<string>();
                _motionList = new SearchableList<AnimationClip>();
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                var appService = Services.Get<ModelViewerAppService>();
                var viewerModel = appService.DomainService.ModelViewerModel;
                var actorModel = appService.DomainService.PreviewActorModel;

                var prevColor = GUI.color;

                if (GUILayout.Button("状態リセット", GUILayout.Width(300.0f))) {
                    appService.ResetActor();
                }

                using (new EditorGUILayout.HorizontalScope("Box")) {
                    // Actorの変更
                    var actorAssetKeys = viewerModel.Master.ActorAssetKeys;
                    _modelList.OnGUI(actorAssetKeys, x => x, (key, index) => {
                        var current = actorModel != null && actorModel.Master.Name == key;
                        GUI.color = current ? Color.green : Color.gray;
                        if (GUILayout.Button(key)) {
                            appService.ChangePreviewActor(index);
                        }
                    }, GUILayout.Width(Window.position.width * 0.3f));

                    GUI.color = prevColor;

                    if (actorModel != null) {
                        // ActorMotionの変更
                        var animationClips = actorModel.Master != null ? actorModel.Master.AnimationClips : Array.Empty<AnimationClip>();
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