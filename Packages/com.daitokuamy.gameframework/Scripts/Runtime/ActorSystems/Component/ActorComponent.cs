using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEngine.Scripting;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorをEntityと紐づけるためのComponent
    /// </summary>
    [Preserve]
    public sealed class ActorComponent : Component {
        /// <summary>
        /// Actor管理情報
        /// </summary>
        private class ActorInfo {
            public int priority;
            public IActor actor;
        }

        // 優先度で並べたActor情報リスト
        private readonly List<ActorInfo> _sortedActorInfos = new List<ActorInfo>();

        /// <summary>
        /// 現在のActorを取得
        /// </summary>
        public TActor GetCurrentActor<TActor>()
            where TActor : Actor {
            if (_sortedActorInfos.Count <= 0) {
                return default;
            }

            return _sortedActorInfos[0].actor as TActor;
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
            foreach (var actorInfo in _sortedActorInfos) {
                var actor = actorInfo.actor;
                if (type.IsInstanceOfType(actor)) {
                    return (Actor)actor;
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
            return _sortedActorInfos
                .Where(x => type.IsInstanceOfType(x.actor))
                .Select(x => (TActor)x.actor)
                .ToArray();
        }

        /// <summary>
        /// Actorの取得
        /// </summary>
        /// <param name="type">検索対象のType</param>
        public Actor[] GetActors(Type type) {
            return _sortedActorInfos
                .Where(x => type.IsInstanceOfType(x.actor))
                .Select(x => (Actor)x.actor)
                .ToArray();
        }

        /// <summary>
        /// Actorが含まれているか
        /// </summary>
        public bool ContainsActor(Actor actor) {
            return _sortedActorInfos.Exists(x => x.actor == actor);
        }

        /// <summary>
        /// Actorの追加
        /// </summary>
        /// <param name="actor">追加するActor</param>
        /// <param name="priority">Actorのアクティブ優先度</param>
        public ActorEntity AddActor(Actor actor, int priority = 0) {
            var info = new ActorInfo { actor = actor, priority = priority };
            _sortedActorInfos.Add(info);
            _sortedActorInfos.Sort((a, b) => b.priority - a.priority);
            RefreshActiveActors();
            return Entity;
        }

        /// <summary>
        /// Actorの削除
        /// </summary>
        /// <param name="actor">取り除くActor</param>
        /// <param name="dispose">取り除く際にActorをDisposeするか</param>
        public ActorEntity RemoveActor(Actor actor, bool dispose = true) {
            var count = _sortedActorInfos.RemoveAll(x => x.actor == actor);
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
            foreach (var actorInfo in _sortedActorInfos) {
                ((Actor)actorInfo.actor).Deactivate();
                if (dispose) {
                    actorInfo.actor.Dispose();
                }
            }

            _sortedActorInfos.Clear();
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
            for (var i = 1; i < _sortedActorInfos.Count; i++) {
                var actor = (Actor)_sortedActorInfos[i].actor;
                actor.Deactivate();
            }

            if (_sortedActorInfos.Count > 0) {
                var actor = (Actor)_sortedActorInfos[0].actor;
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