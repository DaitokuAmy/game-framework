using System.Collections;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Application;
using SampleGame.Infrastructure;
using SampleGame.Presentation;
using UnityEngine;

namespace SampleGame.Main {
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
            var assetManager = Services.Get<AssetManager>();
            
            var tableRepository = new TableRepository(assetManager).ScopeTo(scope);
            ServiceContainer.Set(tableRepository).ScopeTo(scope);
        }
        
        /// <summary>
        /// Serviceの初期化
        /// </summary>
        private void SetupServices(IScope scope) {
        }
    }
}