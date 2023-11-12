using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SampleGame.Editor {
    /// <summary>
    /// SerializeFieldを自動設定するツール
    /// </summary>
    public static class SerializeFieldSetter {
        [MenuItem("CONTEXT/MonoBehaviour/UI Support/Set Serialize Field By Name")]
        private static void SetFieldsByName(MenuCommand command) {
            SetFields(command.context as MonoBehaviour, prop => {
                var targetName = ObjectNames.NicifyVariableName(prop.name);
                return gameObj => IsTarget(gameObj, targetName);
            });
        }

        /// <summary>
        /// ターゲットの設定
        /// </summary>
        private static void SetFields(MonoBehaviour target, Func<SerializedProperty, Func<GameObject, bool>> checkTargetFunc) {
            if (target == null) {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var itr = serializedObject.GetIterator();

            serializedObject.Update();

            itr.NextVisible(true); // Scriptを飛ばす
            while (itr.NextVisible(true)) {
                if (itr.propertyType != SerializedPropertyType.ObjectReference ||
                    itr.objectReferenceValue != null) {
                    continue;
                }

                // 探す
                var checkFunc = checkTargetFunc.Invoke(itr);
                var foundObj = FindGameObject(target.gameObject, x => checkFunc(x));
                if (foundObj == null) {
                    continue;
                }

                itr.objectReferenceValue = foundObj;
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 対象となるGameObjectを探す
        /// </summary>
        private static GameObject FindGameObject(GameObject root, Func<GameObject, bool> checkTarget) {
            var targets = root.GetComponentsInChildren<Transform>(true)
                .Select(x => x.gameObject)
                .ToArray();
            foreach (var target in targets) {
                if (checkTarget(target)) {
                    return target;
                }
            }

            return null;
        }

        /// <summary>
        /// 対象かチェック
        /// </summary>
        private static bool IsTarget(GameObject gameObj, string name) {
            var objName = gameObj.name.Replace(" ", "");
            var searchName = name.Replace(" ", "");
            return objName == searchName;
        }
    }
}