using System;
using GameFramework.Core;
using UnityEngine;

namespace SampleGame.VfxViewer.Editor {
    /// <summary>
    /// VfxViewerのEnvironmentパネル
    /// </summary>
    partial class VfxViewerWindow {
        /// <summary>
        /// EnvironmentPanel
        /// </summary>
        private class EnvironmentPanel : PanelBase {
            public override string Title => "Environment";
            
            private SearchableList<string> _environmentIdList;

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal(IScope scope) {
                _environmentIdList = new SearchableList<string>();
            }
            
            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                var viewerModel = VfxViewerModel.Get();
                var appService = Services.Get<VfxViewerApplicationService>();
                var environmentModel = viewerModel.EnvironmentModel;

                var prevColor = GUI.color;
                
                // Environmentの変更
                var environmentIds = viewerModel.SetupData != null ? viewerModel.SetupData.environmentIds : Array.Empty<string>();
                _environmentIdList.OnGUI(environmentIds, x => x, (id, index) => {
                    var current = environmentModel.AssetId.Value == id;
                    GUI.color = current ? Color.green : Color.gray;
                    if (GUILayout.Button(id)) {
                        appService.ChangeEnvironment(id);
                    }
                });

                GUI.color = prevColor;
            }
        }
    }
}