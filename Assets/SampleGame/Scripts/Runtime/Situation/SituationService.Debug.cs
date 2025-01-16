using System.Linq;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Introduction;
using SampleGame.ModelViewer;
using SampleGame.Presentation;
using UnityDebugMenu;

namespace SampleGame {
    /// <summary>
    /// SituationService
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// Situationのデバッグ登録
        /// </summary>
        private void SetupDebug(IScope scope) {
            var situationTypeIndex = 0;
            var situationTypes = new[] {
                typeof(IntroductionSceneSituation),
                typeof(ModelViewerSceneSituation),
            };
            var situationLabels = situationTypes
                .Select(x => x.Name.Replace("SceneSituation", "").Replace("Situation", ""))
                .ToArray();

            var backTypeIndex = 0;
            var backTypeLabels = new[] { "Loading", "Cross", "Default" };
            DebugMenu.AddWindowItem("Situation", _ => {
                backTypeIndex = DebugMenuUtil.ArrowOrderField("戻り遷移タイプ", backTypeIndex, backTypeLabels);
                if (DebugMenuUtil.ButtonField("", "戻る")) {
                    switch (backTypeIndex) {
                        case 0:
                            Back(null, TransitionUtility.CreateLoadingTransitionEffects());
                            break;
                        case 1:
                            Back(new CrossTransition());
                            break;
                        case 2:
                            Back();
                            break;
                    }
                }

                situationTypeIndex = DebugMenuUtil.ArrowOrderField("シチュエーション", situationTypeIndex, situationLabels);
                if (DebugMenuUtil.ButtonField("", "遷移")) {
                    Transition(situationTypes[situationTypeIndex], null, null, TransitionUtility.CreateLoadingTransitionEffects());
                }

                if (DebugMenuUtil.ButtonField("", "リセット")) {
                    Reset(TransitionUtility.CreateLoadingTransitionEffects());
                }
            }).ScopeTo(scope);
        }
    }
}