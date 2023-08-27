using System;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// アクターエイム用クラス
    /// </summary>
    public sealed class ActorAimController : IDisposable {
        private IAimableActor _actor;
        private IAimTarget _aimTarget;

        /// <summary>エイミング対象のターゲット</summary>
        public IAimTarget Target {
            get => _aimTarget;
            set {
                if (_aimTarget == value) {
                    return;
                }

                _aimTarget = value;
            }
        }
        /// <summary>有効状態か</summary>
        public bool IsActive { get; set; }
        /// <summary>エイミング中か</summary>
        public bool IsAiming => Target != null && IsActive;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="actor">エイム制御するアクター</param>
        public ActorAimController(IAimableActor actor) {
            _actor = actor;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _actor = null;
            _aimTarget = null;
        }

        /// <summary>
        /// 現在の移動をスキップ
        /// </summary>
        public void Skip() {
            if (!IsAiming) {
                return;
            }
            
            _actor.Aim(_aimTarget.GetPosition());
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            if (!IsAiming) {
                return;
            }
            
            // エイム処理
            _actor.Aim(Target.GetPosition());
        }
    }
}