using System;
using System.Collections.Generic;
using System.Threading;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body(見た目制御クラス)
    /// </summary>
    public class Body : IBody {
        // 解放済みフラグ
        private bool _disposed;
        // キャンセル用
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        // BodyControllerリスト
        private Dictionary<Type, IBodyController> _bodyControllers = new Dictionary<Type, IBodyController>();
        // 並び順に並べられたControllerリスト
        private List<IBodyController> _orderedBodyControllers = new List<IBodyController>();

        // 基本Scale
        private float _baseScale = 1.0f;
        // 変形用Scale
        private Vector3 _deformationScale = Vector3.one;

        // 標準利用されるControllerのキャッシュ
        private LocatorController _locatorController;
        private PropertyController _propertyController;
        private ParentController _parentController;
        private MeshController _meshController;

        /// <summary>キャンセル用トークン</summary>
        public CancellationToken Token => _cancellationTokenSource.Token;

        /// <summary>ユーザー定義ID</summary>
        public object UserId { get; set; } = "";
        /// <summary>有効なBodyか</summary>
        public bool IsValid => GameObject != null;
        /// <summary>有効状態</summary>
        public bool IsActive {
            get => IsValid && GameObject.activeSelf;
            set {
                if (IsValid) {
                    GameObject.SetActive(value);
                }
            }
        }
        /// <summary>表示状態</summary>
        public bool IsVisible {
            get => _meshController.IsVisible;
            set => _meshController.IsVisible = value;
        }
        /// <summary>制御対象のGameObject</summary>
        public GameObject GameObject { get; private set; }
        /// <summary>制御対象のTransform</summary>
        public Transform Transform { get; private set; }
        /// <summary>時間管理クラス</summary>
        public LayeredTime LayeredTime { get; } = new LayeredTime();
        /// <summary>変位時間</summary>
        public float DeltaTime => LayeredTime.DeltaTime;
        
        /// <summary>ローカルのタイムスケール</summary>
        public float LocalTimeScale {
            get => LayeredTime.LocalTimeScale;
            set => LayeredTime.LocalTimeScale = value;
        }
        
        /// <summary>座標</summary>
        public Vector3 Position {
            get => Transform.position;
            set => Transform.position = value;
        }
        /// <summary>座標(ローカル)</summary>
        public Vector3 LocalPosition {
            get => Transform.localPosition;
            set => Transform.localPosition = value;
        }
        /// <summary>姿勢</summary>
        public Quaternion Rotation {
            get => Transform.rotation;
            set => Transform.rotation = value;
        }
        /// <summary>姿勢(ローカル)</summary>
        public Quaternion LocalRotation {
            get => Transform.localRotation;
            set => Transform.localRotation = value;
        }
        /// <summary>基本スケール</summary>
        public float BaseScale {
            get => _baseScale;
            set {
                _baseScale = value;
                Transform.localScale = _baseScale * _deformationScale;
            }
        }
        /// <summary>変形用スケール</summary>
        public Vector3 DeformationScale {
            get => _deformationScale;
            set {
                _deformationScale = value;
                BaseScale = _baseScale;
            }
        }
        /// <summary>Transformのスケール</summary>
        public Vector3 LocalScale {
            get => Transform.localScale;
            set {
                _deformationScale = value;
                BaseScale = 1.0f;
            }
        }

        /// <summary>ロケーター取得</summary>
        public LocatorController Locators => _locatorController;

        /// <summary>スコープ通知用</summary>
        public event Action OnExpired;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Body(GameObject gameObject) {
            GameObject = gameObject;
            Transform = gameObject != null ? gameObject.transform : null;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            OnExpired?.InvokeDescending();
            OnExpired = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            LayeredTime.Dispose();

            // Controllerの削除
            for (var i = _orderedBodyControllers.Count - 1; i >= 0; i--) {
                var controller = _orderedBodyControllers[i];
                controller.Dispose();
            }

            _orderedBodyControllers.Clear();
            _bodyControllers.Clear();

            // GameObjectの削除
            if (GameObject != null) {
                BodyUtility.Destroy(GameObject);
            }

            GameObject = null;
            Transform = null;
        }

        /// <summary>
        /// BodyControllerの取得
        /// </summary>
        public T GetController<T>()
            where T : IBodyController {
            if (_bodyControllers.TryGetValue(typeof(T), out var controller)) {
                return (T)controller;
            }

            return default;
        }

        /// <summary>
        /// Component取得
        /// <param name="type">取得するタイプ</param>
        /// </summary>
        public Component GetComponent(Type type) {
            return GameObject.GetComponent(type);
        }

        /// <summary>
        /// Component取得
        /// </summary>
        public T GetComponent<T>()
            where T : Component {
            return GameObject.GetComponent<T>();
        }

        /// <summary>
        /// Component取得(複数)
        /// </summary>
        /// <param name="type">取得するタイプ</param>
        public Component[] GetComponents(Type type) {
            return GameObject.GetComponents(type);
        }

        /// <summary>
        /// Component取得(複数)
        /// </summary>
        public T[] GetComponents<T>()
            where T : Component {
            return GameObject.GetComponents<T>();
        }

        /// <summary>
        /// Component取得
        /// <param name="type">取得するタイプ</param>
        /// </summary>
        public Component GetComponentInChildren(Type type, bool includeInactive = false) {
            return GameObject.GetComponentInChildren(type, includeInactive);
        }

        /// <summary>
        /// Component取得
        /// </summary>
        public T GetComponentInChildren<T>(bool includeInactive = false)
            where T : Component {
            return GameObject.GetComponentInChildren<T>(includeInactive);
        }

        /// <summary>
        /// Component取得(複数)
        /// </summary>
        /// <param name="type">取得するタイプ</param>
        public Component[] GetComponentsInChildren(Type type, bool includeInactive = false) {
            return GameObject.GetComponentsInChildren(type, includeInactive);
        }

        /// <summary>
        /// Component取得(複数)
        /// </summary>
        public T[] GetComponentsInChildren<T>(bool includeInactive = false)
            where T : Component {
            return GameObject.GetComponentsInChildren<T>(includeInactive);
        }

        /// <summary>
        /// 親の設定
        /// </summary>
        /// <param name="parentBody">親のBody</param>
        /// <param name="targetTransform">追従Transform(nullだとparentのroot)</param>
        /// <param name="offsetPosition">オフセット座標(Local)</param>
        /// <param name="offsetRotation">オフセット回転(Local)</param>
        /// <param name="scaleType">スケール反映タイプ</param>
        public void SetParent(Body parentBody, Transform targetTransform, Vector3 offsetPosition,
            Quaternion offsetRotation,
            ParentController.ScaleType scaleType = ParentController.ScaleType.ParentTransform) {
            _parentController.SetParent(parentBody, targetTransform, offsetPosition, offsetRotation, scaleType);
        }

        public void SetParent(Body parentBody, Vector3 offsetPosition, Quaternion offsetRotation,
            ParentController.ScaleType scaleType = ParentController.ScaleType.ParentTransform) {
            SetParent(parentBody, null, offsetPosition, offsetRotation, scaleType);
        }

        public void SetParent(Body parentBody,
            ParentController.ScaleType scaleType = ParentController.ScaleType.ParentTransform) {
            SetParent(parentBody, null, Vector3.zero, Quaternion.identity, scaleType);
        }

        /// <summary>
        /// Floatパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public float GetFloatProperty(string key, float defaultValue = 0.0f) {
            return _propertyController.GetFloatProperty(key, defaultValue);
        }

        /// <summary>
        /// Intパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public int GetIntProperty(string key, int defaultValue = 0) {
            return _propertyController.GetIntProperty(key, defaultValue);
        }

        /// <summary>
        /// Vectorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public Vector4 GetVectorProperty(string key, Vector4 defaultValue) {
            return _propertyController.GetVectorProperty(key, defaultValue);
        }

        /// <summary>
        /// Vectorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        public Vector4 GetVectorProperty(string key) {
            return GetVectorProperty(key, Vector4.zero);
        }

        /// <summary>
        /// Colorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public Color GetColorProperty(string key, Color defaultValue) {
            return _propertyController.GetColorProperty(key, defaultValue);
        }

        /// <summary>
        /// Colorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        public Color GetColorProperty(string key) {
            return GetColorProperty(key, Color.white);
        }

        /// <summary>
        /// Stringパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public string GetStringProperty(string key, string defaultValue = "") {
            return _propertyController.GetStringProperty(key, defaultValue);
        }

        /// <summary>
        /// Objectパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public UnityEngine.Object GetObjectProperty(string key, UnityEngine.Object defaultValue = null) {
            return _propertyController.GetObjectProperty(key, defaultValue);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IBody.Initialize() {
            _orderedBodyControllers.Sort((a, b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));

            // Controller初期化
            for (var i = 0; i < _orderedBodyControllers.Count; i++) {
                var controller = _orderedBodyControllers[i];
                controller.Initialize(this);
            }

            _locatorController = GetController<LocatorController>();
            _propertyController = GetController<PropertyController>();
            _parentController = GetController<ParentController>();
            _meshController = GetController<MeshController>();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBody.Update(float deltaTime) {
            deltaTime *= LayeredTime.TimeScale;

            // Controller更新
            for (var i = 0; i < _orderedBodyControllers.Count; i++) {
                var controller = _orderedBodyControllers[i];
                controller.Update(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBody.LateUpdate(float deltaTime) {
            deltaTime *= LayeredTime.TimeScale;

            // Controller更新
            for (var i = 0; i < _orderedBodyControllers.Count; i++) {
                var controller = _orderedBodyControllers[i];
                controller.LateUpdate(deltaTime);
            }
        }

        /// <summary>
        /// BodyControllerの追加
        /// </summary>
        /// <param name="controller">対象のController</param>
        void IBody.AddController(IBodyController controller) {
            if (controller == null) {
                return;
            }

            var type = controller.GetType();
            if (_bodyControllers.ContainsKey(type)) {
                Debug.LogError($"Already added body controller type. [{GameObject.name}] > [{type}]");
                return;
            }

            _bodyControllers[type] = controller;
            _orderedBodyControllers.Add(controller);
        }
    }
}