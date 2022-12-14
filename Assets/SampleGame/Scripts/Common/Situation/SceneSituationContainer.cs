using GameFramework.SituationSystems;
using GameFramework.TaskSystems;

namespace SampleGame {
    /// <summary>
    /// シーン遷移を行えるシチュエーションコンテナ
    /// </summary>
    public class SceneSituationContainer : SituationContainer, ILateUpdatableTask {
        // アクティブか
        bool ITask.IsActive => true;

        /// <summary>
        /// タスク更新
        /// </summary>
        void ITask.Update() {
            // Rootシーン更新
            Update();
        }

        /// <summary>
        /// タスク後更新
        /// </summary>
        void ILateUpdatableTask.LateUpdate() {
            // Rootシーン更新
            LateUpdate();
        }

        /// <summary>
        /// 遷移用のTransition取得
        /// </summary>
        protected override ITransition GetDefaultTransition() {
            return new OutInTransition();
        }

        /// <summary>
        /// 遷移を行えるか
        /// </summary>
        protected override bool CheckTransitionInternal(Situation next, ITransition transition) {
            return next is SceneSituation && transition is OutInTransition;
        }
    }
}