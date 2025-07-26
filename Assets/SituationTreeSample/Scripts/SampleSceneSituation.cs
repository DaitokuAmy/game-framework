using System.Collections;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SituationTreeSample {
    /// <summary>
    /// サンプル用のSceneSituation
    /// </summary>
    public abstract class SampleSceneSituation : SceneSituation, ISampleSituation {
        private const float LoadDuration = 0.2f;
        private const float SetupDuration = 0.05f;
        
        private GameObject _sampleObject;

        /// <summary>アンロード時の空シーンのアセットパス(未指定だとアンロードでシーンを廃棄しない)</summary>
        protected override string EmptySceneAssetPath => "Assets/SituationTreeSample/Scenes/situation_flow_tree_empty.unity";
        
        /// <summary>表示名</summary>
        string ISampleSituation.DisplayName => GetType().Name.Replace("SampleSceneSituation", "");
        
        /// <summary>
        /// 読み込み
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            Debug.Log($"Load Routine. [{GetType().Name}]");
            yield return base.LoadRoutineInternal(handle, scope);
            yield return new WaitForSeconds(LoadDuration);
        }

        /// <summary>
        /// 初期化
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            Debug.Log($"Setup Routine. [{GetType().Name}]");
            yield return base.SetupRoutineInternal(handle, scope);
            yield return new WaitForSeconds(SetupDuration);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle<Situation> handle, IScope scope) {
            Debug.Log($"Activate. [{GetType().Name}]");
            base.ActivateInternal(handle, scope);
            _sampleObject = new GameObject($"Active Node:{GetType().Name}");
            _sampleObject.transform.SetParent(Services.Resolve<SituationTreeSample>().transform);
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal(TransitionHandle<Situation> handle) {
            Debug.Log($"Deactivate. [{GetType().Name}]");
            Object.Destroy(_sampleObject);
            _sampleObject = null;
            base.DeactivateInternal(handle);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        protected override void CleanupInternal(TransitionHandle<Situation> handle) {
            Debug.Log($"Cleanup. [{GetType().Name}]");
            base.CleanupInternal(handle);
        }

        /// <summary>
        /// アンロード処理
        /// </summary>
        protected override void UnloadInternal(TransitionHandle<Situation> handle) {
            Debug.Log($"Unload. [{GetType().Name}]");
            base.UnloadInternal(handle);
        }
    }
}