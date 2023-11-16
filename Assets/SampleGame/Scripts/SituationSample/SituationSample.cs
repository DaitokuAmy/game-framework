using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame.SituationSample {
    public class SituationSample : MonoBehaviour {
        private SituationContainer _rootContainer;
    
        public Situation SituationA { get; private set; }
        public Situation SituationB { get; private set; }

        private void Awake() {
            Services.Instance.Set(this);
            _rootContainer = new SituationContainer();
            SituationA = new SampleSituationA();
            SituationB = new SampleSituationB();
            _rootContainer.PreLoad(SituationA);
            _rootContainer.PreLoad(SituationB);
            _rootContainer.Transition(SituationA);
        }

        private void OnDestroy() {
            _rootContainer.Dispose();
        }

        private void Update() {
            _rootContainer.Update();
        }

        private void LateUpdate() {
            _rootContainer.LateUpdate();
        }
    }
}