using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SituationFlowSample {
    /// <summary>
    /// SituationFlow用のサンプル
    /// </summary>
    public class SituationFlowSample : MonoBehaviour {
        [SerializeField, Tooltip("遷移先選択用メニュー")]
        private TransitionMenuView _transitionMenuView;
        
        private SituationContainer _rootContainer;
        private SituationFlow _situationFlow;

        /// <summary>遷移先選択メニューView</summary>
        public TransitionMenuView MenuView => _transitionMenuView;

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Start() {
            Services.Instance.Set(this);
            _rootContainer = new SituationContainer(null, false);
            
            // シチュエーションAの依存的な階層構造構築
            // SituationA
            //   SituationA1
            //   SituationA2
            var situationA = new SampleSituationA();
            _rootContainer.PreRegister(situationA);
            situationA.CreateChildContainer(0, false); // 子階層を準備
            var situationA1 = new SampleNodeSituationA1();
            situationA.RegisterChild(situationA1);
            var situationA2 = new SampleNodeSituationA2();
            situationA.RegisterChild(situationA2);
            
            // シチュエーションBの依存的な階層構造構築
            // SituationB
            //   SituationB1
            //   SituationB2
            //     SituationB21
            //     SituationB22
            var situationB = new SampleSituationB();
            _rootContainer.PreRegister(situationB);
            situationB.CreateChildContainer(0, false); // 子階層を準備
            var situationB1 = new SampleNodeSituationB1();
            situationB.RegisterChild(situationB1);
            var situationB2 = new SampleSituationB2();
            situationB2.CreateChildContainer(0, false); // 孫階層を準備
            situationB.RegisterChild(situationB2);
            var situationB21 = new SampleNodeSituationB21();
            situationB2.RegisterChild(situationB21);
            var situationB22 = new SampleNodeSituationB22();
            situationB2.RegisterChild(situationB22);
            
            // シチュエーションの遷移関係を構築
            _situationFlow = new SituationFlow(situationA1);
            var a1Node = _situationFlow.RootNode;
            var a2Node = a1Node.Connect(situationA2); // A1 -> A2
            var b1Node = a2Node.Connect(situationB1); // A2 -> B1
            var b21Node = b1Node.Connect(situationB21); // B1 -> B21
            var b22Node = b1Node.Connect(situationB22); // B1 -> B22
            
            // Fallback
            _situationFlow.SetFallbackNode(a1Node);
            _situationFlow.SetFallbackNode(b1Node);
            
            // 遷移
            _rootContainer.Transition(situationA);
            
            // Flowの初期化
            _situationFlow.SetupAsync();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            _situationFlow.Dispose();
            _rootContainer.Dispose();
            Services.Instance.Remove(GetType());
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            _situationFlow.Update();
            _rootContainer.Update();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            _rootContainer.LateUpdate();
        }
    }
}