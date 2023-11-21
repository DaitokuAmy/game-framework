using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SituationSample {
    public class SituationSample : MonoBehaviour {
        private SituationRunner _situationRunner;
    
        public Situation SituationA { get; private set; }
        public Situation SituationB { get; private set; }

        private void Awake() {
            Services.Instance.Set(this);
            _situationRunner = new SituationRunner();
            SituationA = new SampleSituationA();
            SituationB = new SampleSituationB();
            _situationRunner.Container.PreLoadAsync(SituationA);
            _situationRunner.Container.PreLoadAsync(SituationB);
            _situationRunner.Container.Transition(SituationA);
        }

        private void OnDestroy() {
            _situationRunner.Dispose();
        }

        private void Update() {
            _situationRunner.Update();
        }

        private void LateUpdate() {
            _situationRunner.LateUpdate();
        }
    }
}