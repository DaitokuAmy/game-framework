using System;
using System.Linq;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform加工用のResolver
    /// </summary>
    public abstract class AttachmentResolver {
        // ターゲット情報
        [Serializable]
        public class TargetSource {
            [Tooltip("追従先")]
            public Transform target;
            [Tooltip("影響率")]
            public float weight = 1.0f;
        }

        // ターゲット情報
        private class TargetInfo {
            // ターゲットの元情報
            public TargetSource source = new TargetSource();
            // 参照するTarget
            public Transform target;
            // 正規化済みのWeight
            public float normalizedWeight;
        }

        // ターゲット情報のリスト
        private TargetSource[] _sources = Array.Empty<TargetSource>();
        // ターゲット情報リスト
        private TargetInfo[] _targetInfos = Array.Empty<TargetInfo>();
        // Weightが正規化済みか
        private bool _normalized = false;

        // 制御対象
        public Transform Owner { get; private set; }
        // ターゲットリスト
        public TargetSource[] Sources {
            set {
                _sources = value;
                _targetInfos = value.Select(x => new TargetInfo {
                        source = x,
                        target = x.target
                    })
                    .ToArray();
                _normalized = false;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner">制御対象のTransform</param>
        public AttachmentResolver(Transform owner) {
            Owner = owner;
        }

        /// <summary>
        /// Transformの反映
        /// </summary>
        public abstract void Resolve();

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public abstract void ResetOffset();

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public abstract void TransferOffset();

        /// <summary>
        /// ターゲット座標
        /// </summary>
        protected Vector3 GetTargetPosition() {
            if (!(_normalized && Application.isPlaying)) {
                NormalizeWeights();
            }

            if (!_normalized) {
                return Vector3.zero;
            }

            var position = Vector3.zero;

            foreach (var info in _targetInfos) {
                if (info.target == null) {
                    continue;
                }

                position += info.target.position * info.normalizedWeight;
            }

            return position;
        }

        /// <summary>
        /// ターゲット姿勢
        /// </summary>
        protected Quaternion GetTargetRotation() {
            if (!(_normalized && Application.isPlaying)) {
                NormalizeWeights();
            }

            if (!_normalized) {
                return Quaternion.identity;
            }

            var rotation = Quaternion.identity;

            foreach (var info in _targetInfos) {
                if (info.target == null) {
                    continue;
                }

                rotation *= Quaternion.Slerp(Quaternion.identity, info.target.rotation,
                    info.normalizedWeight);
            }

            return rotation;
        }

        /// <summary>
        /// ターゲットローカルスケール
        /// </summary>
        protected Vector3 GetTargetLocalScale() {
            if (!(_normalized && Application.isPlaying)) {
                NormalizeWeights();
            }

            if (!_normalized) {
                return Vector3.one;
            }

            var scale = Vector3.zero;

            foreach (var info in _targetInfos) {
                if (info.target == null) {
                    continue;
                }

                scale += info.target.localScale * info.normalizedWeight;
            }

            return scale;
        }

        /// <summary>
        /// ターゲットのWeightを合計で1になるように正規化
        /// </summary>
        private void NormalizeWeights() {
            var totalWeight = _targetInfos.Where(x => x.target != null).Sum(x => x.source.weight);

            if (totalWeight <= float.Epsilon) {
                return;
            }

            foreach (var info in _targetInfos) {
                info.normalizedWeight = info.source.weight / totalWeight;
            }

            _normalized = true;
        }
    }
}