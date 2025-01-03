using GameFramework.Core;
using SampleGame.Application.ModelViewer;
using UnityEngine;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// ModelViewerのEnvironmentパネル
    /// </summary>
    partial class ModelViewerWindow {
        /// <summary>
        /// EnvironmentPanel
        /// </summary>
        private class EnvironmentPanel : PanelBase {
            public override string Title => "Environment";
            
            private SearchableList<string> _environmentAssetKeyList;

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal(IScope scope) {
                _environmentAssetKeyList = new SearchableList<string>();
            }
            
            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                var appService = Services.Get<ModelViewerAppService>();
                var viewerModel = appService.DomainService.ModelViewerModel;
                var prevColor = GUI.color;
                
                // Environmentの変更
                var environmentIds = viewerModel.Master.EnvironmentAssetKeys;
                _environmentAssetKeyList.OnGUI(environmentIds, x => x, (key, index) => {
                    var current = viewerModel.EnvironmentAssetKey.Value == key;
                    GUI.color = current ? Color.green : Color.gray;
                    if (GUILayout.Button(key)) {
                        appService.ChangeEnvironment(index);
                    }
                });

                GUI.color = prevColor;
            }
        }
    }
}