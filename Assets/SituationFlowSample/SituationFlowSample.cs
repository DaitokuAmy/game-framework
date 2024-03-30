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
        
        private SituationRunner _situationRunner;
        private SituationFlow _situationFlow;

        /// <summary>遷移先選択メニューView</summary>
        public TransitionMenuView MenuView => _transitionMenuView;

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Start() {
            Services.Instance.Set(this);
            _situationRunner = new SituationRunner();
            
            // シチュエーションAの依存的な階層構造構築
            // SituationA
            //   SituationA1
            //   SituationA2
            var situationA = new SampleSituationA();
            situationA.SetParent(_situationRunner);
            var situationA1 = new SampleNodeSituationA1();
            situationA1.SetParent(situationA);
            var situationA2 = new SampleNodeSituationA2();
            situationA2.SetParent(situationA);
            
            // PreLoad
            situationA.PreLoadAsync();
            situationA1.PreLoadAsync();
            situationA2.PreLoadAsync();
            
            // シチュエーションBの依存的な階層構造構築
            // SituationB
            //   SituationB1
            //   SituationB2
            //     SituationB21
            //     SituationB22
            var situationB = new SampleSituationB();
            situationB.SetParent(_situationRunner);
            var situationB1 = new SampleNodeSituationB1();
            situationB1.SetParent(situationB);
            var situationB2 = new SampleSituationB2();
            situationB2.SetParent(situationB);
            var situationB21 = new SampleNodeSituationB21();
            situationB21.SetParent(situationB2);
            var situationB22 = new SampleNodeSituationB22();
            situationB22.SetParent(situationB2);
            
            // シチュエーションの遷移関係を構築
            _situationFlow = new SituationFlow();
            var a1Node = _situationFlow.ConnectRoot(situationA1);
            var a2Node = a1Node.Connect(situationA2); // A1 -> A2
            var b1Node = a2Node.Connect(situationB1); // A2 -> B1
            var b21Node = b1Node.Connect(situationB21); // B1 -> B21
            var b22Node = b1Node.Connect(situationB22); // B1 -> B22
            
            // Fallback
            _situationFlow.SetFallbackNode(a2Node);
            _situationFlow.SetFallbackNode(b1Node);
            _situationFlow.SetFallbackNode(b22Node, a2Node);
            
            // 遷移
            _situationFlow.Transition(b21Node);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            _situationFlow.Dispose();
            _situationRunner.Dispose();
            Services.Instance.Remove(GetType());
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            _situationFlow.Update();
            _situationRunner.Update();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            _situationRunner.LateUpdate();
        }
    }
}