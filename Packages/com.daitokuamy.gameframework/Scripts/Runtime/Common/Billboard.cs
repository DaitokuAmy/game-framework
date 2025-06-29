using UnityEngine;
using UnityEngine.Rendering;

namespace GameFramework {
    // <summary>
    // Billboard
    // </summary>
    [DisallowMultipleComponent, ExecuteAlways]
    public class Billboard : MonoBehaviour {
        /// <summary>
        /// 固定軸タイプ
        /// </summary>
        private enum FixedAxisType {
            None,
            AxisX,
            AxisY
        }

        /// <summary>
        /// 合わせる軸向き
        /// </summary>
        private enum DirectionType {
            PlusX,
            PlusY,
            PlusZ,
            MinusX,
            MinusY,
            MinusZ
        }

        [SerializeField, Tooltip("軸固定")]
        private FixedAxisType _fixedAxisType = FixedAxisType.None;
        [SerializeField, Tooltip("向ける軸方向")]
        private DirectionType _directionType = DirectionType.PlusZ;
        [SerializeField, Tooltip("オフセット角度")]
        private Vector3 _offsetAngles = Vector3.zero;

        // 軸向き変更用Vector
        private Vector3 _direction;
        // 軸向き変更用Quaternion
        private Quaternion _directionRotation;
        // オフセット回転用Quaternion
        private Quaternion _offsetRotation;
        // 自身のトランスフォームのキャッシュ
        private Transform _cachedTransform;
        // 計算用の事前パラメータ更新フラグ
        private bool _dirtyAdvanceParameter;

        // <summary>
        // 生成時処理
        // </summary>
        private void Awake() {
            _cachedTransform = transform;
            _dirtyAdvanceParameter = true;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }
        
        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        /// <summary>
        /// 描画直前処理
        /// </summary>
        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera targetCamera) {
            // 事前計算情報の更新
            if (_dirtyAdvanceParameter) {
                _dirtyAdvanceParameter = false;
                RefreshAdvanceParameters();
            }
            
            // カメラの方向に向けるビルボード処理
            var lookDirection = targetCamera.transform.position - _cachedTransform.position;

            switch (_fixedAxisType) {
                case FixedAxisType.None:
                    break;
                case FixedAxisType.AxisX:
                    var forward = _direction;
                    var forwardXZ = new Vector2(forward.x, forward.z);
                    lookDirection.Normalize();
                    forwardXZ = forwardXZ.normalized * Mathf.Sqrt(1.0f - lookDirection.y * lookDirection.y);
                    lookDirection.x = forwardXZ.x;
                    lookDirection.z = forwardXZ.y;
                    break;
                case FixedAxisType.AxisY:
                    lookDirection.y = 0.0f;
                    break;
            }

            if (lookDirection.sqrMagnitude > 0.0001f) {
                if (_fixedAxisType == FixedAxisType.AxisX) {
                    _cachedTransform.rotation = _offsetRotation * Quaternion.LookRotation(lookDirection) * _directionRotation;
                }
                else {
                    _cachedTransform.rotation = Quaternion.LookRotation(lookDirection) * _directionRotation * _offsetRotation;
                }
            }
        }

        /// <summary>
        /// パラメータ変化時処理
        /// </summary>
        private void OnValidate() {
            _dirtyAdvanceParameter = true;
        }

        /// <summary>
        /// 事前パラメータの更新
        /// </summary>
        private void RefreshAdvanceParameters() {
            _direction = Vector3.forward;
            switch (_directionType) {
                case DirectionType.PlusX:
                    _direction = Vector3.right;
                    break;
                case DirectionType.PlusY:
                    _direction = Vector3.up;
                    break;
                case DirectionType.PlusZ:
                    _direction = Vector3.forward;
                    break;
                case DirectionType.MinusX:
                    _direction = Vector3.left;
                    break;
                case DirectionType.MinusY:
                    _direction = Vector3.down;
                    break;
                case DirectionType.MinusZ:
                    _direction = Vector3.back;
                    break;
            }
            
            _directionRotation = Quaternion.Inverse(Quaternion.LookRotation(_direction));
            _offsetRotation = Quaternion.Euler(_offsetAngles);
        }
    }
}
