using UnityEngine;
using UnityEngine.AI;

namespace ThirdPersonEngine {
    /// <summary>
    /// NavMeshAgentを使ったActorNavigator
    /// </summary>
    public class NavMeshActorNavigator : IActorNavigator {
        private readonly NavMeshAgent _agent;
        private readonly Vector3 _goalPoint;

        /// <summary>有効か</summary>
        bool IActorNavigator.IsValid => _agent.pathStatus != NavMeshPathStatus.PathInvalid;
        /// <summary>移動先座標</summary>
        Vector3 IActorNavigator.SteeringTarget => _agent.steeringTarget;
        /// <summary>目的地</summary>
        Vector3 IActorNavigator.GoalPoint => _agent.destination;
        /// <summary>目的地までの残り距離</summary>
        float IActorNavigator.RemainingDistance => _agent.hasPath ? _agent.remainingDistance : float.MaxValue;

        /// <summary>タイムアウト時間</summary>
        public float TimeOutDuration { get; set; } = -1.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="agent">ナビゲーションに使うAgent</param>
        /// <param name="goalPoint">目的座標</param>
        public NavMeshActorNavigator(NavMeshAgent agent, Vector3 goalPoint) {
            _agent = agent;
            _goalPoint = goalPoint;
        }

        /// <summary>
        /// 移動開始処理
        /// </summary>
        /// <param name="currentPosition">現在座標</param>
        /// <param name="speed">移動速度</param>
        void IActorNavigator.Start(Vector3 currentPosition, float speed) {
            _agent.Warp(currentPosition);
            _agent.speed = speed;
            _agent.SetDestination(_goalPoint);
        }

        /// <summary>
        /// 移動先座標の設定
        /// </summary>
        /// <param name="position">移動先座標</param>
        void IActorNavigator.SetNextPosition(Vector3 position) {
            _agent.nextPosition = position;
        }
    }
}
