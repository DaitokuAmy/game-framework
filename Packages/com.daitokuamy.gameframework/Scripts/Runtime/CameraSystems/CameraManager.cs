using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// カメラ管理クラス
    /// </summary>
    public class CameraManager : LateUpdatableTaskBehaviour, IDisposable {
        public const string MainCameraGroupKey = "Main";
        
        /// <inheritdoc/>
        public override bool IsActive => base.IsActive && !_disposed;

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
        /// カメラ情報
        /// </summary>
        private class CameraInfo : IDisposable {
            private int _activateCount;
            private int _overridePriority;

            public string Name { get; }
            public ICameraComponent Component { get; }
            public ICameraHandler Handler { get; private set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CameraInfo(string cameraName, ICameraComponent cameraComponent) {
                Name = cameraName;
                Component = cameraComponent;
                _overridePriority = int.MinValue;
                ApplyActiveStatus();
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                SetHandler(null);
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
            /// ハンドラー設定
            /// </summary>
            public void SetHandler(ICameraHandler handler) {
                if (Handler != null) {
                    Handler.Dispose();
                    Handler = null;
                }

                Handler = handler;
                if (Handler != null) {
                    Handler.Initialize(Component);
                    if (Component.IsActive) {
                        handler.Activate();
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
                        Handler?.Activate();
                    }
                    else {
                        Component.Deactivate();
                        Handler?.Deactivate();
                    }
                }
            }
        }

        /// <summary>
        /// カメラグループ情報
        /// </summary>
        private class CameraGroupInfo {
            public GameObject Prefab;
            public CameraGroup CameraGroup;
            public Dictionary<string, CameraInfo> CameraInfos = new();
            public Dictionary<string, Transform> TargetPoints = new();
        }

        [SerializeField, Tooltip("仮想カメラ用のBrain")]
        private CinemachineBrain _brain;
        [SerializeField, Tooltip("メインで扱うカメラグループ")]
        private CameraGroup _mainCameraGroup;
        [SerializeField, Tooltip("メインで扱うカメラグループのPrefab(こちらの指定が優先)")]
        private GameObject _mainCameraGroupPrefab;
        [SerializeField, Tooltip("デフォルトで使用するカメラ名")]
        private string _defaultCameraName = "Default";

        // 初期化済みフラグ
        private bool _initialized;
        // 廃棄済みフラグ
        private bool _disposed;
        // カメラグループのルートオブジェクト
        private GameObject _cameraGroupRoot;
        // Brainの初期状態のUpdateMethod
        private CinemachineBrain.UpdateMethods _defaultUpdateMethod;

        // カメラグループ情報
        private Dictionary<string, CameraGroupInfo> _cameraGroupInfos = new();

        // カメラブレンド情報
        private Dictionary<ICinemachineCamera, CameraBlend> _toCameraBlends = new();
        private Dictionary<ICinemachineCamera, CameraBlend> _fromCameraBlends = new();

        /// <summary>出力カメラ</summary>
        public Camera OutputCamera => _brain != null ? _brain.OutputCamera : null;
        /// <summary>出力カメラブレイン</summary>
        public CinemachineBrain OutputCameraBrain => _brain;
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
            Activate(MainCameraGroupKey, cameraName, blendDefinition, overridePriority);
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
            ForceActivate(MainCameraGroupKey, cameraName, blendDefinition, overridePriority);
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
            Activate(MainCameraGroupKey, cameraName, overridePriority);
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
            ForceActivate(MainCameraGroupKey, cameraName, overridePriority);
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
            Deactivate(MainCameraGroupKey, cameraName, blendDefinition);
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
            ForceDeactivate(MainCameraGroupKey, cameraName, blendDefinition);
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
            Deactivate(MainCameraGroupKey, cameraName);
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
            ForceDeactivate(MainCameraGroupKey, cameraName);
        }

        /// <summary>
        /// カメラのアクティブ状態をチェック
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">カメラ名</param>
        public bool CheckActivate(string groupKey, string cameraName) {
            Initialize();

            var infos = GetCameraInfos(groupKey);
            if (!infos.TryGetValue(cameraName, out var info)) {
                return false;
            }

            return info.CheckActivate();
        }

        /// <summary>
        /// カメラのアクティブ状態をチェック
        /// </summary>
        /// <param name="cameraName">カメラ名</param>
        public bool CheckActivate(string cameraName) {
            return CheckActivate(MainCameraGroupKey, cameraName);
        }

        /// <summary>
        /// カメラ名の一覧を取得
        /// </summary>
        public string[] GetCameraNames(string groupKey = null) {
            if (string.IsNullOrEmpty(groupKey)) {
                groupKey = MainCameraGroupKey;
            }

            var infos = GetCameraInfos(groupKey);
            return infos.Keys.ToArray();
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
            if (string.IsNullOrEmpty(groupKey)) {
                groupKey = MainCameraGroupKey;
            }

            // カメラグループの登録
            if (_cameraGroupInfos.ContainsKey(groupKey)) {
                // すでにあった場合は登録解除して上書き
                UnregisterCameraGroup(groupKey);
            }

            // 登録処理
            cameraGroup.gameObject.SetActive(true);
            cameraGroup.name = groupKey;
            var groupInfo = new CameraGroupInfo();
            groupInfo.CameraGroup = cameraGroup;
            _cameraGroupInfos[groupKey] = groupInfo;

            // TargetPointsの生成
            CreateTargetPointsInternal(groupInfo.TargetPoints, cameraGroup.TargetPointRoot != null ? cameraGroup.TargetPointRoot.transform : null);
            // CameraInfoの生成
            CreateCameraInfosInternal(groupInfo.CameraInfos, cameraGroup.CameraRoot.transform);
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
                ClearCameraInfosInternal(info.CameraInfos);
                ClearTargetPointsInternal(info.TargetPoints);
                if (info.CameraGroup != null) {
                    if (info.Prefab != null) {
                        Destroy(info.CameraGroup.gameObject);
                    }
                    else {
                        info.CameraGroup.gameObject.SetActive(false);
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

            var infos = GetCameraInfos(groupKey);

            if (!infos.TryGetValue(cameraName, out var info)) {
                return default;
            }

            return info.Component as TCameraComponent;
        }

        /// <summary>
        /// CameraComponentの取得
        /// </summary>
        /// <param name="cameraName">対象のカメラ名</param>
        public TCameraComponent GetCameraComponent<TCameraComponent>(string cameraName)
            where TCameraComponent : class, ICameraComponent {
            return GetCameraComponent<TCameraComponent>(MainCameraGroupKey, cameraName);
        }

        /// <summary>
        /// Cameraハンドラーの設定
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">対象のカメラ名</param>
        /// <param name="cameraHandler">設定するHandler</param>
        public void SetCameraHandler(string groupKey, string cameraName, ICameraHandler cameraHandler) {
            Initialize();

            var infos = GetCameraInfos(groupKey);

            if (!infos.TryGetValue(cameraName, out var info)) {
                return;
            }

            info.SetHandler(cameraHandler);
        }

        /// <summary>
        /// Cameraハンドラーの設定
        /// </summary>
        /// <param name="cameraName">対象のカメラ名</param>
        /// <param name="cameraHandler">設定するHandler</param>
        public void SetCameraHandler(string cameraName, ICameraHandler cameraHandler) {
            SetCameraHandler(MainCameraGroupKey, cameraName, cameraHandler);
        }

        /// <summary>
        /// Cameraハンドラーの取得
        /// </summary>
        /// <param name="groupKey">CameraGroupとして登録したキー</param>
        /// <param name="cameraName">対象のカメラ名</param>
        public TCameraHandler GetCameraHandler<TCameraHandler>(string groupKey, string cameraName)
            where TCameraHandler : class, ICameraHandler {
            Initialize();

            var infos = GetCameraInfos(groupKey);

            if (!infos.TryGetValue(cameraName, out var info)) {
                return null;
            }

            return info.Handler as TCameraHandler;
        }

        /// <summary>
        /// Cameraハンドラーの取得
        /// </summary>
        /// <param name="cameraName">対象のカメラ名</param>
        public TCameraHandler GetCameraHandler<TCameraHandler>(string cameraName)
            where TCameraHandler : class, ICameraHandler {
            return GetCameraHandler<TCameraHandler>(MainCameraGroupKey, cameraName);
        }

        /// <summary>
        /// ターゲットポイント(仮想カメラが参照するTransform)の取得
        /// </summary>
        /// <param name="targetPointName">ターゲットポイント名</param>
        public Transform GetTargetPoint(string targetPointName) {
            return GetTargetPoint(MainCameraGroupKey, targetPointName);
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
        /// カメラの状態をリフレッシュ
        /// </summary>
        public void Refresh() {
            _brain.ManualUpdate();
            _brain.enabled = false;
            _brain.enabled = true;
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
            _brain.UpdateMethod = _defaultUpdateMethod;

            // カメラ情報を廃棄
            foreach (var info in _cameraGroupInfos) {
                ClearCameraInfosInternal(info.Value.CameraInfos);
            }

            _cameraGroupInfos.Clear();

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

            // Handlerの更新
            foreach (var pair in _cameraGroupInfos) {
                foreach (var p in pair.Value.CameraInfos) {
                    if (p.Value.Handler == null) {
                        continue;
                    }

                    p.Value.Handler.Update(deltaTime);
                }
            }

            // Componentの更新
            foreach (var pair in _cameraGroupInfos) {
                foreach (var p in pair.Value.CameraInfos) {
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
            var infos = GetCameraInfos(groupKey);

            if (!infos.TryGetValue(cameraName, out var info)) {
                return;
            }

            _toCameraBlends[info.Component.BaseCamera] = cameraBlend;

            info.Activate(force);
        }

        /// <summary>
        /// カメラの非アクティブ化
        /// </summary>
        private void DeactivateInternal(string groupKey, string cameraName, CameraBlend cameraBlend, bool force) {
            var cameraInfos = GetCameraInfos(groupKey);

            if (!cameraInfos.TryGetValue(cameraName, out var info)) {
                return;
            }

            _fromCameraBlends[info.Component.BaseCamera] = cameraBlend;

            info.Deactivate(force);
        }

        /// <summary>
        /// カメラ情報格納用Dictionaryの取得
        /// </summary>
        private Dictionary<string, CameraInfo> GetCameraInfos(string groupKey) {
            if (string.IsNullOrEmpty(groupKey)) {
                groupKey = MainCameraGroupKey;
            }

            if (_cameraGroupInfos.TryGetValue(groupKey, out var info)) {
                return info.CameraInfos;
            }

            return new();
        }

        /// <summary>
        /// ターゲットポイント格納用Dictionaryの取得
        /// </summary>
        private Dictionary<string, Transform> GetTargetPointsInternal(string groupKey) {
            if (string.IsNullOrEmpty(groupKey)) {
                groupKey = MainCameraGroupKey;
            }

            if (_cameraGroupInfos.TryGetValue(groupKey, out var info)) {
                return info.TargetPoints;
            }

            return new();
        }

        /// <summary>
        /// CameraInfoの生成
        /// </summary>
        private void CreateCameraInfosInternal(Dictionary<string, CameraInfo> infos, Transform rootTransform) {
            infos.Clear();

            void Create(Transform root) {
                foreach (Transform child in root) {
                    // カメラ名が既にあれば何もしない
                    var cameraName = child.name;
                    if (infos.ContainsKey(cameraName)) {
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

                    var info = new CameraInfo(cameraName, component);
                    infos[cameraName] = info;
                }
            }

            Create(rootTransform);
        }

        /// <summary>
        /// CameraInfoの解放
        /// </summary>
        private void ClearCameraInfosInternal(Dictionary<string, CameraInfo> infos) {
            // カメラ情報を廃棄
            foreach (var pair in infos) {
                pair.Value.Dispose();
            }

            infos.Clear();
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
        /// ターゲットポイントの解放
        /// </summary>
        private void ClearTargetPointsInternal(Dictionary<string, Transform> targetPoints) {
            targetPoints.Clear();
        }

        /// <summary>
        /// カメラのBlend値を取得(Callback)
        /// </summary>
        /// <param name="fromVcam">前のカメラ</param>
        /// <param name="toVcam">次のカメラ</param>
        /// <param name="defaultBlend">現在のBlend情報</param>
        /// <param name="owner">CinemachineBrain</param>
        private CinemachineBlendDefinition GetBlend(ICinemachineCamera fromVcam, ICinemachineCamera toVcam, CinemachineBlendDefinition defaultBlend, UnityEngine.Object owner) {
            // Cameraに対するBlend情報を探す
            _toCameraBlends.TryGetValue(toVcam, out var toBlend);
            _fromCameraBlends.TryGetValue(fromVcam, out var fromBlend);
            _toCameraBlends.Remove(toVcam);
            _fromCameraBlends.Remove(fromVcam);

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

            _defaultUpdateMethod = _brain.UpdateMethod;
            _brain.UpdateMethod = CinemachineBrain.UpdateMethods.ManualUpdate;
            CinemachineCore.GetBlendOverride += GetBlend;

            // カメラグループのルート作成
            _cameraGroupRoot = new GameObject("Groups");
            _cameraGroupRoot.transform.SetParent(transform, false);

            // メインのカメラグループを設定
            if (_mainCameraGroupPrefab != null) {
                RegisterCameraGroupPrefab(_mainCameraGroupPrefab, MainCameraGroupKey);
            }
            else if (_mainCameraGroup != null) {
                _mainCameraGroup.transform.SetParent(_cameraGroupRoot.transform, false);
                RegisterCameraGroup(_mainCameraGroup, MainCameraGroupKey);
            }

            // デフォルトのカメラをActivate
            Activate(_defaultCameraName);
        }
    }
}