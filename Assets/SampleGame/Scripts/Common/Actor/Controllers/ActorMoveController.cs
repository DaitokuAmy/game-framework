using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// アクター移動用クラス
    /// </summary>
    public sealed class ActorMoveController : IDisposable {
        private IMovableActor _actor;
        private IMoveResolver _activeResolver;
        private AsyncOperator _moveAsyncOperator;
        private float _speedMultiplier = 1.0f;

        // 移動処理解決用のインスタンスリスト
        private Dictionary<Type, IMoveResolver> _resolvers = new();

        /// <summary>移動中か</summary>
        public bool IsMoving => _activeResolver != null && _activeResolver.IsMoving;
        /// <summary>現在の移動速度</summary>
        public Vector3 Velocity => _activeResolver?.Velocity ?? Vector3.zero;
        /// <summary>速度係数</summary>
        public float SpeedMultiplier {
            get => _speedMultiplier;
            set => _speedMultiplier = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="actor">移動対象のアクター</param>
        public ActorMoveController(IMovableActor actor) {
            _actor = actor;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            Cancel();
            RemoveResolvers();
        }

        /// <summary>
        /// 移動解決用クラスの追加
        /// </summary>
        public void AddResolver<T>(T resolver)
            where T : IMoveResolver {
            RemoveResolver<T>();
            
            _resolvers[typeof(T)] = resolver;
            resolver.Initialize(_actor);
        }

        /// <summary>
        /// 移動解決用クラスの除外
        /// </summary>
        public void RemoveResolver<T>() {
            var type = typeof(T);
            if (!_resolvers.TryGetValue(type, out var resolver)) {
                return;
            }

            // アクティブなResolverだったらアクティブから外してキャンセル
            if (_activeResolver == resolver) {
                _activeResolver = null;
            }
            
            _resolvers.Remove(type);
            resolver.Cancel();
        }

        /// <summary>
        /// 移動解決用クラスの全除外
        /// </summary>
        public void RemoveResolvers() {
            // アクティブな物はキャンセル
            Cancel();
            
            // Resolverの参照を全てクリア
            _resolvers.Clear();
        }

        /// <summary>
        /// 現在の移動をキャンセル
        /// </summary>
        public void Cancel() {
            if (_activeResolver == null) {
                return;
            }

            var resolver = _activeResolver;
            _activeResolver = null;
            resolver.Cancel();
        }

        /// <summary>
        /// 現在の移動をスキップ
        /// </summary>
        public void Skip() {
            if (_activeResolver == null) {
                return;
            }
            
            var resolver = _activeResolver;
            _activeResolver = null;
            resolver.Skip();
        }

        /// <summary>
        /// 特定向きへの移動
        /// </summary>
        public void MoveDirection<TResolver>(Vector3 direction, float speedMultiplier = 1.0f, bool updateRotation = true)
            where TResolver : IDirectionMoveResolver {
            if (!_resolvers.TryGetValue(typeof(TResolver), out var resolver)) {
                Debug.LogWarning($"Not found resolver. ({typeof(TResolver).Name})");
                return;
            }

            _speedMultiplier = speedMultiplier;

            if (resolver is IDirectionMoveResolver directionMoveResolver) {
                // 現在の移動をキャンセル
                Cancel();
                
                // 移動開始
                directionMoveResolver.MoveDirection(direction, updateRotation);
                    
                // アクティブなリゾルバとして登録
                _activeResolver = resolver;
            }
        }

        /// <summary>
        /// 特定座標への移動
        /// </summary>
        public AsyncOperationHandle MoveToPointAsync<TResolver>(Vector3 point, float speedMultiplier = 1.0f, bool updateRotation = true)
            where TResolver : IPointMoveResolver {
            if (!_resolvers.TryGetValue(typeof(TResolver), out var resolver)) {
                Debug.LogWarning($"Not found resolver. ({typeof(TResolver).Name})");
                return new AsyncOperationHandle();
            }

            _speedMultiplier = speedMultiplier;

            if (resolver is IPointMoveResolver pointMoveResolver) {
                // 現在の移動をキャンセル
                Cancel();
                
                // 定点移動処理開始
                var moveAsyncOperator = new AsyncOperator();
                pointMoveResolver.MoveToPoint(point, updateRotation);

                // すでに到着していた場合
                if (!pointMoveResolver.IsMoving) {
                    moveAsyncOperator.Completed();
                }
                else {
                    // 移動完了を監視
                    pointMoveResolver.OnMoveEndEvent += completed => {
                        if (completed) {
                            moveAsyncOperator.Completed();
                        }
                        else {
                            moveAsyncOperator.Aborted();
                        }
                    };
                    
                    // アクティブなリゾルバとして登録
                    _activeResolver = resolver;
                }

                return moveAsyncOperator;
            }

            return new AsyncOperationHandle();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            if (_activeResolver != null) {
                _activeResolver.Update(deltaTime, SpeedMultiplier);
                if (!_activeResolver.IsMoving) {
                    _activeResolver = null;
                }
            }
        }
    }
}
