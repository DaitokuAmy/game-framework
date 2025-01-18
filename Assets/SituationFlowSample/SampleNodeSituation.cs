using System.Collections;
using System.Linq;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UniRx;

namespace SituationFlowSample {
    /// <summary>
    /// サンプル用のNodeSituation(Flowの末端扱い)
    /// </summary>
    public abstract class SampleNodeSituation : SampleSituation {
        /// <summary>遷移先候補</summary>
        private static readonly string[] TransitionItemNames = {
            "A1", "A2", "B1", "B21", "B22"
        };

        /// <summary>ノードを表すキー</summary>
        private string NodeKey => GetType().Name.Replace("SampleNodeSituation", "");

        /// <summary>
        /// 初期化
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);
            
            var currentIndex = TransitionItemNames.ToList().IndexOf(NodeKey);
            var sample = Services.Get<SituationFlowSample>();
            sample.MenuView.Title = NodeKey;
            sample.MenuView.SetupItems(currentIndex, TransitionItemNames);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var sample = Services.Get<SituationFlowSample>();
            sample.MenuView.BackSubject
                .TakeUntil(scope)
                .Subscribe(_ => sample.Flow.Back());

            sample.MenuView.SelectedSubject
                .TakeUntil(scope)
                .Subscribe(index => {
                    switch (index) {
                        case 0:
                            sample.Flow.Transition<SampleNodeSituationA1>();
                            break;
                        case 1:
                            sample.Flow.Transition<SampleNodeSituationA2>();
                            break;
                        case 2:
                            sample.Flow.Transition<SampleNodeSituationB1>();
                            break;
                        case 3:
                            sample.Flow.Transition<SampleNodeSituationB21>();
                            break;
                        case 4:
                            sample.Flow.Transition<SampleNodeSituationB22>();
                            break;
                    }
                });
        }
    }
}