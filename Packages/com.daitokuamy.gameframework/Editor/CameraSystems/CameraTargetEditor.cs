using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace GameFramework.CameraSystems.Editor {
    /// <summary>
    /// CameraTargetのエディタ拡張
    /// </summary>
    [CustomEditor(typeof(CameraTarget))]
    public class CameraTargetEditor : UnityEditor.Editor {
        private SerializedProperty _groupProp;
        private SerializedProperty _followTargetNameProp;
        private SerializedProperty _lookAtTargetNameProp;

        /// <summary>
        /// インスペクタ描画
        /// </summary>
        public override void OnInspectorGUI() {
            // 専用TargetGUI描画（失敗したら通常）
            if (!DrawTargetGUI()) {
                base.OnInspectorGUI();
            }
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            _groupProp = serializedObject.FindProperty("_group");
            _followTargetNameProp = serializedObject.FindProperty("_followTargetName");
            _lookAtTargetNameProp = serializedObject.FindProperty("_lookAtTargetName");
        }

        /// <summary>
        /// ターゲット指定用のGUI
        /// </summary>
        private bool DrawTargetGUI() {
            if (!Application.isPlaying) {
                return false;
            }

            var cameraTarget = target as CameraTarget;
            if (cameraTarget == null) {
                return false;
            }

            var cameraManager = cameraTarget.transform.GetComponentInParent<CameraManager>();
            if (cameraManager == null) {
                return false;
            }

            serializedObject.Update();

            var group = serializedObject.FindProperty("_group").objectReferenceValue as CameraGroup;
            var groupKey = group != null ? group.Key : CameraManager.MainCameraGroupKey;
            var targetPoints = cameraManager.GetTargetPoints(groupKey);
            var labels = targetPoints.Select(x => x.name).ToList();
            labels.Insert(0, "(None)");

            void DrawTarget(SerializedProperty prop) {
                var index = Mathf.Max(0, labels.IndexOf(prop.stringValue));
                index = EditorGUILayout.Popup(new GUIContent(prop.displayName, prop.tooltip), index, labels.ToArray());
                prop.stringValue = index > 0 ? labels[index] : "";
            }

            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(_groupProp);
            }

            DrawTarget(_followTargetNameProp);
            DrawTarget(_lookAtTargetNameProp);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed) {
                // 変更をリアルタイムに反映
                var virtualCamera = cameraTarget.GetComponent<CinemachineVirtualCameraBase>();
                if (virtualCamera != null) {
                    cameraTarget.SetupCameraTarget(cameraManager, virtualCamera);
                }
            }

            return true;
        }
    }
}