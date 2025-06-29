using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// フラグ表示コントロールを行うためのPropertyAttribute
    /// </summary>
    public class VisibleFlagAttribute : PropertyAttribute {
        /// <summary>表示フラグ制御用のプロパティ名</summary>
        public string FlagPropertyName { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="flagPropertyName">表示フラグ制御用のプロパティ名</param>
        public VisibleFlagAttribute(string flagPropertyName) {
            FlagPropertyName = flagPropertyName;
        }
    }
}