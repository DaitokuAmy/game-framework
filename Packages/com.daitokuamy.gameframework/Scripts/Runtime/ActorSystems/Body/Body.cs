using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using GameFramework.Core;
#if USE_ANIMATION_RIGGING
using UnityEngine.Animations.Rigging;
#endif

namespace GameFramework.ActorSystems {
    /// <summary>
    /// GameObjectを制御するためのクラス
    /// </summary>
    public sealed class Body : DisposableLateUpdatableTask {
        private readonly Dictionary<Type, IBodyComponent> _componentDict = new(32);
        private readonly List<IBodyComponent> _components = new(32);
        private readonly List<IBodyComponent> _standbyComponents = new(32);

        private bool _disposed;
        private float _baseScale = 1.0f;
        private Vector3 _deformationScale = Vector3.one;

        private MeshComponent _meshComponent;
        private LocatorComponent _locatorComponent;

        /// <summary>Taskが有効状態か</summary>
        protected override bool IsTaskActive => IsActive && GameObject.activeInHierarchy;

        /// <summary>アクティブ状態</summary>
        public bool IsActive => GameObject.activeSelf;
        /// <summary>表示状態</summary>
        public bool IsVisible {
            get => _meshComponent.IsVisible;
            set => _meshComponent.IsVisible = value;
        }
        /// <summary>ロケーター情報</summary>
        public LocatorComponent Locators => _locatorComponent;
        /// <summary>LayeredTime</summary>
        public LayeredTime LayeredTime { get; }
        /// <summary>GameObject参照</summary>
        public GameObject GameObject { get; private set; }
        /// <summary>Transform参照</summary>
        public Transform Transform => GameObject.transform;
        /// <summary>Body内のDeltaTime</summary>
        public float DeltaTime => LayeredTime?.DeltaTime ?? Time.deltaTime;
        /// <summary>Body内のTimeScale</summary>
        public float TimeScale => LayeredTime?.TimeScale ?? Time.timeScale;
        /// <summary>カスタムデータ</summary>
        public object CustomData { get; set; } = null;

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
        /// <summary>向き</summary>
        public Vector3 EulerAngles {
            get => Transform.eulerAngles;
            set => Transform.eulerAngles = value;
        }
        /// <summary>向き(ローカル)</summary>
        public Vector3 LocalEulerAngles {
            get => Transform.localEulerAngles;
            set => Transform.localEulerAngles = value;
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Body(GameObject gameObject, IBodyBuilder builder = null) {
            GameObject = gameObject;
            GameObject.SetActive(true);
            LayeredTime = new LayeredTime();

            _componentDict.Clear();

            // 含まれているBodyComponentのリストを取得して辞書登録
            var components = gameObject.GetComponentsInChildren<IBodyComponent>(true);
            foreach (var component in components) {
                _componentDict.Add(component.GetType(), component);
                _standbyComponents.Add(component);
            }

            // デフォルトのコンポーネントを追加
            CreateDefaultComponents();

            // builder指定されていたらそこでも構築
            if (builder != null) {
                builder.Build(this, GameObject);
            }

            // Dispatcherの生成
            var dispatcher = gameObject.AddComponent<BodyDispatcher>();
            dispatcher.Initialize(this);

            // Componentリストのリフレッシュ
            RefreshComponents();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            foreach (var component in _components) {
                component.Dispose();
            }

            _components.Clear();
            _componentDict.Clear();
            LayeredTime.Dispose();
            Object.Destroy(GameObject);
            GameObject = null;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            RefreshComponents();

            var deltaTime = LayeredTime.DeltaTime;
            for (var i = 0; i < _components.Count; i++) {
                var component = _components[i];
                component.Update(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            base.LateUpdateInternal();

            RefreshComponents();

            var deltaTime = LayeredTime.DeltaTime;
            for (var i = 0; i < _components.Count; i++) {
                var component = _components[i];
                component.LateUpdate(deltaTime);
            }
        }

        /// <summary>
        /// アクティブ状態の設定
        /// </summary>
        public void SetActive(bool active) {
            GameObject.SetActive(active);
        }

        /// <summary>
        /// Componentの取得
        /// </summary>
        public TComponent GetComponent<TComponent>()
            where TComponent : Component {
            return GameObject.GetComponent<TComponent>();
        }

        /// <summary>
        /// Componentの取得
        /// </summary>
        public TComponent GetComponentInChildren<TComponent>(bool includeInactive = false)
            where TComponent : Component {
            return GameObject.GetComponentInChildren<TComponent>(includeInactive);
        }

        /// <summary>
        /// Componentの取得
        /// </summary>
        public TComponent[] GetComponentsInChildren<TComponent>(bool includeInactive = false)
            where TComponent : Component {
            return GameObject.GetComponentsInChildren<TComponent>(includeInactive);
        }

        /// <summary>
        /// BodyComponentの取得
        /// </summary>
        public TBodyComponent GetBodyComponent<TBodyComponent>()
            where TBodyComponent : IBodyComponent {
            var searchType = typeof(TBodyComponent);
            if (!_componentDict.TryGetValue(searchType, out var component)) {
                // 見つからなければ継承関係を探す
                foreach (var c in _components) {
                    if (searchType.IsAssignableFrom(c.GetType())) {
                        return (TBodyComponent)c;
                    }
                }

                return default;
            }

            return (TBodyComponent)component;
        }

        /// <summary>
        /// BodyComponentが追加済みかチェック
        /// </summary>
        public bool ContainsBodyComponent(Type type) {
            return _componentDict.ContainsKey(type);
        }

        /// <summary>
        /// BodyComponentが追加済みかチェック
        /// </summary>
        public bool ContainsBodyComponent<TBodyComponent>()
            where TBodyComponent : IBodyComponent {
            return _componentDict.ContainsKey(typeof(TBodyComponent));
        }

        /// <summary>
        /// BodyComponentの追加
        /// </summary>
        public void AddBodyComponent<TBodyComponent>(TBodyComponent component)
            where TBodyComponent : BodyComponent {
            var type = typeof(TBodyComponent);
            if (!_componentDict.TryAdd(type, component)) {
                throw new InvalidOperationException($"Body component {type} has already been added.");
            }

            _standbyComponents.Add(component);
        }

        /// <summary>
        /// シリアライズ可能なBodyComponentの追加
        /// </summary>
        public void AddSerializedBodyComponent<TBodyComponent>()
            where TBodyComponent : SerializedBodyComponent {
            var type = typeof(TBodyComponent);
            if (_componentDict.ContainsKey(type)) {
                throw new InvalidOperationException($"Body component {type} has already been added.");
            }

            var component = GameObject.AddComponent<TBodyComponent>();
            _componentDict.Add(type, component);
            _standbyComponents.Add(component);
        }

        /// <summary>
        /// Componentリストのリフレッシュ
        /// </summary>
        private void RefreshComponents() {
            if (_standbyComponents.Count <= 0) {
                return;
            }

            int Comparision(IBodyComponent a, IBodyComponent b) {
                return a.ExecutionOrder.CompareTo(b.ExecutionOrder);
            }

            _standbyComponents.Sort(Comparision);
            for (var i = 0; i < _standbyComponents.Count; i++) {
                _standbyComponents[i].Initialize(this);
            }

            _components.AddRange(_standbyComponents);
            _components.Sort(Comparision);
            _standbyComponents.Clear();
        }

        /// <summary>
        /// デフォルト設定すべきComponentを生成
        /// </summary>
        private void CreateDefaultComponents() {
            void TryAddComponent<T>()
                where T : SerializedBodyComponent {
                if (GameObject.GetComponent<T>() != null) {
                    return;
                }

                AddSerializedBodyComponent<T>();
            }

            AddBodyComponent(_locatorComponent = new LocatorComponent());
            AddBodyComponent(new PropertyComponent());
            AddBodyComponent(_meshComponent = new MeshComponent());
            AddBodyComponent(new MaterialComponent());
            AddBodyComponent(new AttachmentComponent());
            AddBodyComponent(new GimmickComponent());

#if USE_ANIMATION_RIGGING
            // RigBuilderがついている場合、RigController追加
            if (GameObject.GetComponent<RigBuilder>() != null) {
                AddBodyComponent(new RigComponent());
            }
#endif

            // Rigidbody or ColliderPartsがついている場合、ColliderController追加
            if (GameObject.GetComponentInChildren<ColliderParts>() != null || GameObject.GetComponent<Rigidbody>() != null) {
                TryAddComponent<ColliderComponent>();
            }

            // GizmoDispatcherを追加
            if (GameObject.GetComponent<GizmoDispatcher>() == null) {
                GameObject.AddComponent<GizmoDispatcher>();
            }
        }
    }
}