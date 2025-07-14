# 🎮 GameFramework.SituationSystems ドキュメント（Markdown形式）

## 🧩 概要

このフレームワークは、**Unityの画面・状態・シーン管理を統一的に扱うライフサイクル基盤**です。`Situation` による状態定義と、`Transition` による遷移処理で構成され、柔軟で非同期的な画面遷移が可能になります。

---

## 🔁 ライフサイクル対応表

| 開始フェーズ                              | 終了フェーズ                                | 説明                      |
| ----------------------------------- | ------------------------------------- | ----------------------- |
| `Standby`                           | `Release`                             | 状態の事前登録と登録解除            |
| `Load`                              | `Unload`                              | リソースの読み込みと解放            |
| `Setup`                             | `Cleanup`                             | 初期化とクリーンアップ             |
| `PreOpen / OpenRoutine / PostOpen`  | `PreClose / CloseRoutine / PostClose` | 開く処理と閉じる処理       |
| `Activate`                          | `Deactivate`                          | 有効化と無効化              |
| `Update / LateUpdate / FixedUpdate` | （Deactivate 後に停止）                     | 各種更新処理 |


---

## 🏗 構成図

```plaintext
SituationContainer           // ランタイム遷移制御
├─ Situation                 // 状態・画面の基本単位（抽象）
│  ├─ SceneSituation         // シーン単位の読み込みを伴う状態
│  └─ AdditiveSceneSituation// 加算読み込みシーン用状態
├─ ITransition               // 遷移パターン定義（OutIn / Cross）
├─ ITransitionEffect         // 遷移演出（フェード等）
└─ SituationFlow             // 画面構成定義用のノード構造
   └─ SituationFlowNode      // 遷移先ノードを接続・管理
```

---

## 🎛 Situation / ISituation

状態や画面の単位を表現する基本クラスです。

### 主なプロパティ

| プロパティ              | 説明                 |
| ------------------ | ------------------ |
| `HasScene`         | シーンを持つか            |
| `IsActive`         | アクティブ中か            |
| `ServiceContainer` | DIや参照渡しに使われる依存管理   |
| `Children`         | 子 Situation（ツリー構造） |

### 主なライフサイクルメソッド

```csharp
void Standby(SituationContainer container);
IEnumerator LoadRoutine(TransitionHandle handle, bool preload);
IEnumerator SetupRoutine(TransitionHandle handle);
void PreOpen(TransitionHandle handle);
IEnumerator OpenRoutine(TransitionHandle handle);
void PostOpen(TransitionHandle handle);
void Activate(TransitionHandle handle);
void Update();
void LateUpdate();
void FixedUpdate();
void PreClose(TransitionHandle handle);
IEnumerator CloseRoutine(TransitionHandle handle);
void PostClose(TransitionHandle handle);
void Deactivate(TransitionHandle handle);
void Cleanup(TransitionHandle handle);
void Unload(TransitionHandle handle);
void Release(SituationContainer container);
```

---

## 🧭 SceneSituation / AdditiveSceneSituation

### `SceneSituation`

Unity のシーンを読み込み対象とした `Situation` の実装。

* `SceneAssetPath`：対象シーンのパス
* `EmptySceneAssetPath`：Unload 時に空シーンを指定可能
* `LoadSceneMode.Single` で読み込み

### `AdditiveSceneSituation`

複数のシーンを重ねる遷移に適した `Situation`。

* `SceneAssetPath`：加算シーンのパス
* `LoadSceneMode.Additive` で読み込み
* `SceneManager.UnloadSceneAsync` による解放

---

## 🔄 Transition / ITransition

遷移処理の振る舞いを定義するインターフェース。

### 実装例

#### OutInTransition

```plaintext
Close (前) → Load (次) → Open (次)
```

* 前シーンのアニメーション後に完全に切り替える
* バックグラウンドロードの優先度設定可

#### CrossTransition

```plaintext
同時に前を閉じつつ次を開く
```

* より高速で自然な遷移演出が可能

---

## 🎨 ITransitionEffect

任意の遷移演出（フェードなど）を追加できる仕組み。

```csharp
void Begin();
IEnumerator EnterRoutine();
void Update();
IEnumerator ExitRoutine();
void End();
```

---

## 🔁 SituationContainer

ランタイムにおける `Situation` の遷移制御クラス。

### 主な機能

| メソッド名               | 概要           |
| ------------------- | ------------ |
| `Setup(root)`       | ルート状態の設定     |
| `Transition<T>()`   | 状態の切り替え      |
| `Reset()`           | 現在の状態をリロード   |
| `PreLoadAsync<T>()` | 状態を事前にロード    |
| `Update()` ほか       | 各種更新タイミングを伝播 |

---

## 🔄 SituationFlow / FlowNode

### `SituationFlow`

画面構成をツリー的に設計・定義するためのクラス。

```csharp
var menuNode = flow.ConnectRoot<MenuSituation>();
var gameNode = menuNode.Connect<GameSituation>();
flow.Transition<GameSituation>();
```

### `SituationFlowNode`

個々の遷移先を保持するノード。`Connect<T>()` により順次遷移構造を拡張できます。

---

## ✅ 状態遷移の使用例（コード）

```csharp
var container = new SituationContainer();
container.Setup(new RootSituation());

var flow = new SituationFlow(container);
var menu = flow.ConnectRoot<MainMenuSituation>();
var game = menu.Connect<GameplaySituation>();

flow.Transition<GameplaySituation>();
flow.Back();              // 1階層戻る
flow.RefreshTransition<GameplaySituation>(); // 再構築遷移
```

---

## 📦 拡張・応用設計

* **ServiceContainer** により DI 対応
* `SituationFlowNode` による UI 遷移設計
* `PreLoadAsync` を使った滑らかな遷移
* **戻り遷移・リセット遷移** 両対応

---
