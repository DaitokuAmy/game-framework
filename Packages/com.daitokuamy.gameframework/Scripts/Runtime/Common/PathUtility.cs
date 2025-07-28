using System.IO;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// Path解決用のユーティリティ
    /// </summary>
    public static class PathUtility {
        /// <summary>Assetsフォルダを格納しているフォルダパス</summary>
        public static readonly string BasePath = Path.Combine(Application.dataPath, "..");
        
        /// <summary>
        /// フルパスを相対パスに変換
        /// </summary>
        public static string GetRelativePath(string fullPath) {
            return Path.GetRelativePath(BasePath, fullPath);
        }
    }
}