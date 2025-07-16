using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// ObjectSelectorを利用するためのPropertyAttribute
    /// </summary>
    public class ObjectSelectorAttribute : PropertyAttribute {
        /// <summary>デフォルトで含めるフィルター</summary>
        public string DefaultFilter { get; }
        /// <summary>ルートフォルダーパス</summary>
        public string RootFolder { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filter">デフォルトで含めるフィルター</param>
        /// <param name="rootFolder">ルートフォルダーパス</param>
        public ObjectSelectorAttribute(string filter = "", string rootFolder = "Assets") {
            DefaultFilter = filter;
            RootFolder = rootFolder;
        }
    }
}