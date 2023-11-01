using UnityEngine;
using UnityEngine.Rendering;

namespace GameFramework.Core {
    // <summary>
    // 軸を固定するためのコンポーネント
    // </summary>
    [DisallowMultipleComponent, ExecuteAlways]
    public class AxisFixer : MonoBehaviour {
        /// <summary>
        /// 固定軸タイプ
        /// </summary>
        private enum FixedAxisType {
            AxisX,
            AxisY,
            AxisZ,
        }

        [SerializeField, Tooltip("固定する軸タイプ")]
        private FixedAxisType _fixedAxisType = FixedAxisType.AxisY;
        
        // 自身のトランスフォームのキャッシュ
        private Transform _cachedTransform;
        // 向き変更前の姿勢
        private Quaternion _prevRotation;

        // <summary>
        // 生成時処理
        // </summary>
        private void Awake() {
            _cachedTransform = transform;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }
        
        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        /// <summary>
        /// 描画直前処理
        /// </summary>
        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera targetCamera) {
            // 現在の姿勢を記憶
            _prevRotation = _cachedTransform.rotation;

            var rotation = _prevRotation;
            switch (_fixedAxisType) {
                case FixedAxisType.AxisX:
                    rotation = Quaternion.FromToRotation(rotation * Vector3.right, Vector3.right) * rotation;
                    break;
                case FixedAxisType.AxisY:
                    rotation = Quaternion.FromToRotation(rotation * Vector3.up, Vector3.up) * rotation;
                    break;
                case FixedAxisType.AxisZ:
                    rotation = Quaternion.FromToRotation(rotation * Vector3.forward, Vector3.forward) * rotation;
                    break;
            }

            _cachedTransform.rotation = rotation;
        }

        /// <summary>
        /// 描画直後処理
        /// </summary>
        private void OnEndCameraRendering(ScriptableRenderContext context, Camera targetCamera) {
            // 姿勢を戻す
            _cachedTransform.rotation = _prevRotation;
        }
    }
}
