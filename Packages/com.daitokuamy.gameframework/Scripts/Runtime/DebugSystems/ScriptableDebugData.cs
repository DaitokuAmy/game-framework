using System;
using UnityEngine;

namespace GameFramework.DebugSystems {
    /// <summary>
    /// ScriptableObject管理型のDebugデータ
    /// Resourcesフォルダ以下にクラス名のファイルを配置してください
    /// </summary>
    [Serializable]
    public abstract class ScriptableDebugData<T> : ScriptableObject
        where T : ScriptableDebugData<T> {
        private static T s_instance;

        protected static T Instance {
            get {
                if (s_instance == null) {
                    s_instance = Resources.Load<T>(typeof(T).Name);
                }

                return s_instance;
            }
        }
    }
}