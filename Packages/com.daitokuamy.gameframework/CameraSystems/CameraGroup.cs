using System.Linq;
using Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// 仮想カメラ登録用のグループ
    /// </summary>
    public class CameraGroup : MonoBehaviour {
        [SerializeField, Tooltip("登録キー")]
        private string _key = "";
        [SerializeField, Tooltip("仮想カメラが登録されているRootTObject")]
        private GameObject _virtualCameraRoot;
        [SerializeField, Tooltip("ターゲットポイントが登録されているRootObject")]
        private GameObject _targetPointRoot;
        
        /// <summary>初期GameObject名</summary>
        public string DefaultName { get; private set; }

        /// <summary>登録キー</summary>
        public string Key => string.IsNullOrWhiteSpace(_key) ? "Unknown" : _key;
        /// <summary>仮想カメラが登録されているRootObject</summary>
        public GameObject CameraRoot => _virtualCameraRoot != null ? _virtualCameraRoot : gameObject;
        /// <summary>仮想カメラが登録されているRootObject</summary>
        public GameObject TargetPointRoot => _targetPointRoot;

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            DefaultName = name.Replace("(Clone)", "");
        }

        /// <summary>
        /// 保持しているカメラ名のリストを取得
        /// </summary>
        public string[] GetCameraNames() {
            return CameraRoot.GetComponentsInChildren<CinemachineVirtualCameraBase>(true)
                .Where(x => x.ParentCamera == null)
                .Select(x => x.name)
                .ToArray();
        }
    }
}