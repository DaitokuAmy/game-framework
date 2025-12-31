using System.Collections;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// ランタイムの動作の基盤となるSituation
    /// </summary>
    public class MainSituation : Situation {
        /// <inheritdoc/>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
        }

        /// <inheritdoc/>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            // スリープ禁止
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            // FPS初期化
            UnityEngine.Application.targetFrameRate = 60;
        }
    }
}