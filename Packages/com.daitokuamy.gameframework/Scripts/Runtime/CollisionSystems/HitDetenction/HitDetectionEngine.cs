using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// ヒット検出エンジン
    /// </summary>
    public sealed partial class HitDetectionEngine : IDisposable {
        /// <summary>
        /// ヒット形状種別
        /// </summary>
        internal enum HitShapeType : byte {
            Sphere = 0,
            Box = 1,
            Sweep = 2,
        }

        /// <summary>
        /// ヒットのJob計算用のSnapshot
        /// </summary>
        private struct HitSnapshot {
            public int Id;
            public int LayerMask;
            public HitShapeType ShapeType;
            public int CustomDataIndex;

            public float3 Center;
            public float Radius;
            public quaternion Rotation;
            public float3 HalfExtents;
            public float3 Start;
            public float3 End;
        }

        /// <summary>
        /// 受けのJob計算用のSnapshot
        /// </summary>
        private struct ReceiveSnapshot {
            public int Id;
            public float3 Start;
            public float3 End;
            public float Radius;
            public int LayerMask;
        }

        /// <summary>
        /// 候補ペア
        /// </summary>
        private struct CandidatePair {
            public int HitIndex;
            public int ReceiveIndex;
        }

        /// <summary>
        /// 当たり判定計算用Job
        /// </summary>
        [BurstCompile]
        private struct CheckHitJob : IJobParallelFor {
            [ReadOnly]
            public NativeArray<HitSnapshot> HitSnapshots;
            [ReadOnly]
            public NativeArray<ReceiveSnapshot> ReceiveSnapshots;
            [ReadOnly]
            public NativeArray<CandidatePair> CandidatePairs;

            public int PairCount;

            [WriteOnly]
            public NativeArray<int> HitFlags;
            [WriteOnly]
            public NativeArray<float3> ContactPoints;
            [WriteOnly]
            public NativeArray<float3> ContactNormals;

            /// <inheritdoc/>
            public void Execute(int index) {
                if (index >= PairCount) {
                    return;
                }

                var pair = CandidatePairs[index];
                var hitSnapshot = HitSnapshots[pair.HitIndex];
                var receiveSnapshot = ReceiveSnapshots[pair.ReceiveIndex];

                // レイヤー判定
                if ((hitSnapshot.LayerMask & receiveSnapshot.LayerMask) == 0) {
                    return;
                }

                // 判定
                HitFlags[index] = 0;
                ContactPoints[index] = default;
                ContactNormals[index] = default;

                switch (hitSnapshot.ShapeType) {
                    case HitShapeType.Sphere: {
                        if (TryGetReceiveContactPoint_Sphere(hitSnapshot, receiveSnapshot, out var contactPoint, out var contactNormal)) {
                            HitFlags[index] = 1;
                            ContactPoints[index] = contactPoint;
                            ContactNormals[index] = contactNormal;
                        }

                        break;
                    }
                    case HitShapeType.Box: {
                        if (TryGetReceiveContactPoint_Box(hitSnapshot, receiveSnapshot, out var contactPoint, out var contactNormal)) {
                            HitFlags[index] = 1;
                            ContactPoints[index] = contactPoint;
                            ContactNormals[index] = contactNormal;
                        }

                        break;
                    }
                    case HitShapeType.Sweep: {
                        if (TryGetReceiveContactPoint_Sweep(hitSnapshot, receiveSnapshot, out var contactPoint, out var contactNormal)) {
                            HitFlags[index] = 1;
                            ContactPoints[index] = contactPoint;
                            ContactNormals[index] = contactNormal;
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// ヒットコリジョン登録情報
        /// </summary>
        private sealed class HitEntry<TCollider> {
            public int Id;
            public TCollider Collider;
            public int LayerMask;
            public object CustomData;
        }

        /// <summary>
        /// 受けコリジョン登録情報
        /// </summary>
        private sealed class ReceiveEntry {
            public int Id;
            public IReceiveCollider Collider;
            public IHitListener Listener;
            public int LayerMask;
        }

        /// <summary>
        /// 衝突ペアキー
        /// </summary>
        private readonly struct PairKey : IEquatable<PairKey> {
            public readonly int HitId;
            public readonly int ReceiveId;

            public PairKey(int hitId, int receiveId) {
                HitId = hitId;
                ReceiveId = receiveId;
            }

            /// <inheritdoc/>
            public bool Equals(PairKey other) {
                return HitId == other.HitId && ReceiveId == other.ReceiveId;
            }

            /// <inheritdoc/>
            public override bool Equals(object obj) {
                return obj is PairKey other && Equals(other);
            }

            /// <inheritdoc/>
            public override int GetHashCode() {
                unchecked {
                    return (HitId * 397) ^ ReceiveId;
                }
            }
        }

        private readonly UniformGrid2D _hitGrid;
        private readonly List<HitEntry<ISphereHitCollider>> _sphereHitEntries = new();
        private readonly List<HitEntry<ISweepHitCollider>> _sweepHitEntries = new();
        private readonly List<HitEntry<IBoxHitCollider>> _boxHitEntries = new();
        private readonly List<ReceiveEntry> _receiveEntries = new();
        private readonly List<object> _customDataList = new();
        private readonly List<int> _tmpCandidates = new(256);

        private Dictionary<PairKey, object> _prevPairKeys = new();
        private Dictionary<PairKey, object> _currentPairKeys = new();

        private NativeArray<HitSnapshot> _hitSnapshots;
        private NativeArray<ReceiveSnapshot> _receiveSnapshots;
        private NativeArray<CandidatePair> _candidatePairs;
        private NativeArray<int> _hitFlags;
        private NativeArray<float3> _contactPoints;
        private NativeArray<float3> _contactNormals;

        private bool _disposed;
        private int _nextHitId = 1;
        private int _nextReceiveId = 1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cellSize">空間分割用のセルサイズ</param>
        public HitDetectionEngine(float cellSize) {
            _hitGrid = new UniformGrid2D(cellSize);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            // Debug情報の削除
            DisableDebugView();

            // NativeArrayの解放
            if (_hitSnapshots.IsCreated) {
                _hitSnapshots.Dispose();
            }

            if (_receiveSnapshots.IsCreated) {
                _receiveSnapshots.Dispose();
            }

            if (_candidatePairs.IsCreated) {
                _candidatePairs.Dispose();
            }

            if (_hitFlags.IsCreated) {
                _hitFlags.Dispose();
            }

            if (_contactPoints.IsCreated) {
                _contactPoints.Dispose();
            }

            if (_contactNormals.IsCreated) {
                _contactNormals.Dispose();
            }
            
            // ワーク領域クリア
            _prevPairKeys.Clear();
            _currentPairKeys.Clear();
            _customDataList.Clear();
            _tmpCandidates.Clear();

            // コリジョン情報をクリア
            _hitGrid.Clear();
            _sphereHitEntries.Clear();
            _boxHitEntries.Clear();
            _sweepHitEntries.Clear();
            _receiveEntries.Clear();
        }

        /// <summary>
        /// 球体ヒットコリジョン登録
        /// </summary>
        /// <param name="collider">ヒットコリジョン情報</param>
        /// <param name="layerMask">判定レイヤーマスク</param>
        /// <param name="customData">ヒット検出時に届くカスタムデータ</param>
        public int RegisterHit(ISphereHitCollider collider, int layerMask = ~0, object customData = null) {
            var id = _nextHitId++;
            _sphereHitEntries.Add(new HitEntry<ISphereHitCollider> { Id = id, Collider = collider, LayerMask = layerMask, CustomData = customData });
            return id;
        }

        /// <summary>
        /// ボックスヒットコリジョン登録
        /// </summary>
        /// <param name="collider">ヒットコリジョン情報</param>
        /// <param name="layerMask">判定レイヤーマスク</param>
        /// <param name="customData">ヒット検出時に届くカスタムデータ</param>
        public int RegisterHit(IBoxHitCollider collider, int layerMask = ~0, object customData = null) {
            var id = _nextHitId++;
            _boxHitEntries.Add(new HitEntry<IBoxHitCollider> { Id = id, Collider = collider, LayerMask = layerMask, CustomData = customData });
            return id;
        }

        /// <summary>
        /// スイープヒットコリジョン登録
        /// </summary>
        /// <param name="collider">ヒットコリジョン情報</param>
        /// <param name="layerMask">判定レイヤーマスク</param>
        /// <param name="customData">ヒット検出時に届くカスタムデータ</param>
        public int RegisterHit(ISweepHitCollider collider, int layerMask = ~0, object customData = null) {
            var id = _nextHitId++;
            _sweepHitEntries.Add(new HitEntry<ISweepHitCollider> { Id = id, Collider = collider, LayerMask = layerMask, CustomData = customData });
            return id;
        }

        /// <summary>
        /// 受けコリジョン登録
        /// </summary>
        /// <param name="collider">受けコリジョン情報</param>
        /// <param name="listener">判定検知用リスナー</param>
        /// <param name="layerMask">判定レイヤーマスク</param>
        public int RegisterReceive(IReceiveCollider collider, IHitListener listener, int layerMask = ~0) {
            var id = _nextReceiveId++;
            _receiveEntries.Add(new ReceiveEntry { Id = id, Collider = collider, Listener = listener, LayerMask = layerMask });
            return id;
        }

        /// <summary>
        /// ヒットコリジョン登録解除
        /// </summary>
        /// <param name="id">登録時のId</param>
        public void UnregisterHit(int id) {
            for (var i = _sphereHitEntries.Count - 1; i >= 0; i--) {
                if (_sphereHitEntries[i].Id == id) {
                    _sphereHitEntries.RemoveAt(i);
                    return;
                }
            }

            for (var i = _boxHitEntries.Count - 1; i >= 0; i--) {
                if (_boxHitEntries[i].Id == id) {
                    _boxHitEntries.RemoveAt(i);
                    return;
                }
            }

            for (var i = _sweepHitEntries.Count - 1; i >= 0; i--) {
                if (_sweepHitEntries[i].Id == id) {
                    _sweepHitEntries.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// 受けコリジョン登録解除
        /// </summary>
        /// <param name="id">登録時のId</param>
        public void UnregisterReceive(int id) {
            for (var i = _receiveEntries.Count - 1; i >= 0; i--) {
                if (_receiveEntries[i].Id == id) {
                    _receiveEntries.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            // スナップショット作成
            EnsureNativeArrays();

            var hitWrite = 0;
            _customDataList.Clear();
            for (var i = 0; i < _sphereHitEntries.Count; i++) {
                var entry = _sphereHitEntries[i];
                var collider = entry.Collider;
                _customDataList.Add(entry.CustomData);
                _hitSnapshots[hitWrite++] = new HitSnapshot {
                    Id = entry.Id,
                    LayerMask = entry.LayerMask,
                    ShapeType = HitShapeType.Sphere,
                    CustomDataIndex = _customDataList.Count - 1,
                    Center = collider.Center,
                    Radius = collider.Radius,
                    Rotation = default,
                    HalfExtents = default,
                    Start = default,
                    End = default
                };
            }

            for (var i = 0; i < _boxHitEntries.Count; i++) {
                var entry = _boxHitEntries[i];
                var collider = entry.Collider;
                _customDataList.Add(entry.CustomData);
                _hitSnapshots[hitWrite++] = new HitSnapshot {
                    Id = entry.Id,
                    LayerMask = entry.LayerMask,
                    ShapeType = HitShapeType.Box,
                    CustomDataIndex = _customDataList.Count - 1,
                    Center = collider.Center,
                    Radius = 0.0f,
                    Rotation = collider.Rotation,
                    HalfExtents = collider.HalfExtents,
                    Start = default,
                    End = default
                };
            }

            for (var i = 0; i < _sweepHitEntries.Count; i++) {
                var entry = _sweepHitEntries[i];
                var collider = entry.Collider;
                _customDataList.Add(entry.CustomData);
                _hitSnapshots[hitWrite++] = new HitSnapshot {
                    Id = entry.Id,
                    LayerMask = entry.LayerMask,
                    ShapeType = HitShapeType.Sweep,
                    CustomDataIndex = _customDataList.Count - 1,
                    Center = default,
                    Radius = collider.Radius,
                    Rotation = default,
                    HalfExtents = default,
                    Start = collider.Start,
                    End = collider.End,
                };
            }

            for (var i = 0; i < _receiveEntries.Count; i++) {
                var entry = _receiveEntries[i];
                var collider = entry.Collider;
                _receiveSnapshots[i] = new ReceiveSnapshot {
                    Id = entry.Id,
                    Radius = collider.Radius,
                    Start = collider.Start,
                    End = collider.End,
                    LayerMask = entry.LayerMask,
                };
            }

            // Gridで候補ペア作成
            _hitGrid.Clear();
            for (var hitIndex = 0; hitIndex < hitWrite; hitIndex++) {
                var hitSnapshot = _hitSnapshots[hitIndex];
                switch (hitSnapshot.ShapeType) {
                    case HitShapeType.Sphere:
                        _hitGrid.UpsertCircleXZ(hitIndex, hitSnapshot.Center, hitSnapshot.Radius);
                        break;
                    case HitShapeType.Box:
                        _hitGrid.UpsertObbXZ(hitIndex, hitSnapshot.Center, hitSnapshot.Rotation, hitSnapshot.HalfExtents);
                        break;
                    case HitShapeType.Sweep:
                        _hitGrid.UpsertCapsuleXZ(hitIndex, hitSnapshot.Start, hitSnapshot.End, hitSnapshot.Radius);
                        break;
                }
            }

            var pairCount = 0;
            for (var receiveIndex = 0; receiveIndex < _receiveEntries.Count; receiveIndex++) {
                var receiveSnapshot = _receiveSnapshots[receiveIndex];
                var queryStart = receiveSnapshot.Start;
                var queryEnd = receiveSnapshot.End;
                var queryRadius = receiveSnapshot.Radius;

                _hitGrid.QueryCapsuleXZ(queryStart, queryEnd, queryRadius, _tmpCandidates);

                for (var i = 0; i < _tmpCandidates.Count; i++) {
                    var hitIndex = _tmpCandidates[i];

                    if (pairCount >= _candidatePairs.Length) {
                        GrowPairs(pairCount + 1);
                    }

                    _candidatePairs[pairCount++] = new CandidatePair { HitIndex = hitIndex, ReceiveIndex = receiveIndex };
                }
            }

            if (pairCount > 0) {
                // Jobを使った処理の実行
                var job = new CheckHitJob {
                    HitSnapshots = _hitSnapshots,
                    ReceiveSnapshots = _receiveSnapshots,
                    CandidatePairs = _candidatePairs,
                    PairCount = pairCount,
                    HitFlags = _hitFlags,
                    ContactPoints = _contactPoints,
                    ContactNormals = _contactNormals,
                };
                var handle = job.Schedule(pairCount, 64);
                handle.Complete();
            }

            // 通知処理
            _currentPairKeys.Clear();
            for (var i = 0; i < pairCount; i++) {
                if (_hitFlags[i] == 0) {
                    continue;
                }

                var pair = _candidatePairs[i];
                var hitSnapshot = _hitSnapshots[pair.HitIndex];
                var hitId = hitSnapshot.Id;
                var receiveId = _receiveSnapshots[pair.ReceiveIndex].Id;
                var listener = _receiveEntries[pair.ReceiveIndex].Listener;
                var customData = _customDataList[hitSnapshot.CustomDataIndex];
                var key = new PairKey(hitId, receiveId);
                _currentPairKeys[key] = customData;

                if (listener != null) {
                    var contactPoint = (Vector3)_contactPoints[i];
                    var contactNormal = (Vector3)_contactNormals[i];
                    var evt = new HitEvent(hitId, receiveId, contactPoint, contactNormal, customData);
                    if (_prevPairKeys.ContainsKey(key)) {
                        listener.OnHitStay(evt);
                    }
                    else {
                        listener.OnHitEnter(evt);
                    }
                }
            }

            foreach (var pair in _prevPairKeys) {
                var key = pair.Key;
                if (_currentPairKeys.ContainsKey(key)) {
                    continue;
                }

                var hitId = key.HitId;
                var receiveId = key.ReceiveId;
                var listener = default(IHitListener);
                for (var i = 0; i < _receiveEntries.Count; i++) {
                    if (_receiveEntries[i].Id == receiveId) {
                        listener = _receiveEntries[i].Listener;
                        break;
                    }
                }

                if (listener != null) {
                    var customData = pair.Value;
                    var evt = new HitEvent(hitId, receiveId, default, default, customData);
                    listener.OnHitExit(evt);
                }
            }

            // ペア集合のスワップ
            _prevPairKeys.Clear();
            (_prevPairKeys, _currentPairKeys) = (_currentPairKeys, _prevPairKeys);

            // デバッグ情報のコミット
            CommitDebugFrame(pairCount);
        }

        /// <summary>
        /// 管理対象のNativeArrayの確保
        /// </summary>
        private void EnsureNativeArrays() {
            var totalHitSnapshotCount = _sphereHitEntries.Count + _boxHitEntries.Count + _sweepHitEntries.Count;
            Ensure(ref _hitSnapshots, totalHitSnapshotCount);
            Ensure(ref _receiveSnapshots, _receiveEntries.Count);

            // Pairsは適当の初期容量を与える
            if (!_candidatePairs.IsCreated) {
                _candidatePairs = new NativeArray<CandidatePair>(Mathf.Max(256, totalHitSnapshotCount * Mathf.Max(1, _receiveEntries.Count)), Allocator.Persistent);
                _hitFlags = new NativeArray<int>(_candidatePairs.Length, Allocator.Persistent);
                _contactPoints = new NativeArray<float3>(_candidatePairs.Length, Allocator.Persistent);
                _contactNormals = new NativeArray<float3>(_candidatePairs.Length, Allocator.Persistent);
            }
        }

        /// <summary>
        /// ペア情報の再確保
        /// </summary>
        private void GrowPairs(int needed) {
            var newSize = Mathf.NextPowerOfTwo(Mathf.Max(needed, _candidatePairs.Length + 1));

            var newPairs = new NativeArray<CandidatePair>(newSize, Allocator.Persistent);
            var newFlags = new NativeArray<int>(newSize, Allocator.Persistent);
            var newPoints = new NativeArray<float3>(newSize, Allocator.Persistent);
            var newNormals = new NativeArray<float3>(newSize, Allocator.Persistent);

            NativeArray<CandidatePair>.Copy(_candidatePairs, newPairs, _candidatePairs.Length);

            _candidatePairs.Dispose();
            _hitFlags.Dispose();
            _contactPoints.Dispose();
            _contactNormals.Dispose();

            _candidatePairs = newPairs;
            _hitFlags = newFlags;
            _contactPoints = newPoints;
            _contactNormals = newNormals;
        }

        /// <summary>
        /// NativeArrayの確保
        /// </summary>
        private static void Ensure<T>(ref NativeArray<T> array, int length)
            where T : struct {
            if (!array.IsCreated || array.Length != length) {
                if (array.IsCreated) {
                    array.Dispose();
                }

                array = new NativeArray<T>(length, Allocator.Persistent);
            }
        }
    }
}