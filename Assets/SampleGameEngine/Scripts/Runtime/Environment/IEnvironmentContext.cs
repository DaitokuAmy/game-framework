using GameFramework.EnvironmentSystem;
using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// 環境設定
    /// </summary>
    public interface IEnvironmentContext : GameFramework.EnvironmentSystem.IEnvironmentContext {
        EnvironmentDefaultSettings DefaultSettings { get; }
        Light Sun { get; }
    }
}