using System;

namespace GameFramework {
    /// <summary>
    /// Interfaceをシリアライズするためのクラス
    /// </summary>
    [Serializable]
    public sealed class InterfaceReference<TInterface> : InterfaceReferenceBase
        where TInterface : class {
        /// <summary>参照している値</summary>
        public TInterface Value => Target as TInterface;
    }
}