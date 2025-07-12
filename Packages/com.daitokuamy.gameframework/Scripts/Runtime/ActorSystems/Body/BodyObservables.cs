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
        public static Observable<Collider> OnTriggerEnterAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collider>(h => source.OnTriggerEnterEvent += h, h => source.OnTriggerEnterEvent -= h, ct);
        }

        /// <summary>
        /// TriggerStay監視
        /// </summary>
        public static Observable<Collider> OnTriggerStayAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collider>(h => source.OnTriggerStayEvent += h, h => source.OnTriggerStayEvent -= h, ct);
        }

        /// <summary>
        /// TriggerExit監視
        /// </summary>
        public static Observable<Collider> OnTriggerExitAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collider>(h => source.OnTriggerExitEvent += h, h => source.OnTriggerExitEvent -= h, ct);
        }

        /// <summary>
        /// CollisionEnter監視
        /// </summary>
        public static Observable<Collision> OnCollisionEnterAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collision>(h => source.OnCollisionEnterEvent += h, h => source.OnCollisionEnterEvent -= h, ct);
        }

        /// <summary>
        /// CollisionStay監視
        /// </summary>
        public static Observable<Collision> OnCollisionStayAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collision>(h => source.OnCollisionStayEvent += h, h => source.OnCollisionStayEvent -= h, ct);
        }

        /// <summary>
        /// CollisionExit監視
        /// </summary>
        public static Observable<Collision> OnCollisionExitAsObservable(this ColliderComponent source, CancellationToken ct = default) {
            return Observable.FromEvent<Collision>(h => source.OnCollisionExitEvent += h, h => source.OnCollisionExitEvent -= h, ct);
        }

#elif USE_UNI_RX
        /// <summary>
        /// TriggerEnter監視
        /// </summary>
        public static IObservable<Collider> OnTriggerEnterAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collider>(h => source.OnTriggerEnterEvent += h, h => source.OnTriggerEnterEvent -= h);
        }

        /// <summary>
        /// TriggerStay監視
        /// </summary>
        public static IObservable<Collider> OnTriggerStayAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collider>(h => source.OnTriggerStayEvent += h, h => source.OnTriggerStayEvent -= h);
        }

        /// <summary>
        /// TriggerExit監視
        /// </summary>
        public static IObservable<Collider> OnTriggerExitAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collider>(h => source.OnTriggerExitEvent += h, h => source.OnTriggerExitEvent -= h);
        }

        /// <summary>
        /// CollisionEnter監視
        /// </summary>
        public static IObservable<Collision> OnCollisionEnterAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collision>(h => source.OnCollisionEnterEvent += h, h => source.OnCollisionEnterEvent -= h);
        }

        /// <summary>
        /// CollisionStay監視
        /// </summary>
        public static IObservable<Collision> OnCollisionStayAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collision>(h => source.OnCollisionStayEvent += h, h => source.OnCollisionStayEvent -= h);
        }

        /// <summary>
        /// CollisionExit監視
        /// </summary>
        public static IObservable<Collision> OnCollisionExitAsObservable(this ColliderController source) {
            return Observable.FromEvent<Collision>(h => source.OnCollisionExitEvent += h, h => source.OnCollisionExitEvent -= h);
        }
#endif
    }
}