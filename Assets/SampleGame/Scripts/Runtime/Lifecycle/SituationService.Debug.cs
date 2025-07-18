using System.Linq;
using GameFramework;
using GameFramework.Core;
using UnityDebugMenu;
using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// SituationService
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// Situationのデバッグ登録
        /// </summary>
        private void SetupDebug(IScope scope) {
            var situationTypeIndex = 0;
            var leafSituationTypes = _situationFlow.GetSituations();
            var situationLabels = leafSituationTypes
                .Select(x => x.Name.Replace("SceneSituation", "").Replace("Situation", ""))
                .ToArray();

            var transitionType = TransitionType.ScreenDefault;
            DebugMenu.AddWindowItem("Situation", _ => {
                transitionType = DebugMenuUtil.EnumArrowOrderField("遷移タイプ", transitionType);
                situationTypeIndex = DebugMenuUtil.ArrowOrderField("シチュエーション", situationTypeIndex, situationLabels);

                GUILayout.Space(10);

                if (DebugMenuUtil.ButtonField("", "遷移")) {
                    Transition(leafSituationTypes[situationTypeIndex], null, transitionType);
                }

                if (DebugMenuUtil.ButtonField("", "戻る")) {
                    Back(null, transitionType);
                }

                if (DebugMenuUtil.ButtonField("", "リセット")) {
                    Reset();
                }
            }).RegisterTo(scope);
        }
    }
}