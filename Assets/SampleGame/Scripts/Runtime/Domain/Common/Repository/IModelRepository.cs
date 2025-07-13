using GameFramework.Core;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// モデル管理用リポジトリ
    /// </summary>
    public interface IModelRepository {
        /// <summary>
        /// SingleModelの生成
        /// </summary>
        TModel CreateSingleModel<TModel>()
            where TModel : SingleModel<TModel>, new();

        /// <summary>
        /// AutoIdModelの生成
        /// </summary>
        TModel CreateAutoIdModel<TBaseModel, TModel>(int startId = 1)
            where TBaseModel : AutoIdModel<TBaseModel>
            where TModel : TBaseModel, new();

        /// <summary>
        /// AutoIdModelの生成
        /// </summary>
        TModel CreateAutoIdModel<TModel>(int startId = 1)
            where TModel : AutoIdModel<TModel>, new();

        /// <summary>
        /// IdModelの生成
        /// </summary>        
        TModel CreateIdModel<TBaseModel, TModel>(int id)
            where TBaseModel : IdModel<int, TBaseModel>
            where TModel : TBaseModel, new();

        /// <summary>
        /// IdModelの生成
        /// </summary>
        TModel CreateIdModel<TModel>(int id)
            where TModel : IdModel<int, TModel>, new();

        /// <summary>
        /// SingleModelの取得
        /// </summary>
        TModel GetSingleModel<TModel>()
            where TModel : SingleModel<TModel>, new();

        /// <summary>
        /// AutoIdModelの取得
        /// </summary>
        TModel GetAutoIdModel<TBaseModel, TModel>(int id)
            where TBaseModel : AutoIdModel<TBaseModel>
            where TModel : TBaseModel, new();

        /// <summary>
        /// AutoIdModelの取得
        /// </summary>
        TModel GetAutoIdModel<TModel>(int id)
            where TModel : AutoIdModel<TModel>, new();

        /// <summary>
        /// IdModelの取得
        /// </summary>
        TModel GetIdModel<TBaseModel, TModel>(int id)
            where TBaseModel : IdModel<int, TBaseModel>
            where TModel : TBaseModel, new();

        /// <summary>
        /// IdModelの取得
        /// </summary>
        TModel GetIdModel<TModel>(int id)
            where TModel : IdModel<int, TModel>, new();

        /// <summary>
        /// SingleModelの削除
        /// </summary>
        void DeleteSingleModel<TModel>()
            where TModel : SingleModel<TModel>, new();

        /// <summary>
        /// AutoIdModelの削除
        /// </summary>
        void DeleteAutoIdModel<TModel>(int id)
            where TModel : AutoIdModel<TModel>;

        /// <summary>
        /// AutoIdModelの削除
        /// </summary>
        void DeleteAutoIdModel<TModel>(TModel model)
            where TModel : AutoIdModel<TModel>;

        /// <summary>
        /// IdModelの削除
        /// </summary>
        void DeleteIdModel<TModel>(int id)
            where TModel : IdModel<int, TModel>;

        /// <summary>
        /// SingleModelの全削除
        /// </summary>
        void ClearSingleModels<TModel>()
            where TModel : SingleModel<TModel>, new();

        /// <summary>
        /// AutoIdModelの全削除
        /// </summary>
        void ClearAutoIdModels<TModel>()
            where TModel : AutoIdModel<TModel>;

        /// <summary>
        /// IdModelの全削除
        /// </summary>
        void ClearIdModels<TModel>()
            where TModel : IdModel<int, TModel>;
    }
}