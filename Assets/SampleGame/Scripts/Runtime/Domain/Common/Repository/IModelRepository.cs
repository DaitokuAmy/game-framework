using GameFramework.Core;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// モデル管理用リポジトリ
    /// </summary>
    public interface IModelRepository {
        /// <summary>
        /// SingleModelの生成
        /// </summary>
        T CreateSingleModel<T>()
            where T : SingleModel<T>, new();

        /// <summary>
        /// AutoIdModelの生成
        /// </summary>
        T CreateAutoIdModel<T>()
            where T : AutoIdModel<T>, new();

        /// <summary>
        /// IdModelの生成
        /// </summary>
        T CreateIdModel<T>(int id)
            where T : IdModel<int, T>, new();

        /// <summary>
        /// SingleModelの取得
        /// </summary>
        T GetSingleModel<T>()
            where T : SingleModel<T>, new();

        /// <summary>
        /// AutoIdModelの取得
        /// </summary>
        T GetAutoIdModel<T>(int id)
            where T : AutoIdModel<T>, new();

        /// <summary>
        /// IdModelの取得
        /// </summary>
        T GetIdModel<T>(int id)
            where T : IdModel<int, T>, new();

        /// <summary>
        /// SingleModelの削除
        /// </summary>
        void DeleteSingleModel<T>()
            where T : SingleModel<T>, new();

        /// <summary>
        /// AutoIdModelの削除
        /// </summary>
        void DeleteAutoIdModel<T>(int id)
            where T : AutoIdModel<T>, new();

        /// <summary>
        /// IdModelの削除
        /// </summary>
        void DeleteIdModel<T>(int id)
            where T : IdModel<int, T>, new();

        /// <summary>
        /// SingleModelの全削除
        /// </summary>
        void ClearSingleModels<T>()
            where T : SingleModel<T>, new();

        /// <summary>
        /// AutoIdModelの全削除
        /// </summary>
        void ClearAutoIdModels<T>()
            where T : AutoIdModel<T>, new();

        /// <summary>
        /// IdModelの全削除
        /// </summary>
        void ClearIdModels<T>()
            where T : IdModel<int, T>, new();
    }
}