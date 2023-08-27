using System;
using GameFramework.TaskSystems;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame {
    /// <summary>
    /// フェード制御クラス
    /// </summary>
    public class FadeController : MonoBehaviour, ITask {
        [SerializeField, Tooltip("制御対象のImage")]
        private Image _image;

        private Subject<Unit> _finishSubject = new Subject<Unit>();
        private float _targetAlpha;
        private float _timer;

        bool ITask.IsActive => true;

        /// <summary>
        /// フェードアウト
        /// </summary>
        public IObservable<Unit> FadeOutAsync(Color color, float duration) {
            return Observable.Defer(() => {
                _finishSubject.OnNext(Unit.Default);

                SetImageColor(color);
                _targetAlpha = 1.0f;
                _timer = duration;
                return _finishSubject.First();
            });
        }

        /// <summary>
        /// フェードイン
        /// </summary>
        public IObservable<Unit> FadeInAsync(float duration) {
            return Observable.Defer(() => {
                _finishSubject.OnNext(Unit.Default);

                _targetAlpha = 0.0f;
                _timer = duration;
                return _finishSubject.First();
            });
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ITask.Update() {
            var deltaTime = Time.deltaTime;
            if (_timer < 0.0f || deltaTime <= float.Epsilon) {
                return;
            }

            _timer -= Time.deltaTime;
            var ratio = _timer <= float.Epsilon ? 1.0f : Mathf.Min(1.0f, deltaTime / _timer);
            var color = _image.color;
            color.a = Mathf.Lerp(color.a, _targetAlpha, ratio);
            _image.color = color;

            // 終了通知
            if (_timer < 0.0f) {
                _finishSubject.OnNext(Unit.Default);
            }
        }

        /// <summary>
        /// α値を除く色をImageに設定する
        /// </summary>
        private void SetImageColor(Color color) {
            var alpha = _image.color.a;
            color.a = alpha;
            _image.color = color;
        }
    }
}