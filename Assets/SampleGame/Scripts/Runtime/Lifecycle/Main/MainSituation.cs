using System.Collections;
using GameFramework.AssetSystems;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Infrastructure;
using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// ランタイムの動作の基盤となるSituation
    /// </summary>
    public class MainSituation : Situation {
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            // スリープ禁止
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Repositoryの初期化
            SetupRepositories(scope);

            // Serviceの初期化
            SetupServices(scope);
        }

        /// <summary>
        /// Repositoryの初期化
        /// </summary>
        private void SetupRepositories(IScope scope) {
            var assetManager = Services.Resolve<AssetManager>();
            
            var tableRepository = new TableRepository(assetManager).RegisterTo(scope);
            ServiceContainer.RegisterInstance(tableRepository).RegisterTo(scope);
        }
        
        /// <summary>
        /// Serviceの初期化
        /// </summary>
        private void SetupServices(IScope scope) {
        }
    }
}