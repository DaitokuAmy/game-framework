using System;

namespace GameFramework.Core {
    /// <summary>
    /// ServiceInjectマーキング用Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method)]
    public class ServiceInjectAttribute : Attribute {
    }
}