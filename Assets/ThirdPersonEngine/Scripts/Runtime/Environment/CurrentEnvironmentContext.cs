using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 現在の環境設定
    /// </summary>
    public class CurrentEnvironmentContext : IEnvironmentContext {
        public virtual EnvironmentDefaultSettings DefaultSettings => EnvironmentDefaultSettings.GetCurrent();
        public virtual Light Sun => RenderSettings.sun;
    }
}