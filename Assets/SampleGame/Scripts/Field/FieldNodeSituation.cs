using System;
using GameFramework.Core;
using GameFramework.SituationSystems;
namespace SampleGame {
    /// <summary>
    /// Field用のNodeSituation
    /// </summary>
    public abstract class FieldNodeSituation : Situation, INodeSituation {
        private SituationTree _situationTree;

        /// <summary>
        /// Tree登録通知
        /// </summary>
        void INodeSituation.OnRegisterTree(SituationTree tree) {
            _situationTree = tree;
        }

        /// <summary>
        /// Tree登録解除通知
        /// </summary>
        void INodeSituation.OnUnregisterTree(SituationTree tree) {
            if (tree == _situationTree) {
                _situationTree = null;
            }
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        protected IProcess Transition<T>(Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where T : Situation {
            if (_situationTree == null) {
                return new TransitionHandle(new Exception("Situation tree is null."));
            }
            
            return _situationTree.Transition<T>(onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        protected IProcess Back(ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            if (_situationTree == null) {
                return new TransitionHandle(new Exception("Situation tree is null."));
            }
            
            return _situationTree.Back(overrideTransition, effects);
        }
    }
}