using System.Linq;
using GameFramework.Core;
using GameFramework.DebugSystems.Editor;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
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
            public override string Label => "Environment";

            private SearchableList<IModelViewerEnvironmentMaster> _environmentMasterList;

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void StartInternal(ModelViewerWindow window, IScope scope) {
                _environmentMasterList = new SearchableList<IModelViewerEnvironmentMaster>();
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void DrawGuiInternal(ModelViewerWindow window) {
                var appService = window.Resolver.Resolve<ModelViewerAppService>();
                var tableRepository = window.Resolver.Resolve<IModelViewerTableRepository>();
                var viewerModel = appService.DomainService.ModelViewerModel;
                var environmentModel = viewerModel.EnvironmentActorModel;
                var prevColor = GUI.color;
                
                // Environmentの変更
                var environmentMasters = viewerModel.Master.EnvironmentIds.Select(tableRepository.FindModelViewerEnvironmentById).ToArray();
                _environmentMasterList.OnGUI(environmentMasters, x => x.Name, (master, index) => {
                    var current = environmentModel != null && environmentModel.Master == master;
                    GUI.color = current ? Color.green : Color.gray;
                    if (GUILayout.Button(master.Name)) {
                        appService.ChangeEnvironment(index);
                    }

                    GUI.color = prevColor;
                });

                GUI.color = prevColor;
            }
        }
    }
}