using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// 腰の高さを調整するためのAnimationJob用コンポーネント
    /// </summary>
    public sealed class AdjustHeightAnimationJobComponent : AnimationJobComponent {
        /// <summary>
        /// Job本体
        /// </summary>
        [BurstCompile]
        public struct AnimationJob : IAnimationJob {
            [ReadOnly]
            public NativeArray<float> Properties;
            [ReadOnly]
            public TransformStreamHandle RootHandle;
            
            public TransformStreamHandle HipsHandle;

            /// <summary>
            /// RootMotion更新用
            /// </summary>
            void IAnimationJob.ProcessRootMotion(AnimationStream stream) {
            }

            /// <summary>
            /// 通常のBone更新用
            /// </summary>
            void IAnimationJob.ProcessAnimation(AnimationStream stream) {
                var rootPosition = RootHandle.GetPosition(stream);
                var hipsPosition = HipsHandle.GetPosition(stream);
                var directionY = (hipsPosition - rootPosition).y;
                directionY = directionY * Properties[0] - directionY;
                hipsPosition.y += directionY;
                HipsHandle.SetPosition(stream, hipsPosition);
            }
        }

        private NativeArray<float> _properties;
        private Transform _root;
        private Transform _hips;

        /// <summary>高さのスケール</summary>
        public float HeightScale {
            get => _properties.IsCreated ? _properties[0] : 1.0f;
            set {
                if (_properties.IsCreated) {
                    _properties[0] = value;
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AdjustHeightAnimationJobComponent(Transform root, Transform hips) {
            _root = root;
            _hips = hips;
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected override AnimationScriptPlayable CreatePlayable(Animator animator, PlayableGraph graph) {
            if (_root == null || _hips == null) {
                return default;
            }
            
            _properties = new NativeArray<float>(1, Allocator.Persistent);
            _properties[0] = 1.0f;
            
            var rootHandle = animator.BindStreamTransform(_root);
            var hipsHandle = animator.BindStreamTransform(_hips);

            var job = new AnimationJob {
                Properties = _properties,
                RootHandle = rootHandle,
                HipsHandle = hipsHandle
            };

            return AnimationScriptPlayable.Create(graph, job);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="playable">Jobを保持しているPlayable</param>
        /// <param name="deltaTime">変位時間</param>
        protected override void UpdateInternal(AnimationScriptPlayable playable, float deltaTime) {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _properties.Dispose();
        }
    }
}