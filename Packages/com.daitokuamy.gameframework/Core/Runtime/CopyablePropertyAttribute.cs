using System;

namespace GameFramework.Core {
    /// <summary>
    /// コピー可能なプロパティにマーキングするためのAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CopyablePropertyAttribute : Attribute {
    }
}
