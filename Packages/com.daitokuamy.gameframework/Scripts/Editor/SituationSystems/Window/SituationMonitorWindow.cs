using System;
using GameFramework.DebugSystems.Editor;
using GameFramework.SituationSystems;
using UnityEditor;

namespace GameFramework.Editor {
    /// <summary>
    /// Situationの情報監視用ウィンドウ
    /// </summary>
    public partial class SituationMonitorWindow : DebugWindowBase<SituationMonitorWindow> {
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
        [MenuItem("Window/Game Framework/Situation Monitor")]
        public static void Open() {
            GetWindow<SituationMonitorWindow>(ObjectNames.NicifyVariableName(nameof(SituationMonitorWindow)));
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void OnEnableInternal() {
            AddPanel(new ContainerPanel());
            AddPanel(new FlowPanel());
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

        /// <summary>
        /// Situation名の取得
        /// </summary>
        private string GetSituationName(Situation situation) {
            if (situation == null) {
                return "None";
            }

            return GetTypeName(situation.GetType());
        }
    }
}