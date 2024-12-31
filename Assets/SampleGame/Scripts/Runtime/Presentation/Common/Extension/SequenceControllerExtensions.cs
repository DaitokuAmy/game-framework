using ActionSequencer;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.VfxSystems;

namespace SampleGame.Presentation {
    /// <summary>
    /// SequenceController用の拡張メソッド定義クラス
    /// </summary>
    public static class SequenceControllerExtensions {
        /// <summary>
        /// Body用カメライベントのバインド
        /// </summary>
        public static void BindBodyCameraEvent(this IReadOnlySequenceController source, Body body, CameraManager cameraManager) {
            source.BindRangeEventHandler<CameraRangeEvent, CameraRangeEventHandler>(handler => { handler.Setup(cameraManager); });
            source.BindRangeEventHandler<MotionCameraRangeEvent, MotionCameraRangeEventHandler>(
                handler => { handler.Setup(cameraManager, body.Transform, body.LayeredTime); });
            source.BindRangeEventHandler<LookAtMotionCameraRangeEvent, LookAtMotionCameraRangeEventHandler>(handler => { handler.Setup(cameraManager, body.Transform, body.LayeredTime); });
            source.BindRangeEventHandler<SplineCameraRangeEvent, SplineCameraRangeEventHandler>(
                handler => { handler.Setup(cameraManager, body.Transform, body.LayeredTime); });
        }

        /// <summary>
        /// Body用エフェクトイベントのバインド
        /// </summary>
        public static void BindBodyEffectEvent(this IReadOnlySequenceController self, Body body, VfxManager vfxManager) {
            self.BindRangeEventHandler<BodyEffectRangeEvent, BodyEffectRangeEventHandler>(handler => handler.Setup(vfxManager, body));
            self.BindSignalEventHandler<BodyEffectSignalEvent, BodyEffectSignalEventHandler>(handler => handler.Setup(vfxManager, body));
        }

        /// <summary>
        /// Body用ギミックイベントのバインド
        /// </summary>
        public static void BindBodyGimmickEvent(this IReadOnlySequenceController self, Body body) {
            var gimmickController = body.GetController<GimmickController>();
            if (gimmickController == null) {
                return;
            }

            self.BindSignalEventHandler<BodyActiveGimmickSingleEvent, BodyActiveGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyAnimationGimmickSingleEvent, BodyAnimationGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyInvokeGimmickSingleEvent, BodyInvokeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyFloatChangeGimmickSingleEvent, BodyFloatChangeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyVectorChangeGimmickSingleEvent, BodyVectorChangeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyColorChangeGimmickSingleEvent, BodyColorChangeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyHdrColorChangeGimmickSingleEvent, BodyHdrColorChangeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyStateGimmickSingleEvent, BodyStateGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));

            self.BindRangeEventHandler<BodyActiveGimmickRangeEvent, BodyActiveGimmickRangeEventHandler>(handler => handler.Setup(gimmickController));
            self.BindRangeEventHandler<BodyAnimationGimmickRangeEvent, BodyAnimationGimmickRangeEventHandler>(handler => handler.Setup(gimmickController));
            self.BindRangeEventHandler<BodyStateGimmickRangeEvent, BodyStateGimmickRangeEventHandler>(handler => handler.Setup(gimmickController));
        }
    }
}