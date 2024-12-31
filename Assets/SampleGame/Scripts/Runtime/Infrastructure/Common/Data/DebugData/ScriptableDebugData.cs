using System;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// ScriptableObject管理型のDebugデータ
    /// Resourcesフォルダ以下にクラス名のファイルを配置してください
    /// </summary>
    [Serializable]
    public class ScriptableDebugData<T> : ScriptableObject
        where T : ScriptableDebugData<T> {
        private static T _instance;
        
        protected static T Instance {
            get {
                if (_instance == null) {
                    _instance = Resources.Load<T>(typeof(T).Name);
                }

                return _instance;
            }
        }
    }
}