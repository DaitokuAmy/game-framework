using Unity.Mathematics;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// ヒット検出エンジン
    /// </summary>
    partial class HitDetectionEngine {
        /// <summary>
        /// 当たり判定計算(Sphere hit vs Capsule receive)
        /// </summary>
        /// <param name="hit">ヒットコリジョン（Sphere）</param>
        /// <param name="receive">受けコリジョン（Capsule: start/end/radius）</param>
        /// <param name="contactPoint">衝突している場合のCapsule表面上の接触点</param>
        /// <param name="contactNormal">衝突している場合のCapsule表面の外向き法線（Hit方向）</param>
        /// <returns>衝突している場合 true</returns>
        private static bool TryGetReceiveContactPoint_Sphere(
            HitSnapshot hit,
            ReceiveSnapshot receive,
            out float3 contactPoint,
            out float3 contactNormal) {
            var sphereCenter = hit.Center;
            var sphereRadius = hit.Radius;

            var capsuleA = receive.Start;
            var capsuleB = receive.End;
            var capsuleRadius = receive.Radius;

            contactPoint = default;
            contactNormal = default;

            if (capsuleRadius < 0.0f || sphereRadius < 0.0f) {
                return false;
            }

            // カプセル軸線分上の最近点 Q
            var q = ClosestPointOnSegment(capsuleA, capsuleB, sphereCenter);

            // 軸から Sphere中心までのベクトル
            var v = sphereCenter - q;
            var distSq = math.lengthsq(v);

            var sumR = capsuleRadius + sphereRadius;
            var sumRSq = sumR * sumR;

            if (distSq > sumRSq) {
                return false;
            }

            // Receive 側の外向き法線（軸→Sphere方向）
            // 方向が定まらない（球中心が軸上）場合は、線分方向に直交する安定方向を作る
            contactNormal = SafeNormalizeFromSegment(v, capsuleA, capsuleB);

            // Receive 側（カプセル表面）接触点
            contactPoint = q + contactNormal * capsuleRadius;
            return true;
        }

        /// <summary>
        /// 当たり判定計算(OBB hit vs Capsule receive)
        /// </summary>
        /// <param name="hit">ヒットコリジョン（OBB）</param>
        /// <param name="receive">受けコリジョン（Capsule: start/end/radius）</param>
        /// <param name="contactPoint">衝突している場合のCapsule表面上の接触点</param>
        /// <param name="contactNormal">衝突している場合のCapsule表面の外向き法線（Hit方向）</param>
        /// <returns>衝突している場合 true</returns>
        private static bool TryGetReceiveContactPoint_Box(
            HitSnapshot hit,
            ReceiveSnapshot receive,
            out float3 contactPoint,
            out float3 contactNormal) {
            contactPoint = default;
            contactNormal = default;

            var capsuleAWorld = receive.Start;
            var capsuleBWorld = receive.End;
            var capsuleRadius = receive.Radius;

            if (capsuleRadius < 0.0f) {
                return false;
            }

            // OBBローカルへ（OBB中心=原点、回転=Identity）
            var invRot = math.inverse(hit.Rotation);

            var a = math.mul(invRot, capsuleAWorld - hit.Center);
            var b = math.mul(invRot, capsuleBWorld - hit.Center);

            // OBBはローカルでは AABB: [-e, +e]
            var e = hit.HalfExtents;

            // 線分上の点を複数評価して AABB への最近点を取る（軽量・実用版）
            // ※厳密な Segment-AABB 最短距離へ差し替える余地あり
            var bestDistSq = float.PositiveInfinity;
            var bestAxisPointLocal = default(float3); // 線分上の点（ローカル）
            var bestBoxPointLocal = default(float3); // AABB上の最近点（ローカル）

            Evaluate(0.0f);
            Evaluate(1.0f);
            Evaluate(0.5f);

            // 面交点っぽい候補
            AddPlaneCandidates(a, b, e.x, 0);
            AddPlaneCandidates(a, b, e.y, 1);
            AddPlaneCandidates(a, b, e.z, 2);

            if (bestDistSq > capsuleRadius * capsuleRadius) {
                return false;
            }

            // ワールドへ戻す（軸点・箱点）
            var axisPointWorld = hit.Center + math.mul(hit.Rotation, bestAxisPointLocal);
            var boxPointWorld = hit.Center + math.mul(hit.Rotation, bestBoxPointLocal);

            // Receive 側外向き法線：カプセル軸→ヒット（箱）方向
            var v = boxPointWorld - axisPointWorld;
            contactNormal = SafeNormalizeFromSegment(v, capsuleAWorld, capsuleBWorld);

            // Receive 側表面点
            contactPoint = axisPointWorld + contactNormal * capsuleRadius;
            return true;

            void Evaluate(float t) {
                t = math.clamp(t, 0.0f, 1.0f);
                var p = math.lerp(a, b, t); // 線分上（ローカル）
                var c = ClampToAabb(p, e); // AABB最近点（ローカル）
                var d = p - c;
                var dsq = math.lengthsq(d);

                if (dsq < bestDistSq) {
                    bestDistSq = dsq;
                    bestAxisPointLocal = p;
                    bestBoxPointLocal = c;
                }
            }

            void AddPlaneCandidates(float3 p0, float3 p1, float extent, int axis) {
                var d = p1 - p0;
                var denom = axis == 0 ? d.x : axis == 1 ? d.y : d.z;
                if (math.abs(denom) < 1e-8f) {
                    return;
                }

                var p0A = axis == 0 ? p0.x : axis == 1 ? p0.y : p0.z;

                var tPos = (extent - p0A) / denom;
                var tNeg = (-extent - p0A) / denom;

                if (tPos >= 0.0f && tPos <= 1.0f) Evaluate(tPos);
                if (tNeg >= 0.0f && tNeg <= 1.0f) Evaluate(tNeg);
            }

            static float3 ClampToAabb(float3 p, float3 he) {
                return new float3(
                    math.clamp(p.x, -he.x, he.x),
                    math.clamp(p.y, -he.y, he.y),
                    math.clamp(p.z, -he.z, he.z)
                );
            }
        }

        /// <summary>
        /// 当たり判定計算（Sweep(線分+半径) vs Capsule(線分+半径)）
        /// </summary>
        /// <param name="hit">ヒットコリジョン（Sweep）</param>
        /// <param name="receive">受けコリジョン（Capsule）</param>
        /// <param name="receiveContactPoint">衝突している場合の受けCapsule表面上の接触点</param>
        /// <param name="receiveContactNormal">衝突している場合の受けCapsule外向き法線（受け→ヒット方向）</param>
        /// <returns>衝突している場合 true</returns>
        private static bool TryGetReceiveContactPoint_Sweep(
            HitSnapshot hit,
            ReceiveSnapshot receive,
            out float3 receiveContactPoint,
            out float3 receiveContactNormal) {
            receiveContactPoint = default;
            receiveContactNormal = default;

            var hitA = hit.Start;
            var hitB = hit.End;
            var hitR = hit.Radius;

            var recA = receive.Start;
            var recB = receive.End;
            var recR = receive.Radius;

            if (hitR < 0.0f || recR < 0.0f) {
                return false;
            }

            // 2本の線分（カプセル軸）間の最近点
            ClosestPointSegmentSegment(hitA, hitB, recA, recB, out var s, out var t, out var cHit, out var cRec);

            // 軸間距離で衝突判定（カプセル同士 = 線分距離 <= 半径和）
            var v = cHit - cRec;
            var distSq = math.lengthsq(v);
            var sumR = hitR + recR;

            if (distSq > sumR * sumR) {
                return false;
            }

            // 受け側法線（受け軸点 -> ヒット軸点）
            const float eps = 1e-8f;
            float3 n;
            if (distSq > eps) {
                n = v * math.rsqrt(distSq);
            }
            else {
                // 退避：軸同士がほぼ重なって方向が出ない
                // ヒットの進行方向や軸方向から安定した法線を作る
                var hitDir = hitB - hitA;
                var recDir = recB - recA;

                var nh = SafeOrtho(hitDir);
                var nr = SafeOrtho(recDir);

                // どちらかが安定なら使う。両方不安定なら固定ベクトル
                n = math.lengthsq(nh) > eps ? nh : (math.lengthsq(nr) > eps ? nr : new float3(1.0f, 0.0f, 0.0f));
            }

            receiveContactNormal = n; // 受け→ヒット方向
            receiveContactPoint = cRec + n * recR; // 受けカプセル表面点
            return true;

            static float3 SafeOrtho(float3 dir) {
                const float eps2 = 1e-8f;
                var lenSq = math.lengthsq(dir);
                if (lenSq <= eps2) {
                    return default;
                }

                dir *= math.rsqrt(lenSq);

                // dir と直交するベクトルを作る（安定）
                var basis = math.abs(dir.y) < 0.99f ? new float3(0.0f, 1.0f, 0.0f) : new float3(1.0f, 0.0f, 0.0f);
                var n = math.cross(dir, basis);
                var nLenSq = math.lengthsq(n);
                if (nLenSq <= eps2) {
                    return default;
                }

                return n * math.rsqrt(nLenSq);
            }
        }

        /// <summary>
        /// SphereCast判定
        /// </summary>
        private static bool TrySphereCast_FirstHit(
            float3 origin,
            float3 direction,
            float radius,
            float maxDistance,
            ReceiveSnapshot receive,
            out float hitDistance,
            out float3 receivePoint,
            out float3 receiveNormal) {
            hitDistance = 0.0f;
            receivePoint = default;
            receiveNormal = default;

            if (radius < 0.0f || maxDistance <= 0.0f) {
                return false;
            }

            var a = receive.Start;
            var b = receive.End;

            var inflatedRadius = receive.Radius + radius;
            if (inflatedRadius < 0.0f) {
                return false;
            }

            // 判定関数：時刻t(0..1)での球中心が “膨張カプセル” 内か
            bool IsInside(float t01) {
                var p = origin + direction * (maxDistance * t01);
                var q = ClosestPointOnSegment(a, b, p);
                var d = p - q;
                return math.lengthsq(d) <= inflatedRadius * inflatedRadius;
            }

            // 全体で当たらないならfalse（粗チェック）
            // ここは2点だけだと取りこぼすので、簡易に mid も見る
            if (!IsInside(0.0f) && !IsInside(1.0f) && !IsInside(0.5f)) {
                // さらに厳密にやるなら segment-segment 最短距離で弾く
                // ただ、この時点で候補はGridで減っている想定なので軽さ優先
                // “本当に外れ”を取りこぼす可能性が気になるなら、下の距離チェック版に差し替えてOK
            }

            // first hit 二分探索：当たっている領域の最小t
            // hi=1 が当たってる保証はないので、まず当たりが存在するtを探す
            var lo = 0.0f;
            var hi = 1.0f;

            // まず当たりがあるかを “区間走査(粗)” で確認
            const int probeCount = 8;
            var found = false;
            var first = 1.0f;

            for (var i = 0; i <= probeCount; i++) {
                var t = (float)i / probeCount;
                if (IsInside(t)) {
                    found = true;
                    first = t;
                    break;
                }
            }

            if (!found) {
                return false;
            }

            // first が見つかったので、[0, first] に答えがある
            lo = 0.0f;
            hi = first;

            // 二分探索（10回で十分）
            for (var i = 0; i < 10; i++) {
                var mid = (lo + hi) * 0.5f;
                if (IsInside(mid)) {
                    hi = mid;
                }
                else {
                    lo = mid;
                }
            }

            hitDistance = hi * maxDistance;

            // contact: Receive側（元のカプセル表面）点と法線を返す
            var sphereCenterAtHit = origin + direction * hitDistance;
            var axisPoint = ClosestPointOnSegment(a, b, sphereCenterAtHit);

            var v = sphereCenterAtHit - axisPoint;
            var lenSq = math.lengthsq(v);

            // Receive外向き法線（軸→球中心）
            receiveNormal = SafeNormalizeFromSegment(v, a, b);

            // Receive表面点（元の半径で押し出す）
            receivePoint = axisPoint + receiveNormal * receive.Radius;

            return true;
        }

        /// <summary>
        /// 線分 AB 上の点で、点 P に最も近い点を返します。
        /// </summary>
        private static float3 ClosestPointOnSegment(float3 a, float3 b, float3 p) {
            var ab = b - a;
            var abLenSq = math.lengthsq(ab);

            if (abLenSq <= 1e-12f) {
                return a;
            }

            var t = math.dot(p - a, ab) / abLenSq;
            t = math.clamp(t, 0.0f, 1.0f);
            return a + ab * t;
        }

        /// <summary>
        /// v を正規化して返します。長さがほぼゼロの場合は、線分方向から安定した代替ベクトルを作って返します。
        /// </summary>
        private static float3 SafeNormalizeFromSegment(float3 v, float3 segA, float3 segB) {
            var lenSq = math.lengthsq(v);
            const float eps = 1e-8f;

            if (lenSq > eps) {
                return v * math.rsqrt(lenSq);
            }

            // v がゼロに近い：線分方向に直交する方向を作る
            var segDir = segB - segA;
            var segLenSq = math.lengthsq(segDir);

            if (segLenSq <= eps) {
                // 線分も潰れているなら固定方向
                return new float3(1.0f, 0.0f, 0.0f);
            }

            segDir *= math.rsqrt(segLenSq);

            // segDir と平行になりにくい基底を選ぶ
            var basis = math.abs(segDir.y) < 0.99f ? new float3(0.0f, 1.0f, 0.0f) : new float3(1.0f, 0.0f, 0.0f);

            // segDir と basis の外積で直交ベクトル
            var n = math.cross(segDir, basis);
            var nLenSq = math.lengthsq(n);

            if (nLenSq <= eps) {
                return new float3(1.0f, 0.0f, 0.0f);
            }

            return n * math.rsqrt(nLenSq);
        }

        /// <summary>
        /// 線分と線分の最近点（Ericson系の定番）
        /// </summary>
        private static void ClosestPointSegmentSegment(
            float3 p1, float3 q1, float3 p2, float3 q2,
            out float s, out float t, out float3 c1, out float3 c2) {
            var d1 = q1 - p1;
            var d2 = q2 - p2;
            var r = p1 - p2;

            var a = math.dot(d1, d1);
            var e = math.dot(d2, d2);
            var f = math.dot(d2, r);

            const float eps = 1e-8f;

            if (a <= eps && e <= eps) {
                s = 0.0f;
                t = 0.0f;
                c1 = p1;
                c2 = p2;
                return;
            }

            if (a <= eps) {
                s = 0.0f;
                t = math.clamp(f / e, 0.0f, 1.0f);
            }
            else {
                var c = math.dot(d1, r);

                if (e <= eps) {
                    t = 0.0f;
                    s = math.clamp(-c / a, 0.0f, 1.0f);
                }
                else {
                    var b = math.dot(d1, d2);
                    var denom = a * e - b * b;

                    if (math.abs(denom) > eps) {
                        s = math.clamp((b * f - c * e) / denom, 0.0f, 1.0f);
                    }
                    else {
                        s = 0.0f;
                    }

                    var tnom = b * s + f;
                    if (tnom < 0.0f) {
                        t = 0.0f;
                        s = math.clamp(-c / a, 0.0f, 1.0f);
                    }
                    else if (tnom > e) {
                        t = 1.0f;
                        s = math.clamp((b - c) / a, 0.0f, 1.0f);
                    }
                    else {
                        t = tnom / e;
                    }
                }
            }

            c1 = p1 + d1 * s;
            c2 = p2 + d2 * t;
        }
    }
}