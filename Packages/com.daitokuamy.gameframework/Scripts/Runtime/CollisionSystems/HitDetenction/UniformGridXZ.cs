using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 均一グリッド(XZ)
    /// </summary>
    internal sealed class UniformGrid2D {
        private readonly float _cellSize;
        private readonly Dictionary<long, List<int>> _cellToIds = new();
        private readonly Dictionary<int, List<long>> _idToCells = new();
        private readonly ObjectPool<List<long>> _listPool;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UniformGrid2D(float cellSize) {
            _cellSize = cellSize;

            _listPool = new ObjectPool<List<long>>(
                createFunc: () => new List<long>(8),
                actionOnGet: list => list.Clear(),
                actionOnRelease: list => list.Clear(),
                actionOnDestroy: list => list.Clear(),
                defaultCapacity: 64);
        }

        /// <summary>
        /// グリッド内の情報をクリア
        /// </summary>
        public void Clear() {
            foreach (var pair in _idToCells) {
                _listPool.Release(pair.Value);
            }

            _cellToIds.Clear();
            _idToCells.Clear();
        }

        /// <summary>
        /// Idの除外
        /// </summary>
        public void Remove(int id) {
            if (!_idToCells.TryGetValue(id, out var cells)) {
                return;
            }

            for (var i = 0; i < cells.Count; i++) {
                var key = cells[i];
                if (_cellToIds.TryGetValue(key, out var list)) {
                    list.Remove(id);
                    if (list.Count == 0) {
                        _cellToIds.Remove(key);
                    }
                }
            }

            _idToCells.Remove(id);
            _listPool.Release(cells);
        }

        /// <summary>
        /// Circle単位での登録
        /// </summary>
        public void UpsertCircleXZ(int id, Vector3 center, float radius) {
            // 矩形を作成して更新
            var minX = center.x - radius;
            var maxX = center.x + radius;
            var minZ = center.z - radius;
            var maxZ = center.z + radius;
            UpsertRect(id, minX, maxX, minZ, maxZ);
        }

        /// <summary>
        /// OBB単位での登録
        /// </summary>
        public void UpsertObbXZ(int id, Vector3 center, Quaternion rotation, Vector3 halfExtents) {
            // AABBにした際のサイズを計算
            var matrix = Matrix4x4.TRS(center, rotation, halfExtents);
            var ex = halfExtents.x;
            var ey = halfExtents.y;
            var ez = halfExtents.z;
            var aabbHalfX = Mathf.Abs(matrix.m00) * ex + Mathf.Abs(matrix.m01) * ey + Mathf.Abs(matrix.m02) * ez;
            var aabbHalfZ = Mathf.Abs(matrix.m20) * ex + Mathf.Abs(matrix.m21) * ey + Mathf.Abs(matrix.m22) * ez;

            // 矩形を作成して更新
            var minX = center.x - aabbHalfX;
            var maxX = center.x + aabbHalfX;
            var minZ = center.z - aabbHalfZ;
            var maxZ = center.z + aabbHalfZ;

            UpsertRect(id, minX, maxX, minZ, maxZ);
        }

        /// <summary>
        /// カプセル単位での登録
        /// </summary>
        public void UpsertCapsuleXZ(int id, Vector3 start, Vector3 end, float radius) {
            if (radius < 0.0f) {
                radius = 0.0f;
            }

            // CapsuleをXZに投影したAABB（線分の両端＋半径）
            var minX = Mathf.Min(start.x, end.x) - radius;
            var maxX = Mathf.Max(start.x, end.x) + radius;
            var minZ = Mathf.Min(start.z, end.z) - radius;
            var maxZ = Mathf.Max(start.z, end.z) + radius;

            UpsertRect(id, minX, maxX, minZ, maxZ);
        }

        /// <summary>
        /// 矩形情報でのCell登録処理
        /// </summary>
        private void UpsertRect(int id, float minX, float maxX, float minZ, float maxZ) {
            Remove(id);

            var minCx = WorldToCell(minX);
            var maxCx = WorldToCell(maxX);
            var minCz = WorldToCell(minZ);
            var maxCz = WorldToCell(maxZ);

            var keys = _listPool.Get();

            for (var cz = minCz; cz <= maxCz; cz++) {
                for (var cx = minCx; cx <= maxCx; cx++) {
                    var key = Pack(cx, cz);
                    if (!_cellToIds.TryGetValue(key, out var list)) {
                        list = new List<int>(8);
                        _cellToIds[key] = list;
                    }

                    list.Add(id);
                    keys.Add(key);
                }
            }

            _idToCells[id] = keys;
        }

        /// <summary>
        /// Circle範囲の登録Idを列挙
        /// </summary>
        public void QueryCircleXZ(Vector3 center, float radius, List<int> outHitIndices) {
            var minX = center.x - radius;
            var maxX = center.x + radius;
            var minZ = center.z - radius;
            var maxZ = center.z + radius;
            
            QueryRect(minX, maxX, minZ, maxZ, outHitIndices);
        }

        /// <summary>
        /// Capsule範囲の登録Idを列挙
        /// </summary>
        public void QueryCapsuleXZ(Vector3 start, Vector3 end, float radius, List<int> outHitIndices) {
            var min = Vector3.Min(start, end);
            var max = Vector3.Max(start, end);

            var minX = min.x - radius;
            var maxX = min.x + radius;
            var minZ = max.z - radius;
            var maxZ = max.z + radius;
            
            QueryRect(minX, maxX, minZ, maxZ, outHitIndices);
        }

        /// <summary>
        /// 矩形範囲の登録Idを列挙
        /// </summary>
        public void QueryRect(float minX, float maxX, float minZ, float maxZ, List<int> outHitIndices) {
            outHitIndices.Clear();
            
            var minCx = WorldToCell(minX);
            var maxCx = WorldToCell(maxX);
            var minCz = WorldToCell(minZ);
            var maxCz = WorldToCell(maxZ);

            var yielded = HashSetPool<int>.Get();
            try {
                for (var cz = minCz; cz <= maxCz; cz++) {
                    for (var cx = minCx; cx <= maxCx; cx++) {
                        var key = Pack(cx, cz);
                        if (!_cellToIds.TryGetValue(key, out var list)) {
                            continue;
                        }

                        for (var i = 0; i < list.Count; i++) {
                            var id = list[i];
                            if (yielded.Add(id)) {
                                outHitIndices.Add(id);
                            }
                        }
                    }
                }
            }
            finally {
                HashSetPool<int>.Release(yielded);
            }
        }

        /// <summary>
        /// ワールド値からセル値に変換
        /// </summary>
        private int WorldToCell(float v) {
            return Mathf.FloorToInt(v / _cellSize);
        }

        /// <summary>
        /// セル情報をHashに変換
        /// </summary>
        private static long Pack(int cx, int cz) {
            unchecked {
                return ((long)cx << 32) | (uint)cz;
            }
        }
    }
}