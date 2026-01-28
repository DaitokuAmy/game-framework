#if USE_ANIMATION_RIGGING
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// RigLayer制御用のパーツ
    /// </summary>
    public abstract class RigParts : MonoBehaviour {
        private bool _initialized = false;
        private Rig _rig;
        private IRigConstraint[] _constraints;

        // 制御対象のRig
        public Rig Rig {
            get {
                Initialize();
                return _rig;
            }
        }
        // 制御対象のConstraintリスト
        public IReadOnlyList<IRigConstraint> Constraints {
            get {
                Initialize();
                return _constraints;
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;
            _rig = GetComponent<Rig>();
            _constraints = GetComponentsInChildren<IRigConstraint>(true);
        }
    }
}
#endif