using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame.SituationSample {
    public class SampleSituationA : Situation {
        /// <summary>
        /// 読み込み
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
            Debug.Log($"Load Routine. [{GetType().Name}]");
        }

        /// <summary>
        /// 初期化
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);
            Debug.Log($"Setup Routine. [{GetType().Name}]");
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            if (Input.GetKeyDown(KeyCode.Space)) {
                ParentContainer.Transition(Services.Get<SituationSample>().SituationB);
            }
        }

        protected override void CleanupInternal(TransitionHandle handle) {
            Debug.Log($"Cleanup. [{GetType().Name}]");
            base.CleanupInternal(handle);
        }

        protected override void UnloadInternal(TransitionHandle handle) {
            Debug.Log($"Unload. [{GetType().Name}]");
            base.UnloadInternal(handle);
        }
    }
}