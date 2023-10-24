using System;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// ParticleSystemを使ったStateGimmick
    /// </summary>
    public class ParticleSystemStateGimmick : StateGimmickBase<ParticleSystemStateGimmick.StateInfo> {
        /// <summary>
        /// ステート情報基底
        /// </summary>
        [Serializable]
        public class StateInfo : StateInfoBase {
            [Tooltip("再生するParticleSystem")]
            public ParticleSystem[] activeParticleSystems;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();
            
            // 全パーティクルの停止
            foreach (var info in StateInfos) {
                foreach (var ps in info.activeParticleSystems) {
                    if (ps == null) {
                        continue;
                    }
                    
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        /// <summary>
        /// ステートの変更処理
        /// </summary>
        /// <param name="prev">変更前のステート</param>
        /// <param name="current">変更後のステート</param>
        /// <param name="immediate">即時遷移するか</param>
        protected override void ChangeState(StateInfo prev, StateInfo current, bool immediate) {
            if (prev != null) {
                foreach (var ps in prev.activeParticleSystems) {
                    if (ps == null) {
                        continue;
                    }
                    
                    ps.Stop(true, immediate ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
                }
            }
            
            if (current != null) {
                foreach (var ps in current.activeParticleSystems) {
                    if (ps == null) {
                        continue;
                    }
                    
                    ps.Play(true);
                }
            }
        }
    }
}