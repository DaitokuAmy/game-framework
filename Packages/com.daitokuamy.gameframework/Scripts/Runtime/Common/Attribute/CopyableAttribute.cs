using System;

namespace GameFramework {
    /// <summary>
    /// コピー可能なプロパティにマーキングするためのAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CopyableAttribute : Attribute {
    }
}
