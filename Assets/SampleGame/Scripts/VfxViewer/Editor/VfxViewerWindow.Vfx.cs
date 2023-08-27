using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.ProjectileSystems;
using GameFramework.VfxSystems;
using UnityEditor;
using UnityEngine;

namespace SampleGame.VfxViewer.Editor {
    /// <summary>
    /// VfxViewerのVfxパネル
    /// </summary>
    partial class VfxViewerWindow {
        /// <summary>
        /// ModelPanel
        /// </summary>
        private class VfxPanel : PanelBase {
            public override string Title => "Vfx";

            private SearchableList<string> _vfxList;

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal(IScope scope) {
                _vfxList = new SearchableList<string>();
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                var viewerModel = VfxViewerModel.Get();
                var appService = Services.Get<VfxViewerApplicationService>();
                var vfxManager = Services.Get<VfxManager>();
                var projectileObjectManager = Services.Get<ProjectileObjectManager>();
                var actorModel = viewerModel.PreviewVfxModel;

                var prevColor = GUI.color;

                using (new EditorGUILayout.VerticalScope("Box")) {
                    // Vfxの変更
                    var actorSetupDataIds = viewerModel.SetupData != null ? viewerModel.SetupData.vfxDataIds : Array.Empty<string>();
                    _vfxList.OnGUI(actorSetupDataIds, x => x, (id, index) => {
                        var current = actorModel.SetupDataId == id;
                        GUI.color = current ? Color.green : Color.gray;
                        if (GUILayout.Button(id)) {
                            appService.ChangePreviewVfxAsync(id, CancellationToken.None).Forget();
                        }
                    }, GUILayout.Width(Window.position.width));

                    GUI.color = prevColor;
                    
                    // Vfxの再生、停止
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button("Play")) {
                            appService.PlayCurrentVfx();
                        }

                        if (GUILayout.Button("Stop")) {
                            appService.StopCurrentVfx();
                        }
                    }
                    
                    // Vfxのキャッシュクリア
                    if (GUILayout.Button("Reset Pool")) {
                        vfxManager.Clear();
                        projectileObjectManager.Clear();
                    }
                }
                
                GUI.color = prevColor;
            }
        }
    }
}