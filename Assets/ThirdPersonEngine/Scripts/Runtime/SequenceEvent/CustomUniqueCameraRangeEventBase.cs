using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// ユニークカメラ切り替え用のイベント
    /// </summary>
    public abstract class CustomUniqueCameraRangeEventBase : UniqueCameraRangeEventBase {
        [Space]
        [Tooltip("遷移に使うカメラ名")]
        public string cameraName = "";
        [Tooltip("オーナーのCameraGroupKeyを使うか")]
        public bool useOwnerGroupKey;
        [Tooltip("グループキー")]
        public string groupKey = "";
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public abstract class CustomUniqueCameraRangeEventHandlerBase<TEvent> : UniqueCameraRangeEventHandlerBase<TEvent>
        where TEvent : CustomUniqueCameraRangeEventBase {

        /// <summary>
        /// カメラ名の取得
        /// </summary>
        protected override void GetCameraName(TEvent sequenceEvent, out string groupKey, out string cameraName) {
            groupKey = sequenceEvent.useOwnerGroupKey ? OwnerGroupKey : sequenceEvent.groupKey;
            cameraName = sequenceEvent.cameraName;
        }
    }
}