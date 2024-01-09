using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameFramework.UISystems {
    /// <summary>
    /// フリック検出用クラス
    /// </summary>
    public class FlickDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        /// <summary>
        /// 記録用情報
        /// </summary>
        private struct RecordInfo {
            public float deltaTime;
            public Vector2 position;
        }

        [SerializeField, Tooltip("フリック判定速度の閾値")]
        private float _flickSpeedThreshold = 0.5f;
        [SerializeField, Tooltip("フリック判定最小距離")]
        private float _minFlickDistance = 50.0f;
        [SerializeField, Tooltip("フリック判定に使う最大秒数")]
        private float _maxFlickDuration = 0.2f;
        
        private readonly List<RecordInfo> _recordInfos = new();

        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private float _prevTime;
        private float _recordTime;

        /// <summary>フリック通知(正規化された向きベクトル)</summary>
        public event Action<Vector2> OnFlickEvent;

        /// <summary>
        /// タッチダウン通知
        /// </summary>
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            _startPosition = eventData.position;
            _recordTime = 0.0f;
            _prevTime = Time.realtimeSinceStartup;
            _recordInfos.Clear();
        }

        /// <summary>
        /// タッチアップ通知
        /// </summary>
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            var currentTime = Time.realtimeSinceStartup;
            var deltaTime = currentTime - _prevTime;
            _endPosition = eventData.position;
            _recordTime += deltaTime;
            UpdateRecordInfos(eventData, deltaTime);
            _prevTime = currentTime;
            
            DetectFlick();
            _recordTime = 0.0f;
            _recordInfos.Clear();
        }

        /// <summary>
        /// ドラッグ通知
        /// </summary>
        void IDragHandler.OnDrag(PointerEventData eventData) {
            var currentTime = Time.realtimeSinceStartup;
            var deltaTime = currentTime - _prevTime;
            _endPosition = eventData.position;
            _recordTime += deltaTime;
            UpdateRecordInfos(eventData, deltaTime);
            _prevTime = currentTime;
        }

        /// <summary>
        /// Record情報の更新
        /// </summary>
        private void UpdateRecordInfos(PointerEventData eventData, float deltaTime) {
            _recordInfos.Add(new RecordInfo {
                deltaTime = deltaTime,
                position = eventData.position
            });
            
            // 記録最大時間を超えていたらバッファを戻す
            while (_recordTime > _maxFlickDuration && _recordInfos.Count > 0) {
                var info = _recordInfos[0];
                _recordTime -= info.deltaTime;
                _startPosition = info.position;
                _recordInfos.RemoveAt(0);
            }
        }

        /// <summary>
        /// フリックの検出
        /// </summary>
        private void DetectFlick() {
            var flickVector = _endPosition - _startPosition;
            var flickDistance = flickVector.magnitude;
            if (flickDistance <= float.Epsilon || _recordTime <= float.Epsilon) {
                return;
            }
            
            var flickSpeed = flickDistance / _recordTime;
            if (flickSpeed >= _flickSpeedThreshold && flickDistance >= _minFlickDistance) {
                OnFlickEvent?.Invoke(flickVector / flickDistance);
            }
        }
    }
}