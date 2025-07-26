# 🎮 GameFramework.UISystems - UIシステムドキュメント

## 🧩 概要

この UI フレームワークは、**動的UIのロード**、**UIサービスの更新管理**、**遷移制御つき画面構築**などを提供する統合的な UI 管理システムです。
`UIManager` を中核として、シーン/プレハブの非同期ロード、画面の切り替え、状態管理を実現します。

---

## 🗂 全体構造図

```plaintext
UIManager
├── IUIAssetLoader               // アセットロード（Scene / Prefab）
├── UIService                   // UIView群の更新とスコープ管理
│   └── UIView (MonoBehaviour)
│       └── UIScreen            // アニメーション付き遷移UI
│           └── UIScreenContainer (複数画面を管理)
│               ├── UIPageContainer    // スタック遷移（戻る対応）
│               └── UISheetContainer   // 単純な切り替え
├── AssetHandle (Scene / Prefab)
└── CoroutineRunner
```

---

## 🔧 主要コンポーネント別ドキュメント

### ### UIManager

UI全体のライフサイクルとアセットを統括するマネージャ。

| メンバー関数                                         | 説明             |
| ---------------------------------------------- | -------------- |
| `Initialize(loader, layeredTime)`              | アセットローダのセットアップ |
| `LoadSceneAsync(key)` / `LoadPrefabAsync(key)` | 非同期読み込みとサービス登録 |
| `Unload(handle)`                               | ロード済みUIのアンロード  |
| `GetService<T>()`                              | 特定のUIサービス取得    |
| `GetCanvases()`                                | 登録されたCanvasの取得 |

---

### ### IUIAssetLoader

UI のロード元アセット（Scene / Prefab）を提供。

```csharp
SceneAssetHandle GetSceneAssetHandle(string key);
AssetHandle<GameObject> GetPrefabAssetHandle(string key);
```

---

### ### UIService（抽象基底）

複数の `UIView` を管理・更新するUI単位。`CanvasGroup`必須。

* `Initialize()`: 内部でUIViewを全自動で登録
* `Update(deltaTime)`, `LateUpdate(deltaTime)`: 各Viewに伝播
* `RegisterView(view)`, `UnregisterView(view)`

---

### ### UIView（個別UI）

個々の UI 表現。`MonoBehaviour`, `IUIView` 実装。

* `Initialize(service)`: `UIService` に登録される
* `Update()`, `LateUpdate()` による動作更新
* `StartInternal`, `InitializeInternal`, `DisposeInternal` 等で拡張可能
* `InstantiateView<T>()`, `ManualInitialize()` により View の動的生成に対応

---

### ### UIScreen（遷移付きビュー）

`UIView` 拡張クラス。画面単位で開閉を制御。

| メンバー                                          | 説明                           |
| --------------------------------------------- | ---------------------------- |
| `OpenAsync(transitionDirection, immediate, force)` | アニメーション付きで開く                 |
| `CloseAsync(...)`                             | 閉じる                          |
| `IsActivated` / `OpenStatus`                  | 状態フラグ                        |
| `RegisterHandler(handler)`                    | `IUIScreenHandler` 登録でイベント通知 |

---

### ### IUIScreenHandler（ハンドラー）

`UIScreen` に紐づけられるライフサイクル対応コンポーネント。

```csharp
void PreOpen(); void PostOpen(); void Activate();
void Update(); void LateUpdate(); void Deactivate();
void PreClose(); void PostClose();
```

---

## 📚 画面切り替えコンテナ群

### UIScreenContainer（共通基底）

* `Add(key, screen)` / `Remove(key)`
* `FindChild(key)` / `SetAsLastSibling(key)`
* 子UIScreenの管理と整列

---

### UIPageContainer（ページ型）

* スタックによる履歴管理と `Back()` 対応
* `Transition(key, transition, ...)`：Forward/Back 遷移処理
* `Clear()`：全画面閉じ

---

### UISheetContainer（単純切替）

* スタックなし・1画面だけ表示
* `Change(key, transition)` により画面切り替え
* `Clear()`：表示リセット

---

## 🔄 アニメーション制御

### AnimationHandle / AnimationStatus

* 遷移・表示切り替え時の非同期完了を管理
* `IsDone`, `Exception` を確認可能

---

## 🔄 コルーチン管理

`CoroutineRunner` により UIView / UIService それぞれで独立したコルーチン実行が可能。

```csharp
StartCoroutine(IEnumerator, onCompleted, onCanceled, onError, token);
StopCoroutine(coroutine);
```

---

## ✅ 適用例（簡易フロー）

```plaintext
1. UIManager.Initialize(loader)
2. UIManager.LoadPrefabAsync("MainMenu")
3. UIService (MainMenu) が自動でUIView登録
4. UIScreen.OpenAsync() → アニメーションで表示
5. Container.Change("Settings") → Settingsへ遷移
6. UIManager.Unload(handle) → 破棄
```

---

## 📝 備考

* 各クラスは `DisposableScope` を通じてスコープベースのリソース管理を行います。
* `LayeredTime` を通じて任意の時間制御に対応可能（例: スローモーションなど）。

---
