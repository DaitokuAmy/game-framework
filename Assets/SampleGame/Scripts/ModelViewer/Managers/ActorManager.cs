using System;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.ActorSystems;
using UniRx;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// Actor管理用クラス
    /// </summary>
    public class ActorManager : IDisposable {
        private Transform _slot;
        private ActorEntity _previewActorEntity;
        private ReactiveProperty<PreviewCharacterActor> _previewActor = new();

        // 現在のプレビュー用アクター
        public IReadOnlyReactiveProperty<PreviewCharacterActor> PreviewActor => _previewActor;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="slot">配置用スロット</param>
        public ActorManager(Transform slot) {
            _slot = slot;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_previewActorEntity != null) {
                _previewActor.Value = null;
                _previewActorEntity.Dispose();
                _previewActorEntity = null;
            }
        }

        /// <summary>
        /// PreviewObjectの変更
        /// </summary>
        public PreviewCharacterActor ChangePreviewActor(PreviewActorModel actorModel) {
            if (_previewActorEntity == null) {
                _previewActorEntity = new ActorEntity();
            }

            var viewerModel = ModelViewerModel.Get();

            // 既存のLogic/Actor/Bodyを削除
            _previewActor.Value = null;
            _previewActorEntity.RemoveLogic<PreviewActorPresenter>();
            _previewActorEntity.RemoveActors();
            _previewActorEntity.RemoveBody();

            var setupData = actorModel.SetupData.Value;

            if (setupData == null) {
                return null;
            }

            // Bodyの生成
            var bodyManager = Services.Get<BodyManager>();
            var body = bodyManager.CreateFromPrefab(setupData.prefab);
            body.Transform.SetParent(_slot, false);
            body.LayeredTime.SetParent(viewerModel.SettingsModel.LayeredTime);
            _previewActorEntity.SetBody(body);

            // Actorの生成
            var actor = new PreviewCharacterActor(body, setupData);
            actor.RegisterTask(TaskOrder.Actor);
            _previewActorEntity.AddActor(actor);
            
            // Presenterの生成
            var presenter = new PreviewActorPresenter(actorModel, actor);
            presenter.RegisterTask(TaskOrder.Logic);
            _previewActorEntity.AddLogic(presenter);

            // アクターの置き換え
            _previewActor.Value = actor;

            return actor;
        }
    }
}