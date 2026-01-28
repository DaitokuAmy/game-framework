using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// ヒット検出エンジン
    /// </summary>
    partial class HitDetectionEngine {
        /// <summary>
        /// デバッグ描画用フレーム（1フレーム分のスナップショット）
        /// </summary>
        internal readonly struct DebugFrame {
            /// <summary>ヒット形状の一覧</summary>
            public readonly IReadOnlyList<DebugHit> Hits;
            /// <summary>受け形状の一覧</summary>
            public readonly IReadOnlyList<DebugReceive> Receives;
            /// <summary>ヒットしている接触情報の一覧（候補ペアのうち命中したもの）</summary>
            public readonly IReadOnlyList<DebugContact> Contacts;
            /// <summary>フレーム番号（更新ごとに増える）</summary>
            public readonly int FrameIndex;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public DebugFrame(
                IReadOnlyList<DebugHit> hits,
                IReadOnlyList<DebugReceive> receives,
                IReadOnlyList<DebugContact> contacts,
                int frameIndex) {
                Hits = hits;
                Receives = receives;
                Contacts = contacts;
                FrameIndex = frameIndex;
            }
        }

        /// <summary>
        /// デバッグ用ヒット情報
        /// </summary>
        internal readonly struct DebugHit {
            /// <summary>登録ID</summary>
            public readonly int Id;
            /// <summary>判定レイヤーマスク</summary>
            public readonly int LayerMask;
            /// <summary>形状種別</summary>
            public readonly HitShapeType ShapeType;
            /// <summary>中心</summary>
            public readonly Vector3 Center;
            /// <summary>Sphere半径（Sphereのみ）</summary>
            public readonly float Radius;
            /// <summary>回転</summary>
            public readonly Quaternion Rotation;
            /// <summary>ハーフサイズ</summary>
            public readonly Vector3 HalfExtents;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public DebugHit(int id, int layerMask, HitShapeType shapeType, Vector3 center, float radius, Quaternion rotation, Vector3 halfExtents) {
                Id = id;
                LayerMask = layerMask;
                ShapeType = shapeType;
                Center = center;
                Radius = radius;
                Rotation = rotation;
                HalfExtents = halfExtents;
            }
        }

        /// <summary>
        /// デバッグ用受け情報（カプセル）
        /// </summary>
        internal readonly struct DebugReceive {
            /// <summary>登録ID</summary>
            public readonly int Id;
            /// <summary>判定レイヤーマスク</summary>
            public readonly int LayerMask;
            /// <summary>カプセル線分始点</summary>
            public readonly Vector3 Start;
            /// <summary>カプセル線分終点</summary>
            public readonly Vector3 End;
            /// <summary>半径</summary>
            public readonly float Radius;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public DebugReceive(int id, int layerMask, Vector3 start, Vector3 end, float radius) {
                Id = id;
                LayerMask = layerMask;
                Start = start;
                End = end;
                Radius = radius;
            }
        }

        /// <summary>
        /// デバッグ用接触情報
        /// </summary>
        internal readonly struct DebugContact {
            /// <summary>ヒットID</summary>
            public readonly int HitId;
            /// <summary>受けID</summary>
            public readonly int ReceiveId;
            /// <summary>接触点（Receive側）</summary>
            public readonly Vector3 Point;
            /// <summary>法線（Receive側の外向き）</summary>
            public readonly Vector3 Normal;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public DebugContact(int hitId, int receiveId, Vector3 point, Vector3 normal) {
                HitId = hitId;
                ReceiveId = receiveId;
                Point = point;
                Normal = normal;
            }
        }

        private readonly List<DebugHit> _debugHits = new(256);
        private readonly List<DebugReceive> _debugReceives = new(256);
        private readonly List<DebugContact> _debugContacts = new(256);

        private DebugFrame _debugFrame;
        private int _debugFrameIndex;
        private bool _debugEnabled;

        private HitDetectionDebugVisualizer _debugVisualizer;

        /// <summary>
        /// デバッグ描画を有効化
        /// </summary>
        /// <param name="name">生成するGameObject名</param>
        public void EnableDebugView(string name = "HitDetectionEngine(Debug)") {
            if (_disposed) {
                return;
            }

            if (_debugEnabled) {
                return;
            }

            _debugEnabled = true;

            var go = new GameObject(name);
            go.hideFlags = HideFlags.DontSave;
            _debugVisualizer = go.AddComponent<HitDetectionDebugVisualizer>();
            _debugVisualizer.Bind(this);
        }

        /// <summary>
        /// デバッグ描画を無効化
        /// </summary>
        public void DisableDebugView() {
            if (!_debugEnabled) {
                return;
            }

            _debugEnabled = false;

            if (_debugVisualizer != null) {
                var go = _debugVisualizer.gameObject;
                _debugVisualizer.Unbind();
                _debugVisualizer = null;

                if (go != null) {
                    Object.Destroy(go);
                }
            }
        }

        /// <summary>
        /// デバッグ用フレームを取得します
        /// </summary>
        /// <param name="frame">取得できた場合に返すフレーム</param>
        /// <returns>取得できた場合 true</returns>
        internal bool TryGetDebugFrame(out DebugFrame frame) {
            if (!_debugEnabled) {
                frame = default;
                return false;
            }

            frame = _debugFrame;
            return true;
        }

        /// <summary>
        /// Update() の最後に呼び出して、描画用フレームを確定させます
        /// </summary>
        private void CommitDebugFrame(int pairCount) {
            if (!_debugEnabled) {
                return;
            }

            _debugHits.Clear();
            _debugReceives.Clear();
            _debugContacts.Clear();

            // Hits
            for (var i = 0; i < _hitSnapshots.Length; i++) {
                var hs = _hitSnapshots[i];
                _debugHits.Add(new DebugHit(
                    hs.Id,
                    hs.LayerMask,
                    hs.ShapeType,
                    hs.Center,
                    hs.Radius,
                    hs.Rotation,
                    hs.HalfExtents));
            }

            // Receives
            for (var i = 0; i < _receiveSnapshots.Length; i++) {
                var rs = _receiveSnapshots[i];
                _debugReceives.Add(new DebugReceive(
                    rs.Id,
                    rs.LayerMask,
                    rs.Start,
                    rs.End,
                    rs.Radius));
            }

            // Contacts（命中分のみ）
            for (var i = 0; i < pairCount; i++) {
                if (_hitFlags[i] == 0) {
                    continue;
                }

                var pair = _candidatePairs[i];
                var hitId = _hitSnapshots[pair.HitIndex].Id;
                var receiveId = _receiveSnapshots[pair.ReceiveIndex].Id;

                _debugContacts.Add(new DebugContact(
                    hitId,
                    receiveId,
                    _contactPoints[i],
                    _contactNormals[i]));
            }

            _debugFrameIndex++;
            _debugFrame = new DebugFrame(_debugHits, _debugReceives, _debugContacts, _debugFrameIndex);
        }
    }
}