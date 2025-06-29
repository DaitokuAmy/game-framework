using System.Collections.Generic;
using System.Linq;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using R3;
using UnityEngine;
using UniRx;

namespace SituationFlowSample {
    /// <summary>
    /// SituationFlow用のサンプル
    /// </summary>
    public class SituationFlowSample : MonoBehaviour {
        [SerializeField, Tooltip("遷移先選択用メニュー")]
        private TransitionMenuView _transitionMenuView;

        private DisposableScope _scope;
        private SituationContainer _situationContainer;
        private SituationFlow _situationFlow;
        private List<ISampleSituation> _nodeSituations = new();

        /// <summary>遷移先選択メニューView</summary>
        public TransitionMenuView MenuView => _transitionMenuView;
        /// <summary>遷移に使うFlow</summary>
        public SituationFlow Flow => _situationFlow;

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Start() {
            DontDestroyOnLoad(gameObject);

            Services.Instance.RegisterInstance(this);

            var situationRoot = new SampleSituationRoot();

            // シチュエーションAの依存的な階層構造構築
            // SituationA
            //   SituationA1
            //   SituationA2
            var situationA = new SampleSituationA();
            situationA.SetParent(situationRoot);
            var situationA1 = new SampleSituationA1();
            situationA1.SetParent(situationA);
            var situationA2 = new SampleSituationA2();
            situationA2.SetParent(situationA);

            // シチュエーションBの依存的な階層構造構築
            // SituationB
            //   SituationB1
            //   SituationB2
            //     SituationB21
            //     SituationB22
            //   SituationB3
            var situationB = new SampleSituationB();
            situationB.SetParent(situationRoot);
            var situationB1 = new SampleSituationB1();
            situationB1.SetParent(situationB);
            var situationB2 = new SampleSituationB2();
            situationB2.SetParent(situationB);
            var situationB21 = new SampleSituationB21();
            situationB21.SetParent(situationB2);
            var situationB22 = new SampleSituationB22();
            situationB22.SetParent(situationB2);
            var situationB3 = new SampleSceneSituationB3();
            situationB3.SetParent(situationB);
            
            // シチュエーションCの依存的な階層構造構築
            // SituationC
            //   SituationC1
            var situationC = new SampleSceneSituationC();
            situationC.SetParent(situationRoot);
            var situationC1 = new SampleSituationC1();
            situationC1.SetParent(situationC);
            
            // PreLoad
            situationA.PreLoadAsync();
            situationA1.PreLoadAsync();
            situationA2.PreLoadAsync();

            // コンテナの初期化
            _situationContainer.Setup(situationRoot);

            // シチュエーションの遷移関係を構築
            _situationFlow = new SituationFlow(_situationContainer);
            var aNode = _situationFlow.ConnectRoot<SampleSituationA>();
            var aA1Node = aNode.Connect<SampleSituationA1>(); // A -> A1
            var a1A2Node = aA1Node.Connect<SampleSituationA2>(); // A1 -> A2
            var a2B1Node = a1A2Node.Connect<SampleSituationB1>(); // A2 -> B1
            var b1B21Node = a2B1Node.Connect<SampleSituationB21>(); // B1 -> B21
            var b1B22Node = a2B1Node.Connect<SampleSituationB22>(); // B1 -> B22
            var b22CNode = b1B22Node.Connect<SampleSceneSituationC>(); // B22 -> C
            var cC1Node = b22CNode.Connect<SampleSituationC1>(); // C -> C1
            var b22C1Node = b1B22Node.Connect<SampleSituationC1>(); // B22 -> C1
            var c1B3Node = b22C1Node.Connect<SampleSceneSituationB3>(); // C1 -> B3
            var c1A2Node = b22C1Node.Connect<SampleSituationA2>(); // C1 -> A2

            // Fallback
            _situationFlow.SetFallbackNode(a1A2Node);
            _situationFlow.SetFallbackNode(a2B1Node);
            _situationFlow.SetFallbackNode(b1B22Node, a1A2Node);

            // Viewの初期化
            _nodeSituations.Clear();
            var nodes = _situationFlow.GetNodes();
            var nodeSituations = nodes.Select(x => x.Situation).Distinct();
            _nodeSituations.AddRange(nodeSituations.Cast<ISampleSituation>());
            _transitionMenuView.SetupItems(-1, _nodeSituations.Select(x => x.DisplayName).ToArray());

            // 遷移
            _situationFlow.Transition(b1B21Node);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            _scope = new DisposableScope();

            _situationContainer = new SituationContainer();
            _situationContainer.ChangedCurrentAsObservable()
                .TakeUntil(_scope)
                .Subscribe(situation => {
                    if (situation is ISampleSituation nodeSituation) {
                        var index = _nodeSituations.IndexOf(nodeSituation);
                        _transitionMenuView.SelectItem(index);
                        _transitionMenuView.Title = nodeSituation.DisplayName;
                    }
                });

            MenuView.BackSubject
                .TakeUntil(_scope)
                .Subscribe(_ => Flow.Back());

            MenuView.SelectedSubject
                .TakeUntil(_scope)
                .Subscribe(index => {
                    if (index >= 0 && index < _nodeSituations.Count) {
                        var nodeSituation = _nodeSituations[index];
                        Flow.Transition(nodeSituation.GetType());
                    }
                });
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            _scope.Dispose();
            _scope = null;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            _situationFlow.Dispose();
            _situationContainer.Dispose();
            Services.Instance.Remove(GetType());
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if (Input.GetKeyDown(KeyCode.R)) {
                _situationFlow.Reset();
            }

            _situationContainer.Update();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            _situationContainer.LateUpdate();
        }

        /// <summary>
        /// 固定更新処理
        /// </summary>
        private void FixedUpdate() {
            _situationContainer.FixedUpdate();
        }
    }
}