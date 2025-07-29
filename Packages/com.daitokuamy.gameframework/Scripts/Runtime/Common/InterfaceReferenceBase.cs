using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework {
    /// <summary>
    /// Interfaceをシリアライズするためのクラス基底
    /// </summary>
    [Serializable]
    public abstract class InterfaceReferenceBase {
        [SerializeField]
        private Object _target;
        [SerializeField]
        private int _hoge;

        /// <summary>シリアライズされた参照</summary>
        protected Object Target => _target;
    }
}
