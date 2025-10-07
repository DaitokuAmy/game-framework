using UnityEngine;
#if USE_R3
using System.Threading;
using R3;

#elif USE_UNI_RX
using System;
using UniRx;
#endif

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Body用のRx拡張メソッド
    /// </summary>
    public static class BodyObservables {
#if USE_R3
        /// <summary>
        /// TriggerEnter監視
        /// </summary>
        public static Observable<Collider> TriggerEnterAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collider>(h => source.TriggerEnterEvent += h, h => source.TriggerEnterEvent -= h, ct);
        }

        /// <summary>
        /// TriggerStay監視
        /// </summary>
        public static Observable<Collider> TriggerStayAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collider>(h => source.TriggerStayEvent += h, h => source.TriggerStayEvent -= h, ct);
        }

        /// <summary>
        /// TriggerExit監視
        /// </summary>
        public static Observable<Collider> TriggerExitAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collider>(h => source.TriggerExitEvent += h, h => source.TriggerExitEvent -= h, ct);
        }

        /// <summary>
        /// CollisionEnter監視
        /// </summary>
        public static Observable<Collision> CollisionEnterAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collision>(h => source.CollisionEnterEvent += h, h => source.CollisionEnterEvent -= h, ct);
        }

        /// <summary>
        /// CollisionStay監視
        /// </summary>
        public static Observable<Collision> CollisionStayAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collision>(h => source.CollisionStayEvent += h, h => source.CollisionStayEvent -= h, ct);
        }

        /// <summary>
        /// CollisionExit監視
        /// </summary>
        public static Observable<Collision> CollisionExitAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collision>(h => source.CollisionExitEvent += h, h => source.CollisionExitEvent -= h, ct);
        }

        /// <summary>
        /// ControllerColliderHit監視
        /// </summary>
        public static Observable<ControllerColliderHit> ControllerColliderHitAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<ControllerColliderHit>(h => source.ControllerColliderHitEvent += h, h => source.ControllerColliderHitEvent -= h, ct);
        }

#elif USE_UNI_RX
        /// <summary>
        /// TriggerEnter監視
        /// </summary>
        public static IObservable<Collider> TriggerEnterAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collider>(h => source.TriggerEnterEvent += h, h => source.TriggerEnterEvent -= h);
        }

        /// <summary>
        /// TriggerStay監視
        /// </summary>
        public static IObservable<Collider> TriggerStayAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collider>(h => source.TriggerStayEvent += h, h => source.TriggerStayEvent -= h);
        }

        /// <summary>
        /// TriggerExit監視
        /// </summary>
        public static IObservable<Collider> TriggerExitAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collider>(h => source.TriggerExitEvent += h, h => source.TriggerExitEvent -= h);
        }

        /// <summary>
        /// CollisionEnter監視
        /// </summary>
        public static IObservable<Collision> CollisionEnterAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collision>(h => source.CollisionEnterEvent += h, h => source.CollisionEnterEvent -= h);
        }

        /// <summary>
        /// CollisionStay監視
        /// </summary>
        public static IObservable<Collision> CollisionStayAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collision>(h => source.CollisionStayEvent += h, h => source.CollisionStayEvent -= h);
        }

        /// <summary>
        /// CollisionExit監視
        /// </summary>
        public static IObservable<Collision> CollisionExitAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collision>(h => source.CollisionExitEvent += h, h => source.CollisionExitEvent -= h);
        }

        /// <summary>
        /// ControllerColliderHit監視
        /// </summary>
        public static IObservable<ControllerColliderHit> ControllerColliderHitAsObservable(this ColliderComponent source) {
            return Observable.FromEvent<ControllerColliderHit>(h => source.ControllerColliderHitEvent += h, h => source.ControllerColliderHitEvent -= h, ct);
        }
#endif
    }
}