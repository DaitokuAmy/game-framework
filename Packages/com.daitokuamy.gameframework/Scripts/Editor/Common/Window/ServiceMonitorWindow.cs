using System;
using GameFramework.DebugSystems.Editor;
using UnityEditor;

namespace GameFramework.Editor {
    /// <summary>
    /// Serviceの情報監視用ウィンドウ
    /// </summary>
    public partial class ServiceMonitorWindow : DebugWindowBase<ServiceMonitorWindow> {
        /// <summary>
        /// 型名タイプ
        /// </summary>
        private enum TypeNameType {
            Name,
            FullName,
        }

        private TypeNameType _typeNameType;

        /// <summary>
        /// 開く処理
        /// </summary>
        [MenuItem("Window/Game Framework/Service Monitor")]
        public static void Open() {
            GetWindow<ServiceMonitorWindow>(ObjectNames.NicifyVariableName(nameof(ServiceMonitorWindow)));
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void OnEnableInternal() {
            AddPanel(new ContainerPanel());
        }

        /// <summary>
        /// ヘッダー描画
        /// </summary>
        protected override void OnHeaderGuiInternal() {
            _typeNameType = (TypeNameType)EditorGUILayout.EnumPopup("Type Name", _typeNameType);
        }

        /// <summary>
        /// 型名称の取得
        /// </summary>
        private string GetTypeName(Type type) {
            switch (_typeNameType) {
                case TypeNameType.Name:
                    return type.Name;
                default:
                    return type.FullName;
            }
        }
    }
}