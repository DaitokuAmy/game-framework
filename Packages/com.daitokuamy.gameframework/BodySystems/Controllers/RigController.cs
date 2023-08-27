#if USE_ANIMATION_RIGGING
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace GameFramework.BodySystems {
    /// <summary>
    /// BodyのRig制御クラス
    /// </summary>
    public class RigController : BodyController {
        // リグ構築用
        private RigBuilder _rigBuilder;
        // RigParts情報
        private Dictionary<string, RigParts> _rigParts = new();
        // RigのRebuildリクエスト
        private bool _rigDirty;

        // リグ名一覧
        public string[] RigNames => _rigParts.Keys.ToArray();

        // 実行優先度
        public override int ExecutionOrder => 16;

        /// <summary>
        /// RigのWeight設定
        /// </summary>
        /// <param name="rigName">リグ名</param>
        /// <param name="weight">重み</param>
        public void SetRigWeight(string rigName, float weight) {
            weight = Mathf.Clamp01(weight);
            if (_rigParts.TryGetValue(rigName, out var parts)) {
                if (parts.Rig != null) {
                    parts.Rig.weight = weight;
                }
            }
        }

        /// <summary>
        /// RigのWeight取得
        /// </summary>
        /// <param name="rigName">リグ名</param>
        public float GetRigWeight(string rigName) {
            if (_rigParts.TryGetValue(rigName, out var parts)) {
                if (parts.Rig != null) {
                    return parts.Rig.weight;
                }
            }

            return 0.0f;
        }

        /// <summary>
        /// 制御用RigPartsの取得
        /// </summary>
        /// <param name="rigName">リグ名</param>
        public TRigParts GetRigParts<TRigParts>(string rigName)
            where TRigParts : RigParts {
            if (_rigParts.TryGetValue(rigName, out var parts)) {
                return parts as TRigParts;
            }

            return default;
        }

        /// <summary>
        /// ConstraintDataの取得
        /// </summary>
        /// <param name="rigName">リグ名</param>
        /// <param name="constraintName">Constraint名</param>
        public TData GetConstraintData<TData>(string rigName, string constraintName)
            where TData : IAnimationJobData {
            if (!_rigParts.TryGetValue(rigName, out var parts)) {
                return default;
            }

            var constraint = parts.Constraints.FirstOrDefault(x => x.component.name == constraintName);
            if (constraint == null) {
                return default;
            }

            return (TData)constraint.data;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            _rigBuilder = Body.GetComponent<RigBuilder>();
            
            var meshController = Body.GetController<MeshController>();
            // Mesh更新時にRigビルド予約
            meshController.OnRefreshed += () => _rigDirty = true;

            _rigDirty = true;
        }

        /// <summary>
        /// 更新時処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            if (_rigDirty) {
                _rigDirty = false;
                
                RefreshRigParts();
                
                // RigのRebuild
                _rigBuilder.Build();
            }
        }

        /// <summary>
        /// RigPartsの取得
        /// </summary>
        private void RefreshRigParts() {
            _rigParts = Body.GetComponentsInChildren<RigParts>(true)
                .ToDictionary(x => x.name, x => x);
        }
    }
}
#endif