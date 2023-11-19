using System;
using GameFramework.Core;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// SituationFlow管理可能なNodeSituation
    /// </summary>
    public abstract class NodeSituation : Situation, INodeSituation {
        private SituationFlow _situationFlow;

        /// <summary>
        /// Tree登録通知
        /// </summary>
        void INodeSituation.OnRegisterTree(SituationFlow flow) {
            _situationFlow = flow;
        }

        /// <summary>
        /// Tree登録解除通知
        /// </summary>
        void INodeSituation.OnUnregisterTree(SituationFlow flow) {
            if (flow == _situationFlow) {
                _situationFlow = null;
            }
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        protected IProcess Transition<T>(Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where T : Situation {
            if (_situationFlow == null) {
                return new TransitionHandle(new Exception("Situation tree is null."));
            }
            
            return _situationFlow.Transition<T>(onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        protected IProcess Back(ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            if (_situationFlow == null) {
                return new TransitionHandle(new Exception("Situation tree is null."));
            }
            
            return _situationFlow.Back(overrideTransition, effects);
        }

        /// <summary>
        /// 遷移可能かチェック
        /// </summary>
        /// <param name="includeFallback">Fallback対象の型を含めるか</param>
        /// <typeparam name="T">チェックする型</typeparam>
        /// <returns>遷移可能か</returns>
        protected bool CheckTransition<T>(bool includeFallback = true)
            where T : Situation {
            return _situationFlow.CheckTransition<T>(includeFallback);
        }
    }
}