using System.Collections;
using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// RootNode
    /// </summary>
    public class RootNode : GameFramework.NavigationSystems.RootNode {
        /// <inheritdoc/>
        protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield return base.InitializeRoutine(handle, scope);

            // スリープ禁止
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            // FPS初期化
            UnityEngine.Application.targetFrameRate = 60;
        }
    }
}