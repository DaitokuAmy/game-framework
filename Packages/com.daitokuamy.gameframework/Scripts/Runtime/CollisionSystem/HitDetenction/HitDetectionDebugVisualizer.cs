using UnityEngine;

namespace GameFramework.CollisionSystem {
    /// <summary>
    /// HitDetectionEngineの情報を可視化するためのクラス
    /// </summary>
    public sealed class HitDetectionDebugVisualizer : MonoBehaviour {
        [SerializeField, Tooltip("ヒット当たりの表示")]
        private bool _drawHits = true;
        [SerializeField, Tooltip("受け当たりの表示")]
        private bool _drawReceives = true;
        [SerializeField, Tooltip("衝突発生の表示")]
        private bool _drawContacts = true;
        [SerializeField, Tooltip("衝突時ポイントの半径")]
        private float _contactPointRadius = 0.05f;
        [SerializeField, Tooltip("衝突時法線の長さ")]
        private float _contactNormalLength = 0.25f;

        [Header("Color")]
        [SerializeField]
        private Color _hitColor = Color.red;
        [SerializeField]
        private Color _receiveColor = Color.cyan;
        [SerializeField]
        private Color _contactColor = Color.yellow;

        private HitDetectionEngine _engine;

        /// <summary>
        /// 描画対象エンジンを設定します
        /// </summary>
        public void Bind(HitDetectionEngine engine) {
            _engine = engine;
        }

        /// <summary>
        /// 描画対象エンジンを解除します
        /// </summary>
        public void Unbind() {
            _engine = null;
        }

        /// <summary>
        /// Gizmo描画
        /// </summary>
        private void OnDrawGizmos() {
            if (_engine == null) {
                return;
            }

            if (!_engine.TryGetDebugFrame(out var frame)) {
                return;
            }

            if (_drawHits) {
                DrawHits(frame);
            }

            if (_drawReceives) {
                DrawReceives(frame);
            }

            if (_drawContacts) {
                DrawContacts(frame);
            }
        }

        /// <summary>
        /// ヒット形状を描画します
        /// </summary>
        private void DrawHits(HitDetectionEngine.DebugFrame frame) {
            var prevColor = Gizmos.color;
            Gizmos.color = _hitColor;

            foreach (var h in frame.Hits) {
                var center = h.Center;

                if (h.ShapeType == HitDetectionEngine.HitShapeType.Sphere) {
                    DrawWireSphere(center, h.Radius);
                }
                else if (h.ShapeType == HitDetectionEngine.HitShapeType.Box) {
                    DrawWireObb(h.Center, h.Rotation, h.HalfExtents);
                }
            }

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// 受け形状（カプセル）を描画します
        /// </summary>
        private void DrawReceives(HitDetectionEngine.DebugFrame frame) {
            var prevColor = Gizmos.color;
            Gizmos.color = _receiveColor;

            foreach (var r in frame.Receives) {
                DrawWireCapsule(r.Start, r.End, r.Radius);
            }

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// 接触点・法線を描画します
        /// </summary>
        private void DrawContacts(HitDetectionEngine.DebugFrame frame) {
            var prevColor = Gizmos.color;
            Gizmos.color = _contactColor;

            foreach (var c in frame.Contacts) {
                var p = c.Point;
                var n = c.Normal;

                DrawWireSphere(p, _contactPointRadius);
                Gizmos.DrawLine(p, p + n * _contactNormalLength);
            }

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Gizmosでワイヤースフィアを描画します
        /// </summary>
        private static void DrawWireSphere(Vector3 center, float radius) {
            Gizmos.DrawWireSphere(center, radius);
        }

        /// <summary>
        /// Gizmosでワイヤーカプセル（線分+半径）を描画します（簡易版）
        /// </summary>
        private static void DrawWireCapsule(Vector3 a, Vector3 b, float radius) {
            if (radius <= 0f) {
                Gizmos.DrawLine(a, b);
                return;
            }

            // 両端球
            Gizmos.DrawWireSphere(a, radius);
            Gizmos.DrawWireSphere(b, radius);

            // 側面を “4本の線” で簡易表現（SceneViewで十分分かる）
            var dir = b - a;
            var len = dir.magnitude;
            if (len <= 1e-6f) {
                return;
            }

            dir /= len;

            var basis = Mathf.Abs(dir.y) < 0.99f ? Vector3.up : Vector3.right;
            var right = Vector3.Cross(dir, basis).normalized;
            var forward = Vector3.Cross(right, dir).normalized;

            Gizmos.DrawLine(a + right * radius, b + right * radius);
            Gizmos.DrawLine(a - right * radius, b - right * radius);
            Gizmos.DrawLine(a + forward * radius, b + forward * radius);
            Gizmos.DrawLine(a - forward * radius, b - forward * radius);
        }

        /// <summary>
        /// GizmosでワイヤーOBBを描画します
        /// </summary>
        private static void DrawWireObb(Vector3 center, Quaternion rotation, Vector3 halfExtents) {
            var m = Matrix4x4.TRS((Vector3)center, (Quaternion)rotation, Vector3.one);

            // ローカルAABBの8頂点
            var he = halfExtents;

            var p0 = m.MultiplyPoint3x4(new Vector3(-he.x, -he.y, -he.z));
            var p1 = m.MultiplyPoint3x4(new Vector3(he.x, -he.y, -he.z));
            var p2 = m.MultiplyPoint3x4(new Vector3(he.x, -he.y, he.z));
            var p3 = m.MultiplyPoint3x4(new Vector3(-he.x, -he.y, he.z));

            var p4 = m.MultiplyPoint3x4(new Vector3(-he.x, he.y, -he.z));
            var p5 = m.MultiplyPoint3x4(new Vector3(he.x, he.y, -he.z));
            var p6 = m.MultiplyPoint3x4(new Vector3(he.x, he.y, he.z));
            var p7 = m.MultiplyPoint3x4(new Vector3(-he.x, he.y, he.z));

            // 底面
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);

            // 上面
            Gizmos.DrawLine(p4, p5);
            Gizmos.DrawLine(p5, p6);
            Gizmos.DrawLine(p6, p7);
            Gizmos.DrawLine(p7, p4);

            // 側面
            Gizmos.DrawLine(p0, p4);
            Gizmos.DrawLine(p1, p5);
            Gizmos.DrawLine(p2, p6);
            Gizmos.DrawLine(p3, p7);
        }
    }
}