using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション定義用インターフェース
    ///
    /// Standby
    ///   LoadRoutine
    ///     SetupRoutine
    ///       OpenRoutine
    ///         Activate
    ///           Update/LateUpdate/FixedUpdate
    ///         Deactivate
    ///       CloseRoutine
    ///     Cleanup
    ///   Unload
    /// Release
    /// </summary>
    public interface ISituation {
        /// <summary>インスタンス管理用</summary>
        ServiceContainer ServiceContainer { get; }
        /// <summary>プリロード状態</summary>
        PreLoadState PreLoadState { get; }
        /// <summary>親のSituation</summary>
        ISituation Parent { get; }
        /// <summary>子のSituationリスト</summary>
        IReadOnlyList<ISituation> Children { get; }
        /// <summary>RootSituationか(親を持たない)</summary>
        bool IsRoot { get; }
        /// <summary>UnitySceneを保持するSituationか</summary>
        bool HasScene { get; }
        /// <summary>PreLoad可能か</summary>
        bool CanPreLoad { get; }

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
    }
}