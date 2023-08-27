using GameFramework.BodySystems;
using GameFramework.ModelSystems;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorEntityの拡張メソッド
    /// </summary>
    public static class ActorEntityExtensions {
        /// <summary>
        /// Bodyの検索
        /// </summary>
        public static Body GetBody(this ActorEntity source) {
            var component = source.AddOrGetComponent<BodyComponent>();
            return component.Body;
        }

        /// <summary>
        /// Bodyの設定
        /// </summary>
        public static ActorEntity SetBody(this ActorEntity source, Body body, bool prevDispose = true) {
            var component = source.AddOrGetComponent<BodyComponent>();
            return component.SetBody(body, prevDispose);
        }

        /// <summary>
        /// Bodyの設定
        /// </summary>
        public static ActorEntity RemoveBody(this ActorEntity source, bool dispose = true) {
            var component = source.AddOrGetComponent<BodyComponent>();
            return component.RemoveBody(dispose);
        }

        /// <summary>
        /// 現在のActorを取得
        /// </summary>
        public static TActor GetCurrentActor<TActor>(this ActorEntity source)
            where TActor : Actor {
            var component = source.AddOrGetComponent<ActorComponent>();
            return component.GetCurrentActor<TActor>();
        }

        /// <summary>
        /// Actorを取得
        /// </summary>
        public static TActor GetActor<TActor>(this ActorEntity source)
            where TActor : Actor {
            var component = source.AddOrGetComponent<ActorComponent>();
            return component.GetActor<TActor>();
        }

        /// <summary>
        /// Actorが含まれているか
        /// </summary>
        public static bool ContainsActor(this ActorEntity source, Actor actor) {
            var component = source.AddOrGetComponent<ActorComponent>();
            return component.ContainsActor(actor);
        }

        /// <summary>
        /// Actorを追加
        /// </summary>
        public static ActorEntity AddActor(this ActorEntity source, Actor actor, int priority = 0) {
            var component = source.AddOrGetComponent<ActorComponent>();
            return component.AddActor(actor, priority);
        }

        /// <summary>
        /// Actorの削除
        /// </summary>
        public static ActorEntity RemoveActor(this ActorEntity source, Actor actor, bool dispose = true) {
            var component = source.AddOrGetComponent<ActorComponent>();
            return component.RemoveActor(actor, dispose);
        }

        /// <summary>
        /// Actorの全削除
        /// </summary>
        public static ActorEntity RemoveActors(this ActorEntity source, bool dispose = true) {
            var component = source.AddOrGetComponent<ActorComponent>();
            return component.RemoveActors(dispose);
        }

        /// <summary>
        /// Logicの取得
        /// </summary>
        public static TLogic GetLogic<TLogic>(this ActorEntity source)
            where TLogic : ActorEntityLogic {
            var component = source.AddOrGetComponent<LogicComponent>();
            return component.GetLogic<TLogic>();
        }

        /// <summary>
        /// Logicが含まれているか
        /// </summary>
        public static bool ContainsLogic(this ActorEntity source, ActorEntityLogic logic) {
            var component = source.AddOrGetComponent<LogicComponent>();
            return component.ContainsLogic(logic);
        }

        /// <summary>
        /// Logicを追加
        /// </summary>
        public static ActorEntity AddLogic(this ActorEntity source, ActorEntityLogic logic) {
            var component = source.AddOrGetComponent<LogicComponent>();
            return component.AddLogic(logic);
        }

        /// <summary>
        /// Logicの削除
        /// </summary>
        public static ActorEntity RemoveLogic(this ActorEntity source, ActorEntityLogic logic, bool dispose = true) {
            var component = source.AddOrGetComponent<LogicComponent>();
            return component.RemoveLogic(logic, dispose);
        }

        /// <summary>
        /// Logicの削除
        /// </summary>
        public static ActorEntity RemoveLogic<TLogic>(this ActorEntity source, bool dispose = true)
            where TLogic : ActorEntityLogic {
            var component = source.AddOrGetComponent<LogicComponent>();
            return component.RemoveLogic<TLogic>(dispose);
        }

        /// <summary>
        /// Modelの取得
        /// </summary>
        public static TModel GetModel<TModel>(this ActorEntity source)
            where TModel : IModel {
            var component = source.AddOrGetComponent<ModelComponent>();
            return component.GetModel<TModel>();
        }

        /// <summary>
        /// Modelを追加
        /// </summary>
        public static ActorEntity SetModel<TModel>(this ActorEntity source, TModel model)
            where TModel : class, IModel {
            var component = source.AddOrGetComponent<ModelComponent>();
            return component.SetModel(model);
        }

        /// <summary>
        /// Modelの設定をクリア（削除はされない）
        /// </summary>
        public static ActorEntity ClearModel<TModel>(this ActorEntity source)
            where TModel : class, IModel {
            var component = source.AddOrGetComponent<ModelComponent>();
            return component.ClearModel<TModel>();
        }
    }
}