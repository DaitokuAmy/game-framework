using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// カメラ管理クラス
    /// </summary>
    public class CameraManager : LateUpdatableTaskBehaviour, IDisposable {
        public const string MainCameraGroupName = "Main";
        
        /// <summary>
        /// カメラブレンド情報
        /// </summary>
        private class CameraBlend {
            public CinemachineBlendDefinition BlendDefinition { get; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CameraBlend(CinemachineBlendDefinition blendDefinition) {
                BlendDefinition = blendDefinition;
            }
        }

        /// <summary>
        /// カメラハンドリング用クラス
        /// </summary>
        private class CameraHandler : IDisposable {
            private int _activateCount;
            private int _overridePriority;

            public string Name { get; }
            public ICameraComponent Component { get; }
            public ICameraController Controller { get; private set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CameraHandler(string cameraName, ICameraComponent cameraComponent) {
                Name = cameraName;
                Component = cameraComponent;
                _overridePriority = int.MinValue;
                ApplyActiveStatus();
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                SetController(null);
                if (Component != null) {
                    Component.Dispose();
                }
            }

            /// <summary>
            /// アクティブ状態か
            /// </summary>
            public bool CheckActivate() {
                return _activateCount > 0;
            }

            /// <summary>
            /// アクティブ化
            /// </summary>
            public void Activate(bool force = false, int overridePriority = int.MinValue) {
                if (force) {
                    _activateCount = 1;
                }
                else {
                    _activateCount++;
                }
                
                if (_activateCount > 0 && overridePriority > _overridePriority) {
                    _overridePriority = overridePriority;
                    Component.SetPriority(_overridePriority);
                }

                ApplyActiveStatus();
            }

            /// <summary>
            /// 非アクティブ化
            /// </summary>
            public void Deactivate(bool force = false) {
                if (force) {
                    _activateCount = 0;
                }
                else {
                    _activateCount--;
                }

                if (_activateCount < 0) {
                    Debug.LogWarning($"activateCount is minus. [{Name}]");
                    _activateCount = 0;
                }

                if (_activateCount == 0) {
                    Component.ResetPriority();
                }

                ApplyActiveStatus();
            }

            /// <summary>
            /// コントローラーの設定
            /// </summary>
            public void SetController(ICameraController controller) {
                if (Controller != null) {
                    Controller.Dispose();
                    Controller = null;
                }

                Controller = controller;
                if (Controller != null) {
                    Controller.Initialize(Component);
                    if (Component.IsActive) {
                        controller.Activate();
                    }
                }
            }

            /// <summary>
            /// アクティブ状態の反映
            /// </summary>
            private void ApplyActiveStatus() {
                var active = _activateCount > 0;
                if (active != Component.IsActive) {
                    if (active) {
                        Component.Activate();
                        Controller?.Activate();
                    }
                    else {
                        Component.Deactivate();
                        Controller?.Deactivate();
                    }
                }
            }
        }

        /// <summary>
        /// カメラグループ情報
        /// </summary>
        private class CameraGroupInfo {
            public GameObject prefab;
            public CameraGroup cameraGroup;
            public Dictionary<string, CameraHandler> handlers = new();
            public Dictionary<string, Transform> targetPoints = new();
        }

        [SerializeField, Tooltip("仮想カメラ用のBrain")]
        private CinemachineBrain _brain;
        [SerializeField, Tooltip("メインで扱うカメラグループ")]
        private CameraGroup _mainCameraGroup;
        [SerializeField, Tooltip("メインで扱うカメラグループのPrefab(こちらの指定が優先)")]
        private GameObject _mainCameraGroupPrefab;
        // [SerializeField, Tooltip("仮想カメラを配置しているRootObject")]
        // private GameObject _virtualCameraRoot;
        // [SerializeField, Tooltip("仮想カメラの基準Transformを配置しているRootObject")]
        // private GameObject _targetPointRoot;
        [SerializeField, Tooltip("デフォルトで使用するカメラ名")]
        private string _defaultCameraName = "Default";

        // 初期化済みフラグ
        private bool _initialized;
        // 廃棄済みフラグ
        private bool _disposed;
        // カメラグループのルートオブジェクト
        private GameObject _cameraGroupRoot;
        // Brainの初期状態のUpdateMethod
        private CinemachineBrain.UpdateMethod _defaultUpdateMethod;
        // カメラハンドリング用情報
        private Dictionary<string, CameraHandler> _cameraHandlers = new();
        // 基準Transform
        private Dictionary<string, Transform> _targetPoints = new();

        // カメラグループ情報
        private Dictionary<string, CameraGroupInfo> _cameraGroupInfos = new();

        // カメラブレンド情報
        private Dictionary<ICinemachineCamera, CameraBlend> _toCameraBlends = new();
        private Dictionary<ICinemachineCamera, CameraBlend> _fromCameraBlends = new();

        /// <summary>出力カメラ</summary>
        public Camera OutputCamera => _brain != null ? _brain.OutputCamera : null;
        /// <summary>更新時間コントロール用</summary>
        public LayeredTime LayeredTime { get; private set; } = new LayeredTime();

        /// <summary>
        /// カメラのアクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">アクティブ化するカメラ名</param>
        /// <param name="blendDefinition">上書き用ブレンド設定</param>
        /// <param name="overridePriority">上書き用プライオリティ</param>
        public void Activate(string groupKey, string cameraName, CinemachineBlendDefinition blendDefinition, int overridePriority = int.MinValue) {
            Initialize();
            ActivateInternal(groupKey, cameraName, new CameraBlend(blendDefinition), false, overridePriority);
        }

        /// <summary>
        /// カメラのアクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="cameraName">アクティブ化するカメラ名</param>
        /// <param name="blendDefinition">上書き用ブレンド設定</param>
        /// <param name="overridePriority">上書き用プライオリティ</param>
        public void Activate(string cameraName, CinemachineBlendDefinition blendDefinition, int overridePriority = int.MinValue) {
            Activate(MainCameraGroupName, cameraName, blendDefinition, overridePriority);
        }

        /// <summary>
        /// カメラのアクティブ化(参照カウンタなし)
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">アクティブ化するカメラ名</param>
        /// <param name="blendDefinition">上書き用ブレンド設定</param>
        /// <param name="overridePriority">上書き用プライオリティ</param>
        public void ForceActivate(string groupKey, string cameraName, CinemachineBlendDefinition blendDefinition, int overridePriority = int.MinValue) {
            Initialize();
            ActivateInternal(groupKey, cameraName, new CameraBlend(blendDefinition), true, overridePriority);
        }

        /// <summary>
        /// カメラのアクティブ化(参照カウンタなし)
        /// </summary>
        /// <param name="cameraName">アクティブ化するカメラ名</param>
        /// <param name="blendDefinition">上書き用ブレンド設定</param>
        /// <param name="overridePriority">上書き用プライオリティ</param>
        public void ForceActivate(string cameraName, CinemachineBlendDefinition blendDefinition, int overridePriority = int.MinValue) {
            ForceActivate(MainCameraGroupName, cameraName, blendDefinition, overridePriority);
        }

        /// <summary>
        /// カメラのアクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">アクティブ化するカメラ名</param>
        /// <param name="overridePriority">上書き用プライオリティ</param>
        public void Activate(string groupKey, string cameraName, int overridePriority = int.MinValue) {
            Initialize();
            ActivateInternal(groupKey, cameraName, null, false, overridePriority);
        }

        /// <summary>
        /// カメラのアクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="cameraName">アクティブ化するカメラ名</param>
        /// <param name="overridePriority">上書き用プライオリティ</param>
        public void Activate(string cameraName, int overridePriority = int.MinValue) {
            Activate(MainCameraGroupName, cameraName, overridePriority);
        }

        /// <summary>
        /// カメラのアクティブ化(参照カウンタなし)
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">アクティブ化するカメラ名</param>
        /// <param name="overridePriority">上書き用プライオリティ</param>
        public void ForceActivate(string groupKey, string cameraName, int overridePriority = int.MinValue) {
            Initialize();
            ActivateInternal(groupKey, cameraName, null, true, overridePriority);
        }

        /// <summary>
        /// カメラのアクティブ化(参照カウンタなし)
        /// </summary>
        /// <param name="cameraName">アクティブ化するカメラ名</param>
        /// <param name="overridePriority">上書き用プライオリティ</param>
        public void ForceActivate(string cameraName, int overridePriority = int.MinValue) {
            ForceActivate(MainCameraGroupName, cameraName, overridePriority);
        }

        /// <summary>
        /// カメラの非アクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">非アクティブ化するカメラ名</param>
        /// <param name="blendDefinition">上書き用ブレンド設定</param>
        public void Deactivate(string groupKey, string cameraName, CinemachineBlendDefinition blendDefinition) {
            Initialize();
            DeactivateInternal(groupKey, cameraName, new CameraBlend(blendDefinition), false);
        }

        /// <summary>
        /// カメラの非アクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="cameraName">非アクティブ化するカメラ名</param>
        /// <param name="blendDefinition">上書き用ブレンド設定</param>
        public void Deactivate(string cameraName, CinemachineBlendDefinition blendDefinition) {
            Deactivate(MainCameraGroupName, cameraName, blendDefinition);
        }

        /// <summary>
        /// カメラの非アクティブ化(参照カウンタなし)
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">非アクティブ化するカメラ名</param>
        /// <param name="blendDefinition">上書き用ブレンド設定</param>
        public void ForceDeactivate(string groupKey, string cameraName, CinemachineBlendDefinition blendDefinition) {
            Initialize();
            DeactivateInternal(groupKey, cameraName, new CameraBlend(blendDefinition), true);
        }

        /// <summary>
        /// カメラの非アクティブ化(参照カウンタなし)
        /// </summary>
        /// <param name="cameraName">非アクティブ化するカメラ名</param>
        /// <param name="blendDefinition">上書き用ブレンド設定</param>
        public void ForceDeactivate(string cameraName, CinemachineBlendDefinition blendDefinition) {
            ForceDeactivate(MainCameraGroupName, cameraName, blendDefinition);
        }

        /// <summary>
        /// カメラの非アクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">非アクティブ化するカメラ名</param>
        public void Deactivate(string groupKey, string cameraName) {
            Initialize();
            DeactivateInternal(groupKey, cameraName, null, false);
        }

        /// <summary>
        /// カメラの非アクティブ化(参照カウンタ有)
        /// </summary>
        /// <param name="cameraName">非アクティブ化するカメラ名</param>
        public void Deactivate(string cameraName) {
            Deactivate(MainCameraGroupName, cameraName);
        }

        /// <summary>
        /// カメラの非アクティブ化(参照カウンタなし)
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">非アクティブ化するカメラ名</param>
        public void ForceDeactivate(string groupKey, string cameraName) {
            Initialize();
            DeactivateInternal(groupKey, cameraName, null, true);
        }

        /// <summary>
        /// カメラの非アクティブ化(参照カウンタなし)
        /// </summary>
        /// <param name="cameraName">非アクティブ化するカメラ名</param>
        public void ForceDeactivate(string cameraName) {
            ForceDeactivate(MainCameraGroupName, cameraName);
        }

        /// <summary>
        /// カメラのアクティブ状態をチェック
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">カメラ名</param>
        public bool CheckActivate(string groupKey, string cameraName) {
            Initialize();

            var handlers = GetCameraHandlers(groupKey);
            if (!handlers.TryGetValue(cameraName, out var handler)) {
                return false;
            }

            return handler.CheckActivate();
        }

        /// <summary>
        /// カメラのアクティブ状態をチェック
        /// </summary>
        /// <param name="cameraName">カメラ名</param>
        public bool CheckActivate(string cameraName) {
            return CheckActivate(MainCameraGroupName, cameraName);
        }

        /// <summary>
        /// カメラ名の一覧を取得
        /// </summary>
        public string[] GetCameraNames(string groupKey = null) {
            var handlers = GetCameraHandlers(groupKey);
            return handlers.Keys.ToArray();
        }

        /// <summary>
        /// CameraGroupの登録
        /// </summary>
        /// <param name="cameraGroup">登録するCameraGroup(Prefabではない)</param>
        /// <param name="overrideGroupKey">上書き用登録する際のキー</param>
        public void RegisterCameraGroup(CameraGroup cameraGroup, string overrideGroupKey = null) {
            Initialize();

            if (cameraGroup == null) {
                Debug.LogError("Camera group is null");
                return;
            }

            var groupKey = string.IsNullOrEmpty(overrideGroupKey) ? cameraGroup.Key : overrideGroupKey;

            // カメラグループの登録
            if (_cameraGroupInfos.ContainsKey(groupKey)) {
                // すでにあった場合は登録解除して上書き
                UnregisterCameraGroup(groupKey);
            }

            // 登録処理
            cameraGroup.gameObject.SetActive(true);
            cameraGroup.name = groupKey;
            var groupInfo = new CameraGroupInfo();
            groupInfo.cameraGroup = cameraGroup;
            _cameraGroupInfos[groupKey] = groupInfo;

            // TargetPointsの生成
            CreateTargetPointsInternal(groupInfo.targetPoints, cameraGroup.TargetPointRoot != null ? cameraGroup.TargetPointRoot.transform : null);
            // CameraHandlerの生成
            CreateCameraHandlersInternal(groupInfo.handlers, cameraGroup.CameraRoot.transform);
        }

        /// <summary>
        /// CameraGroupの登録
        /// </summary>
        /// <param name="prefab">CameraGroupを含むPrefab</param>
        /// <param name="overrideGroupKey">上書き用登録する際のキー</param>
        public void RegisterCameraGroupPrefab(GameObject prefab, string overrideGroupKey = null) {
            if (prefab == null) {
                Debug.LogError("Camera group prefab is null");
                return;
            }

            var gameObj = Instantiate(prefab, _cameraGroupRoot.transform, false);
            var cameraGroup = gameObj.GetComponent<CameraGroup>();
            if (cameraGroup == null || cameraGroup.CameraRoot == null) {
                Destroy(cameraGroup.gameObject);
                Debug.LogError($"Invalid camera group. [{cameraGroup.name}]");
                return;
            }

            // CameraGroupの登録
            RegisterCameraGroup(cameraGroup, overrideGroupKey);
        }

        /// <summary>
        /// カメラグループの登録解除
        /// </summary>
        public void UnregisterCameraGroup(string key) {
            Initialize();

            if (_cameraGroupInfos.TryGetValue(key, out var info)) {
                ClearCameraHandlersInternal(info.handlers);
                ClearTargetPointsInternal(info.targetPoints);
                if (info.cameraGroup != null) {
                    if (info.prefab != null) {
                        Destroy(info.cameraGroup.gameObject);
                    }
                    else {
                        info.cameraGroup.gameObject.SetActive(false);
                    }
                }

                _cameraGroupInfos.Remove(key);
            }
        }

        /// <summary>
        /// カメラグループの登録解除
        /// </summary>
        public void UnregisterCameraGroup(CameraGroup cameraGroup) {
            if (cameraGroup == null) {
                Debug.LogError("Camera group prefab is null");
                return;
            }

            UnregisterCameraGroup(cameraGroup.Key);
        }

        /// <summary>
        /// カメラグループの登録解除
        /// </summary>
        public void UnregisterCameraGroup(GameObject prefab) {
            if (prefab == null) {
                Debug.LogError("Camera group prefab is null");
                return;
            }

            var cameraGroup = prefab.GetComponent<CameraGroup>();
            if (cameraGroup == null) {
                Debug.LogError($"Not found camera group. [{prefab.name}]");
                return;
            }

            UnregisterCameraGroup(cameraGroup.Key);
        }

        /// <summary>
        /// CameraComponentの取得
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">対象のカメラ名</param>
        public TCameraComponent GetCameraComponent<TCameraComponent>(string groupKey, string cameraName)
            where TCameraComponent : class, ICameraComponent {
            Initialize();

            var handlers = GetCameraHandlers(groupKey);

            if (!handlers.TryGetValue(cameraName, out var handler)) {
                return default;
            }

            return handler.Component as TCameraComponent;
        }

        /// <summary>
        /// CameraComponentの取得
        /// </summary>
        /// <param name="cameraName">対象のカメラ名</param>
        public TCameraComponent GetCameraComponent<TCameraComponent>(string cameraName)
            where TCameraComponent : class, ICameraComponent {
            return GetCameraComponent<TCameraComponent>(MainCameraGroupName, cameraName);
        }

        /// <summary>
        /// CameraControllerの設定
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">対象のカメラ名</param>
        /// <param name="cameraController">設定するController</param>
        public void SetCameraController(string groupKey, string cameraName, ICameraController cameraController) {
            Initialize();

            var handlers = GetCameraHandlers(groupKey);

            if (!handlers.TryGetValue(cameraName, out var handler)) {
                return;
            }

            handler.SetController(cameraController);
        }

        /// <summary>
        /// CameraControllerの設定
        /// </summary>
        /// <param name="cameraName">対象のカメラ名</param>
        /// <param name="cameraController">設定するController</param>
        public void SetCameraController(string cameraName, ICameraController cameraController) {
            SetCameraController(MainCameraGroupName, cameraName, cameraController);
        }

        /// <summary>
        /// CameraControllerの取得
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">対象のカメラ名</param>
        public TCameraController GetCameraController<TCameraController>(string groupKey, string cameraName)
            where TCameraController : class, ICameraController {
            Initialize();

            var handlers = GetCameraHandlers(groupKey);

            if (!handlers.TryGetValue(cameraName, out var handler)) {
                return null;
            }

            return handler.Controller as TCameraController;
        }

        /// <summary>
        /// CameraControllerの取得
        /// </summary>
        /// <param name="cameraName">対象のカメラ名</param>
        public TCameraController GetCameraController<TCameraController>(string cameraName)
            where TCameraController : class, ICameraController {
            return GetCameraController<TCameraController>(MainCameraGroupName, cameraName);
        }

        /// <summary>
        /// ターゲットポイント(仮想カメラが参照するTransform)の取得
        /// </summary>
        /// <param name="targetPointName">ターゲットポイント名</param>
        public Transform GetTargetPoint(string targetPointName) {
            return GetTargetPoint(MainCameraGroupName, targetPointName);
        }

        /// <summary>
        /// ターゲットポイント(仮想カメラが参照するTransform)の取得
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="targetPointName">ターゲットポイント名</param>
        public Transform GetTargetPoint(string groupKey, string targetPointName) {
            Initialize();

            var targetPoints = GetTargetPointsInternal(groupKey);

            if (!targetPoints.TryGetValue(targetPointName, out var targetPoint)) {
                return null;
            }

            return targetPoint;
        }

        /// <summary>
        /// ターゲットポイントのリストを取得
        /// </summary>
        public Transform[] GetTargetPoints(string groupKey = null) {
            var dict = GetTargetPointsInternal(groupKey);
            return dict.Values.ToArray();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            // Cameraの設定を戻す
            CinemachineCore.GetBlendOverride -= GetBlend;
            _brain.m_UpdateMethod = _defaultUpdateMethod;

            // カメラ情報を廃棄
            foreach (var info in _cameraGroupInfos) {
                ClearCameraHandlersInternal(info.Value.handlers);
            }

            _cameraGroupInfos.Clear();

            ClearCameraHandlersInternal(_cameraHandlers);

            _targetPoints.Clear();

            LayeredTime.Dispose();
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void AwakeInternal() {
            Initialize();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void OnDestroyInternal() {
            Dispose();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            var deltaTime = LayeredTime.DeltaTime;

            // Controllerの更新
            foreach (var pair in _cameraHandlers) {
                if (pair.Value.Controller == null) {
                    continue;
                }

                pair.Value.Controller.Update(deltaTime);
            }

            foreach (var pair in _cameraGroupInfos) {
                foreach (var p in pair.Value.handlers) {
                    if (p.Value.Controller == null) {
                        continue;
                    }

                    p.Value.Controller.Update(deltaTime);
                }
            }

            // Componentの更新
            foreach (var pair in _cameraHandlers) {
                if (pair.Value.Component == null || !pair.Value.Component.IsActive) {
                    continue;
                }

                pair.Value.Component.Update(deltaTime);
            }

            foreach (var pair in _cameraGroupInfos) {
                foreach (var p in pair.Value.handlers) {
                    if (p.Value.Component == null) {
                        continue;
                    }

                    p.Value.Component.Update(deltaTime);
                }
            }

            // Brainの更新
            CinemachineCore.UniformDeltaTimeOverride = deltaTime;
            _brain.ManualUpdate();
        }

        /// <summary>
        /// カメラのアクティブ化
        /// </summary>
        private void ActivateInternal(string groupKey, string cameraName, CameraBlend cameraBlend, bool force, int overridePriority) {
            var handlers = GetCameraHandlers(groupKey);

            if (!handlers.TryGetValue(cameraName, out var handler)) {
                return;
            }

            _toCameraBlends[handler.Component.BaseCamera] = cameraBlend;

            handler.Activate(force);
        }

        /// <summary>
        /// カメラの非アクティブ化
        /// </summary>
        private void DeactivateInternal(string groupKey, string cameraName, CameraBlend cameraBlend, bool force) {
            var handlers = GetCameraHandlers(groupKey);

            if (!handlers.TryGetValue(cameraName, out var handler)) {
                return;
            }

            _fromCameraBlends[handler.Component.BaseCamera] = cameraBlend;

            handler.Deactivate(force);
        }

        /// <summary>
        /// カメラハンドラー格納用Dictionaryの取得
        /// </summary>
        private Dictionary<string, CameraHandler> GetCameraHandlers(string groupKey) {
            if (string.IsNullOrEmpty(groupKey)) {
                return _cameraHandlers;
            }

            if (_cameraGroupInfos.TryGetValue(groupKey, out var info)) {
                return info.handlers;
            }

            return new();
        }

        /// <summary>
        /// ターゲットポイント格納用Dictionaryの取得
        /// </summary>
        private Dictionary<string, Transform> GetTargetPointsInternal(string groupKey) {
            if (string.IsNullOrEmpty(groupKey)) {
                return _targetPoints;
            }

            if (_cameraGroupInfos.TryGetValue(groupKey, out var info)) {
                return info.targetPoints;
            }

            return new();
        }

        /// <summary>
        /// CameraHandlerの生成
        /// </summary>
        private void CreateCameraHandlersInternal(Dictionary<string, CameraHandler> handlers, Transform rootTransform) {
            handlers.Clear();

            void Create(Transform root) {
                foreach (Transform child in root) {
                    // カメラ名が既にあれば何もしない
                    var cameraName = child.name;
                    if (handlers.ContainsKey(cameraName)) {
                        Debug.LogWarning($"Already exists camera name. [{cameraName}]");
                        continue;
                    }

                    var component = child.GetComponent<ICameraComponent>();
                    if (component == null) {
                        // Componentがないただの仮想カメラだったら、DefaultComponentを設定
                        var vcam = child.GetComponent<CinemachineVirtualCameraBase>();
                        if (vcam == null) {
                            // 何もない場合は再起する
                            Create(child);
                            continue;
                        }

                        component = new DefaultCameraComponent(vcam);
                    }

                    component.Initialize(this);

                    var handler = new CameraHandler(cameraName, component);
                    handlers[cameraName] = handler;
                }
            }

            Create(rootTransform);
        }

        /// <summary>
        /// CameraHandlerの解放
        /// </summary>
        private void ClearCameraHandlersInternal(Dictionary<string, CameraHandler> handlers) {
            // カメラ情報を廃棄
            foreach (var pair in handlers) {
                pair.Value.Dispose();
            }

            handlers.Clear();
        }

        /// <summary>
        /// TargetPoint情報の生成
        /// </summary>
        private void CreateTargetPointsInternal(Dictionary<string, Transform> targetPoints, Transform rootTransform) {
            targetPoints.Clear();

            if (rootTransform == null) {
                return;
            }

            foreach (Transform targetPoint in rootTransform) {
                if (targetPoint == rootTransform) {
                    continue;
                }

                var targetPointName = targetPoint.name;
                if (targetPoints.ContainsKey(targetPointName)) {
                    targetPoint.gameObject.SetActive(false);
                    continue;
                }

                targetPoints[targetPointName] = targetPoint;
            }
        }

        /// <summary>
        /// CameraHandlerの解放
        /// </summary>
        private void ClearTargetPointsInternal(Dictionary<string, Transform> targetPoints) {
            targetPoints.Clear();
        }

        /// <summary>
        /// カメラのBlend値を取得(Callback)
        /// </summary>
        /// <param name="fromCamera">前のカメラ</param>
        /// <param name="toCamera">次のカメラ</param>
        /// <param name="defaultBlend">現在のBlend情報</param>
        /// <param name="owner">CinemachineBrain</param>
        private CinemachineBlendDefinition GetBlend(ICinemachineCamera fromCamera, ICinemachineCamera toCamera,
            CinemachineBlendDefinition defaultBlend, MonoBehaviour owner) {
            // Cameraに対するBlend情報を探す
            _toCameraBlends.TryGetValue(toCamera, out var toBlend);
            _fromCameraBlends.TryGetValue(fromCamera, out var fromBlend);

            // Toが優先
            if (toBlend != null) {
                return toBlend.BlendDefinition;
            }

            if (fromBlend != null) {
                return fromBlend.BlendDefinition;
            }

            return defaultBlend;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;

            _defaultUpdateMethod = _brain.m_UpdateMethod;
            _brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;
            CinemachineCore.GetBlendOverride += GetBlend;

            // カメラグループのルート作成
            _cameraGroupRoot = new GameObject("Groups");
            _cameraGroupRoot.transform.SetParent(transform, false);

            // メインのカメラグループを設定
            if (_mainCameraGroupPrefab != null) {
                RegisterCameraGroupPrefab(_mainCameraGroupPrefab, MainCameraGroupName);
            }
            else if (_mainCameraGroup != null) {
                _mainCameraGroup.transform.SetParent(_cameraGroupRoot.transform, false);
                RegisterCameraGroup(_mainCameraGroup, MainCameraGroupName);
            }

            // デフォルトのカメラをActivate
            Activate(_defaultCameraName);
        }
    }
}