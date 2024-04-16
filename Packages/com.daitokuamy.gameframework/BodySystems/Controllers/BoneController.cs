using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Bodyの骨制御クラス
    /// </summary>
    public class BoneController : SerializedBodyController {
        /// <summary>
        /// Job本体
        /// </summary>
        [BurstCompile]
        public struct AnimationJob : IAnimationJob {
            [ReadOnly]
            public NativeArray<TransformStreamHandle> sourceTransformHandles;
            [ReadOnly]
            public NativeArray<MeshParts.ConstraintMasks> constraintMasksList;
            [ReadOnly]
            public NativeArray<TransformStreamHandle> destinationTransformHandles;

            /// <summary>
            /// RootMotion更新用
            /// </summary>
            void IAnimationJob.ProcessRootMotion(AnimationStream stream) {
            }

            /// <summary>
            /// 通常のBone更新用
            /// </summary>
            void IAnimationJob.ProcessAnimation(AnimationStream stream) {
                for (var i = 0; i < sourceTransformHandles.Length; i++) {
                    var masks = constraintMasksList[i];
                    if ((masks & MeshParts.ConstraintMasks.Position) != 0) {
                        var position = sourceTransformHandles[i].GetLocalPosition(stream);
                        destinationTransformHandles[i].SetLocalPosition(stream, position);
                    }

                    if ((masks & MeshParts.ConstraintMasks.Rotation) != 0) {
                        var rotation = sourceTransformHandles[i].GetLocalRotation(stream);
                        destinationTransformHandles[i].SetLocalRotation(stream, rotation);
                    }

                    if ((masks & MeshParts.ConstraintMasks.LocalScale) != 0) {
                        var localScale = sourceTransformHandles[i].GetLocalScale(stream);
                        destinationTransformHandles[i].SetLocalScale(stream, localScale);
                    }
                }
            }
        }

        [SerializeField, Tooltip("骨検索のためのルートとなるTransform")]
        private Transform _boneRoot = default;
        [SerializeField, Tooltip("AnimationJob登録時の実行オーダー")]
        private ushort _jobSortingOrder = 1500;

        // Animator
        private Animator _animator;
        // Mesh制御用クラス
        private MeshController _meshController;

        // Constraint用Graph情報
        private PlayableGraph _graph;
        private AnimationScriptPlayable _playable;

        // パラメータを渡すための配列
        private NativeArray<TransformStreamHandle> _sourceTransformHandles;
        private NativeArray<MeshParts.ConstraintMasks> _constraintMasksList;
        private NativeArray<TransformStreamHandle> _destinationTransformHandles;
        
        // 骨の参照カウンタ保持用
        private readonly Dictionary<GameObject, Transform[]> _mergedBones = new();
        private readonly Dictionary<Transform, int> _boneReferenceCounts = new();

        // BoneのRootTransform
        public Transform BoneRoot => _boneRoot;

        // 実行優先度
        public override int ExecutionOrder => 15;
        
        /// <summary>
        /// 骨のマージ
        /// </summary>
        /// <param name="target">対象のGameObject</param>
        /// <param name="prefix">マージする際につけるPrefix</param>
        public void MergeBones(GameObject target, string prefix) {
            if (BoneRoot == null) {
                return;
            }

            if (_mergedBones.ContainsKey(target)) {
                Debug.LogWarning($"Already merged bone target. [{target.name}]");
                return;
            }

            // メッシュパーツ情報
            var meshParts = target.GetComponent<MeshParts>();

            // 対象の骨のRootを取得
            var root = meshParts != null ? meshParts.boneRoot : null;
            if (root == null) {
                root = FindTransform(target.transform, BoneRoot.name);
            }

            if (root == null) {
                return;
            }

            // 既に存在する骨の列挙
            var currentBones = BoneRoot.GetComponentsInChildren<Transform>()
                .ToDictionary(x => x.name, x => x);

            // 対象の情報取得
            var rootName = root.name;
            var renderers = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            // Mergeできない
            if (root == null || renderers.Length <= 0) {
                return;
            }

            var bones = root.GetComponentsInChildren<Transform>(true);

            // ユニーク骨を他のMerge対象と被らせないようにするために置き換え
            if (meshParts != null) {
                ConvertUniqueBoneName(meshParts.uniqueBoneInfos.SelectMany(x => x.targetBones).ToArray(),
                    prefix);
            }

            // 関節を列挙
            var addableBones = new List<Transform>();
            var deletableBones = new List<Transform>();
            var addedBones = new List<Transform>();

            // 既存の骨に含まれている物とそうでないものを分離
            for (var i = bones.Length - 1; i >= 0; i--) {
                var bone = bones[i];

                if (bone == null) {
                    continue;
                }

                if (currentBones.TryGetValue(bone.name, out var currentBone)) {
                    deletableBones.Add(bone);

                    // 別で追加された骨の場合は覚えておく(参照カウンタUP用)
                    if (_boneReferenceCounts.ContainsKey(currentBone)) {
                        addedBones.Add(currentBone);
                    }
                }
                else {
                    addableBones.Add(bone);
                }
            }

            // 骨の追加
            foreach (var addBone in addableBones) {
                var parentName = addBone.parent.name;

                // Root骨直下だった場合、自身の名前をベースに親を探す
                if (parentName == rootName) {
                    var originalBone = GetOriginalBone(addBone);
                    if (originalBone != null) {
                        addBone.SetParent(originalBone.parent, false);
                    }
                }
                // 通常時は親の名前をベースに探す
                else {
                    if (currentBones.TryGetValue(addBone.parent.name, out var parentBone)) {
                        addBone.SetParent(parentBone, false);
                    }
                }
            }

            // Rendererの中身を入れ替える
            var targetBones = new List<Transform>();

            // 対応するCurrentBoneの取得
            Transform GetCurrentBone(Transform targetBone) {
                if (targetBone == null) {
                    return null;
                }

                // Root骨だった場合、直下の骨名の親を置き換え対象にする
                if (targetBone.name == rootName) {
                    if (targetBone.childCount > 0) {
                        var childBone = targetBone.GetChild(0);
                        var bone = GetCurrentBone(childBone);

                        if (bone != null && bone.parent != null) {
                            return bone;
                        }
                    }
                }

                // 既存骨リストに追加されている場合
                if (currentBones.TryGetValue(targetBone.name, out var currentBone)) {
                    return currentBone;
                }

                // 今回追加した骨の場合
                if (addableBones.Contains(targetBone)) {
                    return targetBone;
                }

                return null;
            }

            foreach (var skinnedMeshRenderer in renderers) {
                // メッシュのデフォーマーになっているTransform
                targetBones.Clear();

                foreach (var targetBone in skinnedMeshRenderer.bones) {
                    var bone = GetCurrentBone(targetBone);

                    // 差し替え対象の骨リストとして列挙し直す
                    targetBones.Add(bone);
                }

                // 骨の参照を差し替える
                skinnedMeshRenderer.bones = targetBones.ToArray();

                // Root骨を差し替える
                var rootBone = GetCurrentBone(skinnedMeshRenderer.rootBone);

                if (rootBone != null) {
                    skinnedMeshRenderer.rootBone = rootBone;
                }
                else {
                    skinnedMeshRenderer.rootBone = BoneRoot;
                }
            }

            // 不要なTransformを削除する
            foreach (var deleteBone in deletableBones) {
                DestroyImmediate(deleteBone.gameObject);
            }

            var additiveBones = addableBones.Concat(addedBones).ToArray();

            // 骨参照カウントの更新
            foreach (var bone in additiveBones) {
                _boneReferenceCounts.TryGetValue(bone, out var referenceCount);
                referenceCount++;
                _boneReferenceCounts[bone] = referenceCount;
            }

            // 追加した骨を記憶
            _mergedBones[target] = additiveBones;
        }

        /// <summary>
        /// 骨の削除
        /// </summary>
        public void DeleteBones(GameObject target) {
            if (!_mergedBones.TryGetValue(target, out var additiveBones)) {
                return;
            }

            // 追加した骨を削除
            foreach (var additiveBone in additiveBones) {
                // 参照カウンタが無くなったら削除する
                _boneReferenceCounts.TryGetValue(additiveBone, out var referenceCount);

                if (referenceCount <= 1) {
                    _boneReferenceCounts.Remove(additiveBone);
                    if (additiveBone != null) {
                        DestroyImmediate(additiveBone.gameObject);
                    }
                }
                else {
                    _boneReferenceCounts[additiveBone] = referenceCount - 1;
                }
            }

            // 追加骨情報を削除
            _mergedBones.Remove(target);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            _animator = Body.GetComponent<Animator>();
            _meshController = Body.GetController<MeshController>();

            if (_meshController != null) {
                // Mesh更新時にGraphを再構築
                _meshController.OnRefreshed += BuildGraph;
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            ClearGraph();
        }

        /// <summary>
        /// グラフの構築
        /// </summary>
        private void BuildGraph() {
            ClearGraph();

            if (_meshController == null) {
                return;
            }

            // 追従対象の骨を列挙
            var partsList = _meshController.GetMeshPartsList();
            var constraintInfos = new List<(Transform, Transform, MeshParts.ConstraintMasks)>();
            foreach (var parts in partsList) {
                foreach (var info in parts.uniqueBoneInfos) {
                    if (info.constraintMask == 0) {
                        continue;
                    }

                    foreach (var bone in info.targetBones) {
                        if (bone == null) {
                            continue;
                        }

                        var originalBone = GetOriginalBone(bone);
                        if (originalBone == null) {
                            continue;
                        }

                        constraintInfos.Add((originalBone, bone, info.constraintMask));
                    }
                }
            }

            if (constraintInfos.Count <= 0) {
                return;
            }

            // Graph/Outputの生成
            _graph = PlayableGraph.Create($"{nameof(BoneController)}({Body.Transform.name})");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(_graph, "Output", _animator);
            output.SetAnimationStreamSource(AnimationStreamSource.PreviousInputs);
            output.SetSortingOrder(_jobSortingOrder);

            // バッファの構築
            _sourceTransformHandles = new NativeArray<TransformStreamHandle>(constraintInfos.Count, Allocator.Persistent);
            _destinationTransformHandles = new NativeArray<TransformStreamHandle>(constraintInfos.Count, Allocator.Persistent);
            _constraintMasksList = new NativeArray<MeshParts.ConstraintMasks>(constraintInfos.Count, Allocator.Persistent);
            for (var i = 0; i < constraintInfos.Count; i++) {
                _sourceTransformHandles[i] = _animator.BindStreamTransform(constraintInfos[i].Item1);
                _destinationTransformHandles[i] = _animator.BindStreamTransform(constraintInfos[i].Item2);
                _constraintMasksList[i] = constraintInfos[i].Item3;
            }

            // Playableの生成
            var job = new AnimationJob {
                sourceTransformHandles = _sourceTransformHandles,
                destinationTransformHandles = _destinationTransformHandles,
                constraintMasksList = _constraintMasksList
            };

            _playable = AnimationScriptPlayable.Create(_graph, job);
            output.SetSourcePlayable(_playable);
            _graph.Play();
        }

        /// <summary>
        /// グラフの削除
        /// </summary>
        private void ClearGraph() {
            if (_sourceTransformHandles.IsCreated) {
                _sourceTransformHandles.Dispose();
            }

            if (_destinationTransformHandles.IsCreated) {
                _destinationTransformHandles.Dispose();
            }

            if (_constraintMasksList.IsCreated) {
                _constraintMasksList.Dispose();
            }

            if (_playable.IsValid()) {
                _playable.Destroy();
            }

            if (_graph.IsValid()) {
                _graph.Destroy();
            }
        }

        /// <summary>
        /// ユニークな骨名に変換する
        /// </summary>
        private void ConvertUniqueBoneName(Transform[] bones, string prefix) {
            foreach (var target in bones) {
                target.name = $"{prefix}-{target.name}";
            }
        }

        /// <summary>
        /// 再帰的にTransformを探す
        /// </summary>
        private Transform FindTransform(Transform parent, string targetName) {
            if (parent.name == targetName) {
                return parent;
            }

            for (var i = 0; i < parent.childCount; i++) {
                var found = FindTransform(parent.GetChild(i), targetName);
                if (found != null) {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// 骨のオリジナルを取得
        /// </summary>
        private Transform GetOriginalBone(Transform bone) {
            if (BoneRoot == null) {
                return null;
            }

            if (bone == null) {
                return null;
            }

            // 対象の骨を探す
            var boneName = bone.name;
            var names = boneName.Split('-');
            var originalBoneName = string.Join("-", names, Mathf.Min(1, names.Length - 1), names.Length - 1);
            return FindTransform(BoneRoot, originalBoneName);
        }
    }
}