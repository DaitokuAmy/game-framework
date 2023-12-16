using System.Collections;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション定義用インターフェース
    ///
    /// Standby
    ///   LoadRoutine
    ///     SetupRoutine
    ///       OpenRoutine
    ///         Activate
    ///           Update/LateUpdate
    ///         Deactivate
    ///       CloseRoutine
    ///     Cleanup
    ///   Unload
    /// Release
    /// </summary>
    public interface ISituation {
        /// <summary>コンテナ登録されているか</summary>
        bool PreRegistered { get; }
        /// <summary>プリロード状態</summary>
        PreLoadState PreLoadState { get; }

        /// <summary>
        /// 待機処理
        /// </summary>
        /// <param name="container">登録されたContainer</param>
        void Standby(SituationContainer container);

        /// <summary>
        /// 読み込み処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="preload">事前読みか</param>
        IEnumerator LoadRoutine(TransitionHandle handle, bool preload);

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator SetupRoutine(TransitionHandle handle);

        /// <summary>
        /// 開く直前の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void PreOpen(TransitionHandle handle);

        /// <summary>
        /// 開く処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator OpenRoutine(TransitionHandle handle);

        /// <summary>
        /// 開く直後の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void PostOpen(TransitionHandle handle);

        /// <summary>
        /// アクティブ化された時の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Activate(TransitionHandle handle);

        /// <summary>
        /// 更新
        /// </summary>
        void Update();

        /// <summary>
        /// 後更新
        /// </summary>
        void LateUpdate();

        /// <summary>
        /// 物理更新
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// ディアクティブ化された時の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Deactivate(TransitionHandle handle);

        /// <summary>
        /// 閉じる直前の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void PreClose(TransitionHandle handle);

        /// <summary>
        /// 閉じる処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator CloseRoutine(TransitionHandle handle);

        /// <summary>
        /// 閉じる直後の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void PostClose(TransitionHandle handle);

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Cleanup(TransitionHandle handle);

        /// <summary>
        /// 解放処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Unload(TransitionHandle handle);

        /// <summary>
        /// 解除処理
        /// </summary>
        /// <param name="container">登録されていたContainer</param>
        void Release(SituationContainer container);

        /// <summary>
        /// コンテナ事前登録
        /// </summary>
        /// <param name="container">事前登録するContainer</param>
        void PreRegister(SituationContainer container);

        /// <summary>
        /// コンテナ事前登録解除
        /// </summary>
        /// <param name="container">事前登録されていたContainer</param>
        void PreUnregister(SituationContainer container);

        /// <summary>
        /// プリロード処理
        /// </summary>
        IEnumerator PreLoadRoutine();

        /// <summary>
        /// プリロード解除処理
        /// </summary>
        void UnPreLoad();

        /// <summary>
        /// 次に遷移する際のデフォルト遷移方法を取得
        /// </summary>
        ITransition GetDefaultNextTransition();

        /// <summary>
        /// 遷移可能かチェック
        /// </summary>
        /// <param name="nextTransition">遷移するの子シチュエーション</param>
        /// <param name="transition">遷移処理</param>
        /// <returns>遷移可能か</returns>
        bool CheckNextTransition(Situation nextTransition, ITransition transition);
    }
}