using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEngine.Scripting;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorをActorEntityと紐づけるためのComponent
    /// </summary>
    [Preserve]
    public sealed class ActorActorEntityComponent : ActorEntityComponent {
        /// <summary>
        /// Actor管理情報
        /// </summary>
        private class ActorInfo {
            public int Priority;
            public Actor Actor;
        }

        // 優先度で並べたActor情報リスト
        private readonly List<ActorInfo> _orderedActorInfos = new();

        /// <summary>
        /// 現在のActorを取得
        /// </summary>
        public TActor GetCurrentActor<TActor>()
            where TActor : Actor {
            if (_orderedActorInfos.Count <= 0) {
                return null;
            }

            return _orderedActorInfos[0].Actor as TActor;
        }

        /// <summary>
        /// Actorを取得
        /// </summary>
        public TActor GetActor<TActor>()
            where TActor : Actor {
            return GetActor(typeof(TActor)) as TActor;
        }

        /// <summary>
        /// Actorの取得
        /// </summary>
        /// <param name="type">検索対象のType</param>
        public Actor GetActor(Type type) {
            foreach (var actorInfo in _orderedActorInfos) {
                var actor = actorInfo.Actor;
                if (type.IsAssignableFrom(actor.GetType())) {
                    return actor;
                }
            }

            return null;
        }

        /// <summary>
        /// Actorを取得
        /// </summary>
        public TActor[] GetActors<TActor>()
            where TActor : Actor {
            var type = typeof(TActor);
            return _orderedActorInfos
                .Where(x => type.IsAssignableFrom(x.Actor.GetType()))
                .Select(x => (TActor)x.Actor)
                .ToArray();
        }

        /// <summary>
        /// Actorの取得
        /// </summary>
        /// <param name="type">検索対象のType</param>
        public Actor[] GetActors(Type type) {
            return _orderedActorInfos
                .Where(x => type.IsAssignableFrom(x.Actor.GetType()))
                .Select(x => x.Actor)
                .ToArray();
        }

        /// <summary>
        /// Actorが含まれているか
        /// </summary>
        public bool ContainsActor(Actor actor) {
            return _orderedActorInfos.Exists(x => x.Actor == actor);
        }

        /// <summary>
        /// Actorの追加
        /// </summary>
        /// <param name="actor">追加するActor</param>
        /// <param name="priority">Actorのアクティブ優先度</param>
        public ActorEntity AddActor(Actor actor, int priority = 0) {
            var info = new ActorInfo { Actor = actor, Priority = priority };
            _orderedActorInfos.Add(info);
            _orderedActorInfos.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            RefreshActiveActors();
            return Entity;
        }

        /// <summary>
        /// Actorの削除
        /// </summary>
        /// <param name="actor">取り除くActor</param>
        /// <param name="dispose">取り除く際にActorをDisposeするか</param>
        public ActorEntity RemoveActor(Actor actor, bool dispose = true) {
            var count = _orderedActorInfos.RemoveAll(x => x.Actor == actor);
            if (count <= 0) {
                return Entity;
            }

            actor.Deactivate();
            if (dispose) {
                actor.Dispose();
            }

            RefreshActiveActors();
            return Entity;
        }

        /// <summary>
        /// Actorの全削除
        /// </summary>
        /// <param name="dispose">取り除く際にActorをDisposeするか</param>
        public ActorEntity RemoveActors(bool dispose = true) {
            foreach (var actorInfo in _orderedActorInfos) {
                (actorInfo.Actor).Deactivate();
                if (dispose) {
                    actorInfo.Actor.Dispose();
                }
            }

            _orderedActorInfos.Clear();
            return Entity;
        }

        /// <summary>
        /// アクティブ化された時の処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            RefreshActiveActors();
        }

        /// <summary>
        /// 非アクティブ化された時の処理
        /// </summary>
        protected override void DeactivateInternal() {
            RefreshActiveActors();
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            RemoveActors();
        }

        /// <summary>
        /// ActorのActive状態更新
        /// </summary>
        private void RefreshActiveActors() {
            var entityActive = Entity.IsActive;
            for (var i = 1; i < _orderedActorInfos.Count; i++) {
                var actor = _orderedActorInfos[i].Actor;
                actor.Deactivate();
            }

            if (_orderedActorInfos.Count > 0) {
                var actor = _orderedActorInfos[0].Actor;
                if (entityActive) {
                    actor.Activate();
                }
                else {
                    actor.Deactivate();
                }
            }
        }
    }
}