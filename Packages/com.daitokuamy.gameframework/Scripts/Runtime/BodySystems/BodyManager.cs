using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UnityEngine;
using Object = UnityEngine.Object;
#if USE_ANIMATION_RIGGING
using UnityEngine.Animations.Rigging;
#endif

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body管理クラス
    /// </summary>
    public class BodyManager : DisposableLateUpdatableTask {
        /// <summary>
        /// Body情報
        /// </summary>
        private class BodyInfo {
            public IBody body;
            public bool disposed;
        }

        // 構築クラス
        private IBodyBuilder _builder;
        // 時間管理クラス
        private LayeredTime _layeredTime;
        // インスタンス管理
        private List<BodyInfo> _bodyInfos = new List<BodyInfo>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="builder">Body構築用クラス</param>
        /// <param name="layeredTime">時間管理クラス</param>
        public BodyManager(IBodyBuilder builder = null, LayeredTime layeredTime = null) {
            _builder = builder;
            _layeredTime = layeredTime;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            for (var i = 0; i < _bodyInfos.Count; i++) {
                var bodyInfo = _bodyInfos[i];
                if (bodyInfo.disposed) {
                    continue;
                }

                bodyInfo.body.Dispose();
            }

            _bodyInfos.Clear();
        }

        /// <summary>
        /// InstantiateされているGameObjectからBodyを作成する
        /// </summary>
        public Body CreateFromGameObject(GameObject gameObject) {
            var body = new Body(gameObject);

            // 構築処理
            BuildDefault(body, gameObject);
            if (_builder != null) {
                _builder.Build(body, gameObject);
            }

            // Dispatcherの生成
            var dispatcher = gameObject.AddComponent<BodyDispatcher>();
            dispatcher.Initialize(body);

            // 登録情報の初期化
            var bodyInfo = new BodyInfo {
                body = body,
                disposed = false
            };
            bodyInfo.body.ExpiredEvent += () => { bodyInfo.disposed = true; };
            _bodyInfos.Add(bodyInfo);

            // Bodyの初期化
            bodyInfo.body.Initialize();

            return body;
        }

        /// <summary>
        /// PrefabからBodyを作成する
        /// </summary>
        public Body CreateFromPrefab(GameObject prefab) {
            var gameObject = Object.Instantiate(prefab);
            gameObject.name = prefab.name;
            return CreateFromGameObject(gameObject);
        }

        /// <summary>
        /// 何もない空のBodyを作成する
        /// </summary>
        public Body CreateEmpty(string bodyName) {
            return CreateFromGameObject(new GameObject(bodyName));
        }

        /// <summary>
        /// タスク更新処理
        /// </summary>
        protected override void UpdateInternal() {
            var deltaTime = _layeredTime != null ? _layeredTime.DeltaTime : Time.deltaTime;

            for (var i = 0; i < _bodyInfos.Count; i++) {
                var bodyInfo = _bodyInfos[i];
                if (bodyInfo.disposed) {
                    continue;
                }

                bodyInfo.body.Update(deltaTime);
            }
        }

        /// <summary>
        /// タスク後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            var deltaTime = _layeredTime != null ? _layeredTime.DeltaTime : Time.deltaTime;

            for (var i = 0; i < _bodyInfos.Count; i++) {
                var bodyInfo = _bodyInfos[i];
                if (bodyInfo.disposed) {
                    continue;
                }

                bodyInfo.body.LateUpdate(deltaTime);
            }

            // BodyInfosのリフレッシュ
            RefreshBodyInfos();
        }

        /// <summary>
        /// デフォルトのBody構築処理
        /// </summary>
        /// <param name="body">構築対象のBody</param>
        /// <param name="gameObject">制御対象のGameObject</param>
        private void BuildDefault(IBody body, GameObject gameObject) {
            body.AddController(new LocatorController());
            body.AddController(new PropertyController());
            body.AddController(new ParentController());
            body.AddController(new MaterialController());
            body.AddController(new MeshController());
            body.AddController(new AttachmentController());
            body.AddController(new GimmickController());

            void TryAddComponent<T>(GameObject go)
                where T : Component {
                if (go.GetComponent<T>() != null) {
                    return;
                }

                go.AddComponent<T>();
            }

#if USE_ANIMATION_RIGGING
            // RigBuilderがついている場合、RigController追加
            if (gameObject.GetComponent<RigBuilder>() != null) {
                body.AddController(new RigController());
            }
#endif

            // Rigidbody or ColliderPartsがついている場合、ColliderController追加
            if (gameObject.GetComponentInChildren<ColliderParts>() != null || gameObject.GetComponent<Rigidbody>() != null) {
                TryAddComponent<ColliderController>(gameObject);
            }

            // Componentとして入っている物を抽出
            var controllers = gameObject.GetComponentsInChildren<SerializedBodyController>(true);
            foreach (var controller in controllers) {
                body.AddController(controller);
            }
        }

        /// <summary>
        /// BodyInfosの中身をリフレッシュ
        /// </summary>
        private void RefreshBodyInfos() {
            for (var i = _bodyInfos.Count - 1; i >= 0; i--) {
                var bodyInfo = _bodyInfos[i];
                if (!bodyInfo.disposed) {
                    continue;
                }

                _bodyInfos.RemoveAt(i);
            }
        }
    }
}