using GameFramework.Core;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// Unity用のDeltaTimeProvider
    /// </summary>
    public class UnityDeltaTimeProvider : IDeltaTimeProvider {
        float IDeltaTimeProvider.DeltaTime => Time.deltaTime;
    }
}
