using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// コリジョン管理クラス
    /// </summary>
    public class CollisionManager : DisposableLateUpdatable {
        /// <summary>
        /// 更新モード
        /// </summary>
        public enum UpdateMode {
            Update,
            LateUpdate,
        }

        /// <summary>
        /// コリジョン情報用インターフェース
        /// </summary>
        internal interface ICollisionInfo {
            bool IsValid { get; }
        }

        /// <summary>
        /// コリジョン情報
        /// </summary>
        private class CollisionInfo : ICollisionInfo {
            public bool Destroy;
            public ICollisionListener Listener;
            public ICollision Collision;
            public int LayerMask;
            public object CustomData;
            public float Timer;

            bool ICollisionInfo.IsValid => !Destroy && Collision != null;
        }

        /// <summary>
        /// レイキャストコリジョン情報
        /// </summary>
        private class RaycastCollisionInfo : ICollisionInfo {
            public bool Destroy;
            public IRaycastCollisionListener Listener;
            public IRaycastCollision Collision;
            public int LayerMask;
            public int HitCount;
            public object CustomData;
            public float Timer;

            bool ICollisionInfo.IsValid => !Destroy && Collision != null;
        }

        // Collision判定時のワーク領域
        private int _collisionResultBufferSize = 0;
        private Collider[] _workCollisionBuffer = Array.Empty<Collider>();
        private RaycastHit[] _workRaycastBuffer = Array.Empty<RaycastHit>();

        // 時間軸
        private LayeredTime _layeredTime;

        // 登録済みコリジョン情報
        private List<CollisionInfo> _collisionInfos = new List<CollisionInfo>();
        private List<RaycastCollisionInfo> _raycastCollisionInfos = new List<RaycastCollisionInfo>();

        // 結果格納用ワーク
        private List<Collider> _workCollisionResults = new List<Collider>();
        private List<RaycastHit> _workRaycastResults = new List<RaycastHit>();

        /// <summary>当たり判定時の結果格納用バッファサイズ</summary>
        public int CollisionResultBufferSize {
            get => _collisionResultBufferSize;
            set {
                var max = Mathf.Max(0, value);
                if (_collisionResultBufferSize == max) {
                    return;
                }

                _collisionResultBufferSize = max;
                _workCollisionBuffer = new Collider[_collisionResultBufferSize];
                _workRaycastBuffer = new RaycastHit[_collisionResultBufferSize];
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layeredTime">時間軸用</param>
        /// <param name="collisionResultBufferSize">当たり判定検出時の固定バッファーサイズ</param>
        public CollisionManager(LayeredTime layeredTime = null, int collisionResultBufferSize = 32) {
            _layeredTime = layeredTime;
            CollisionResultBufferSize = collisionResultBufferSize;
        }

        /// <summary>
        /// コリジョンの登録
        /// </summary>
        /// <param name="listener">通知を受け取るためのリスナー</param>
        /// <param name="collision">衝突判定用コリジョン</param>
        /// <param name="layerMask">判定対象を絞るためのレイヤーマスク</param>
        /// <param name="customData">当たり通知時に付与できるカスタムデータ</param>
        /// <param name="autoDisposeTimer">自動廃棄されるためのタイマー(負の値だと無効)</param>
        /// <param name="clearHistory">衝突履歴をクリアするか</param>
        public CollisionHandle Register(ICollisionListener listener, ICollision collision, int layerMask, object customData, float autoDisposeTimer = -1, bool clearHistory = true) {
            var collisionInfo = new CollisionInfo {
                Listener = listener,
                Collision = collision,
                LayerMask = layerMask,
                CustomData = customData,
                Timer = autoDisposeTimer
            };

            if (clearHistory) {
                collision.ClearHistory();
            }

            // 管理リストに登録
            _collisionInfos.Add(collisionInfo);

            // ハンドル生成
            var handle = new CollisionHandle(this, collisionInfo);

            // Debug用の登録
            CollisionVisualizer.Register(collision);

            return handle;
        }

        /// <summary>
        /// コリジョンの登録
        /// </summary>
        /// <param name="collision">衝突判定用コリジョン</param>
        /// <param name="layerMask">判定対象を絞るためのレイヤーマスク</param>
        /// <param name="customData">当たり通知時に付与できるカスタムデータ</param>
        /// <param name="hitAction">当たり判定通知用のCallback</param>
        /// <param name="autoDisposeTimer">自動廃棄されるためのタイマー(負の値だと無効)</param>
        /// <param name="clearHistory">衝突履歴をクリアするか</param>
        public CollisionHandle Register(ICollision collision, int layerMask, object customData, Action<HitResult> hitAction, float autoDisposeTimer = -1, bool clearHistory = true) {
            var listener = new CollisionListener();
            listener.HitCollisionEvent += hitAction;
            return Register(listener, collision, layerMask, customData, autoDisposeTimer, clearHistory);
        }

        /// <summary>
        /// コリジョンの登録
        /// </summary>
        /// <param name="listener">通知を受け取るためのリスナー</param>
        /// <param name="collision">衝突判定用コリジョン</param>
        /// <param name="layerMask">判定対象を絞るためのレイヤーマスク</param>
        /// <param name="customData">当たり通知時に付与できるカスタムデータ</param>
        /// <param name="autoDisposeTimer">自動廃棄されるためのタイマー(負の値だと無効)</param>
        /// <param name="clearHistory">衝突履歴をクリアするか</param>
        public CollisionHandle Register(IRaycastCollisionListener listener, IRaycastCollision collision, int layerMask, object customData, float autoDisposeTimer = -1, bool clearHistory = true) {
            var collisionInfo = new RaycastCollisionInfo {
                Listener = listener,
                Collision = collision,
                LayerMask = layerMask,
                CustomData = customData,
                Timer = autoDisposeTimer
            };

            if (clearHistory) {
                collision.ClearHistory();
            }

            // 管理リストに登録
            _raycastCollisionInfos.Add(collisionInfo);

            // ハンドル生成
            var handle = new CollisionHandle(this, collisionInfo);

            // Debug用の登録
            CollisionVisualizer.Register(collision);

            return handle;
        }

        /// <summary>
        /// コリジョンの登録
        /// </summary>
        /// <param name="collision">衝突判定用コリジョン</param>
        /// <param name="layerMask">判定対象を絞るためのレイヤーマスク</param>
        /// <param name="customData">当たり通知時に付与できるカスタムデータ</param>
        /// <param name="hitAction">当たり判定通知用のCallback</param>
        /// <param name="autoDisposeTimer">自動廃棄されるためのタイマー(負の値だと無効)</param>
        /// <param name="clearHistory">衝突履歴をクリアするか</param>
        public CollisionHandle Register(IRaycastCollision collision, int layerMask, object customData, Action<RaycastHitResult> hitAction, float autoDisposeTimer = -1, bool clearHistory = true) {
            var listener = new RaycastCollisionListener();
            listener.HitRaycastCollisionEvent += hitAction;
            return Register(listener, collision, layerMask, customData, autoDisposeTimer, clearHistory);
        }

        /// <summary>
        /// コリジョンの登録解除
        /// </summary>
        /// <param name="handle">登録時に入手したハンドル</param>
        public void Unregister(CollisionHandle handle) {
            if (!handle.IsValid) {
                return;
            }

            // 登録されていたら削除フラグを立てる
            if (handle.CollisionInfo is CollisionInfo collisionInfo && !collisionInfo.Destroy) {
                collisionInfo.Destroy = true;
            }

            if (handle.CollisionInfo is RaycastCollisionInfo raycastCollisionInfo && !raycastCollisionInfo.Destroy) {
                raycastCollisionInfo.Destroy = true;
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            // 削除済みの物をクリア
            for (var i = 0; i < _collisionInfos.Count; i++) {
                // Debug用の登録解除
                CollisionVisualizer.Unregister(_collisionInfos[i].Collision);
            }

            for (var i = 0; i < _raycastCollisionInfos.Count; i++) {
                // Debug用の登録解除
                CollisionVisualizer.Unregister(_raycastCollisionInfos[i].Collision);
            }

            _collisionInfos.Clear();
            _raycastCollisionInfos.Clear();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            var deltaTime = _layeredTime?.DeltaTime ?? Time.deltaTime;

            UpdateCollisionInfos(deltaTime);
            UpdateRaycastCollisionInfos(deltaTime);
        }

        /// <summary>
        /// コリジョン情報の更新
        /// </summary>
        private void UpdateCollisionInfos(float deltaTime) {
            // 削除済みの物をクリア
            for (var i = _collisionInfos.Count - 1; i >= 0; i--) {
                if (!_collisionInfos[i].Destroy) {
                    continue;
                }

                // Debug用の登録解除
                CollisionVisualizer.Unregister(_collisionInfos[i].Collision);

                _collisionInfos.RemoveAt(i);
            }

            // 当たり判定実行
            var hitResult = new HitResult();
            for (var i = 0; i < _collisionInfos.Count; i++) {
                var info = _collisionInfos[i];
                _workCollisionResults.Clear();

                // 自動削除Timerを更新
                if (info.Timer >= 0.0f) {
                    info.Timer -= deltaTime;
                    info.Destroy = info.Timer <= 0.0f;
                }

                // ヒット判定
                if (!info.Collision.Tick(info.LayerMask, _workCollisionResults, _workCollisionBuffer)) {
                    continue;
                }

                // 衝突が発生していたら通知する
                hitResult.center = info.Collision.Center;
                hitResult.customData = info.CustomData;
                foreach (var result in _workCollisionResults) {
                    hitResult.collider = result;
                    info.Listener?.OnHitCollision(hitResult);
                }
            }
        }

        /// <summary>
        /// レイキャストコリジョン情報の更新
        /// </summary>
        private void UpdateRaycastCollisionInfos(float deltaTime) {
            // 削除済みの物をクリア
            for (var i = _raycastCollisionInfos.Count - 1; i >= 0; i--) {
                if (!_raycastCollisionInfos[i].Destroy) {
                    continue;
                }

                // Debug用の登録解除
                CollisionVisualizer.Unregister(_raycastCollisionInfos[i].Collision);

                _raycastCollisionInfos.RemoveAt(i);
            }

            // 当たり判定実行
            var hitResult = new RaycastHitResult();
            for (var i = 0; i < _raycastCollisionInfos.Count; i++) {
                var info = _raycastCollisionInfos[i];
                _workRaycastResults.Clear();

                // 自動削除Timerを更新
                if (info.Timer >= 0.0f) {
                    info.Timer -= deltaTime;
                    info.Destroy = info.Timer <= 0.0f;
                }

                // ヒット判定
                if (!info.Collision.Tick(info.LayerMask, _workRaycastResults, _workRaycastBuffer)) {
                    continue;
                }

                // 衝突が発生していたら通知する
                hitResult.CustomData = info.CustomData;
                foreach (var result in _workRaycastResults) {
                    hitResult.RaycastHit = result;
                    hitResult.HitCount = ++info.HitCount;
                    info.Listener?.OnHitRaycastCollision(hitResult);
                }
            }
        }
    }
}