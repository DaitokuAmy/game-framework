using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
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
                var viewerModel = ModelViewerModel.Get();
                var appService = Services.Get<ModelViewerApplicationService>();
                var actorModel = viewerModel.PreviewActorModel;

                var prevColor = GUI.color;

                using (new EditorGUILayout.HorizontalScope("Box")) {
                    // Actorの変更
                    var actorSetupDataIds = viewerModel.SetupData != null ? viewerModel.SetupData.actorDataIds : Array.Empty<string>();
                    _modelList.OnGUI(actorSetupDataIds, x => x, (id, index) => {
                        var current = actorModel.SetupDataId == id;
                        GUI.color = current ? Color.green : Color.gray;
                        if (GUILayout.Button(id)) {
                            appService.ChangePreviewActorAsync(id, CancellationToken.None).Forget();
                        }
                    }, GUILayout.Width(Window.position.width * 0.3f));

                    GUI.color = prevColor;
                
                    // ActorMotionの変更
                    var animationClips = actorModel.SetupData.Value != null ? actorModel.SetupData.Value.animationClips : Array.Empty<AnimationClip>();
                    _motionList.OnGUI(animationClips, x => x.name, (clip, index) => {
                        if (clip == null) {
                            return;
                        }
                            
                        var clipName = clip.name;
                        var current = actorModel.CurrentAnimationClip.Value == clip;
                        var currentAdditive = actorModel.CurrentAdditiveAnimationClip.Value == clip;

                        using (new EditorGUILayout.HorizontalScope()) {
                            GUI.color = current ? Color.green : Color.gray;
                            if (GUILayout.Button(clipName)) {
                                actorModel.ChangeClip(index);
                            }
                                
                            GUI.color = currentAdditive ? Color.green : Color.gray;
                            if (GUILayout.Button("Add", GUILayout.Width(100))) {
                                actorModel.ToggleAdditiveClip(index);
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