using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 環境設定
    /// </summary>
    public interface IEnvironmentContext : GameFramework.EnvironmentSystems.IEnvironmentContext {
        EnvironmentDefaultSettings DefaultSettings { get; }
        Light Sun { get; }
    }
}