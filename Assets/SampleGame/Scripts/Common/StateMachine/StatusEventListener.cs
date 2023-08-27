using System;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// ステータスイベント監視用リスナー
    /// </summary>
    public class StatusEventListener : MonoBehaviour, IStatusEventListener {
        private Subject<string> _enterSubject = new Subject<string>();
        private Subject<Tuple<string, int>> _cycleSubject = new Subject<Tuple<string, int>>();
        private Subject<string> _exitSubject = new Subject<string>();

        public IObservable<string> EnterSubject => _enterSubject;
        [CanBeNull]
        public IObservable<Tuple<string, int>> CycleSubject => _cycleSubject;
        public IObservable<string> ExitSubject => _exitSubject;

        /// <summary>
        /// ステータスに入った時
        /// </summary>
        void IStatusEventListener.OnStatusEnter(string statusName) {
            _enterSubject.OnNext(statusName);
        }

        /// <summary>
        /// ステータスのループ回数変化時
        /// </summary>
        void IStatusEventListener.OnStatusCycle(string statusName, int cycle) {
            _cycleSubject.OnNext(new Tuple<string, int>(statusName, cycle));
        }

        /// <summary>
        /// ステータスを抜けた時
        /// </summary>
        void IStatusEventListener.OnStatusExit(string statusName) {
            _exitSubject.OnNext(statusName);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            _enterSubject.OnCompleted();
            _cycleSubject.OnCompleted();
            _exitSubject.OnCompleted();
        }
    }
}