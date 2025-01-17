using System.Collections;
using System.Linq;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;
using UniRx;

namespace SituationFlowSample {
    /// <summary>
    /// サンプル用のNodeSituation(Flowの末端扱い)
    /// </summary>
    public abstract class SampleNodeSituation : Situation {
        // 遷移先候補
        private static readonly string[] TransitionItemNames = {
            "A1", "A2", "B1", "B21", "B22"
        };

        /// <summary>ノードを表すキー</summary>
        private string NodeKey => GetType().Name.Replace("SampleNodeSituation", "");
        
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

            var currentIndex = TransitionItemNames.ToList().IndexOf(NodeKey);
            var sample = Services.Get<SituationFlowSample>();
            sample.MenuView.Title = NodeKey;
            sample.MenuView.SetupItems(currentIndex, TransitionItemNames);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        protected override void CleanupInternal(TransitionHandle handle) {
            Debug.Log($"Cleanup. [{GetType().Name}]");
            base.CleanupInternal(handle);
        }

        /// <summary>
        /// アンロード処理
        /// </summary>
        protected override void UnloadInternal(TransitionHandle handle) {
            Debug.Log($"Unload. [{GetType().Name}]");
            base.UnloadInternal(handle);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);
            Debug.Log($"Activate. [{GetType().Name}]");

            var sample = Services.Get<SituationFlowSample>();
            // sample.MenuView.BackSubject
            //     .TakeUntil(scope)
            //     .Subscribe(_ => Back());

            // sample.MenuView.SelectedSubject
            //     .TakeUntil(scope)
            //     .Subscribe(index => {
            //         switch (index) {
            //             case 0:
            //                 Transition<SampleNodeSituationA1>();
            //                 break;
            //             case 1:
            //                 Transition<SampleNodeSituationA2>();
            //                 break;
            //             case 2:
            //                 Transition<SampleNodeSituationB1>();
            //                 break;
            //             case 3:
            //                 Transition<SampleNodeSituationB21>();
            //                 break;
            //             case 4:
            //                 Transition<SampleNodeSituationB22>();
            //                 break;
            //         }
            //     });
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal(TransitionHandle handle) {
            Debug.Log($"Deactivate. [{GetType().Name}]");
            base.DeactivateInternal(handle);
        }
    }
}