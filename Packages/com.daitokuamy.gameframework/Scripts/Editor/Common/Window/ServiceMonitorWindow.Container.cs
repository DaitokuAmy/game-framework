using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEditor;

namespace GameFramework.Editor {
    /// <summary>
    /// Serviceの情報監視用ウィンドウ
    /// </summary>
    partial class ServiceMonitorWindow {
        /// <summary>
        /// コンテナ情報描画用パネル
        /// </summary>
        private class ContainerPanel : PanelBase {
            private readonly List<(Type type, object instance)> _registeredServiceInfos = new();

            /// <inheritdoc/>
            public override string Label => "Container";

            /// <inheritdoc/>
            protected override void DrawGuiInternal(ServiceMonitorWindow window) {
                var containers = ServiceMonitor.Containers;

                foreach (var container in containers) {
                    DrawContainer(container, window);
                }
            }

            /// <summary>
            /// コンテナ情報の描画
            /// </summary>
            private void DrawContainer(IMonitoredServiceContainer container, ServiceMonitorWindow window) {
                DrawFoldoutContent(container.Label, container, target => {
                    _registeredServiceInfos.Clear();
                    target.GetRegisteredServiceInfos(_registeredServiceInfos);

                    // 登録中サービス情報
                    var prevLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 240.0f;
                    EditorGUILayout.LabelField("Services", EditorStyles.boldLabel);
                    for (var i = 0; i < _registeredServiceInfos.Count; i++) {
                        var (type, instance) = _registeredServiceInfos[i];
                        EditorGUILayout.LabelField(window.GetTypeName(type), instance?.ToString() ?? "null");
                    }
                    EditorGUIUtility.labelWidth = prevLabelWidth;
                }, true);
            }
        }
    }
}