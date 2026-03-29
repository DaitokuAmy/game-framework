# UISystem Overview

`UISystem` は、ゲーム内 UI を「読み込む」「更新する」「画面を切り替える」「ダイアログの結果を受け取る」ための package です。  
このドキュメントは、Package 導入者が最初に使い方を把握するための入口としてまとめています。

## まず何ができるか

`UISystem` で主に扱えるのは次の 4 つです。

- UI プレハブや UI シーンの読み込み
- UI ルートごとの更新管理
- 画面の open / close と画面切り替え
- ダイアログの表示と結果受け取り

細かいクラスは多いですが、最初に覚えるべきものは多くありません。

- `UIManager`
  - UI 全体の入口
- `UIService`
  - ひとまとまりの UI ルート
- `UIScreen`
  - 開閉できる画面
- `UIScreenContainer`
  - 子画面の切り替え
- `UIDialogContainer`
  - ダイアログの表示と結果取得

## どう使い始めるか

基本的な流れは次のとおりです。

1. `IUIAssetLoader` を実装する
2. `UIManager` を生成して `Initialize()` する
3. `UIManager` を毎フレーム更新する
4. UI プレハブまたは UI シーンを読み込む
5. 読み込まれた `UIService` を取得して使う

最小構成のイメージは次のようになります。

```csharp
using GameFramework;
using GameFramework.UISystem;
using UnityEngine;

public sealed class ExampleUiBootstrap : MonoBehaviour
{
    private UIManager _uiManager;
    private UpdateScheduler _updateScheduler;

    private void Awake()
    {
        _updateScheduler = new UpdateScheduler();

        _uiManager = new UIManager();
        _uiManager.Initialize(new ExampleUIAssetLoader());
        _updateScheduler.RegisterUpdatable(_uiManager, 0);
    }

    private void Update()
    {
        _updateScheduler.Update();
    }

    private void LateUpdate()
    {
        _updateScheduler.LateUpdate();
    }

    private void OnDestroy()
    {
        _uiManager?.Dispose();
        _updateScheduler?.Dispose();
    }
}
```

`UIManager` は `MonoBehaviour` ではありません。  
そのため、`UpdateScheduler` などを使って自分で更新を流す必要があります。

## 画面側はどう作るか

Package 導入後に作ることが多いのは、次の 3 種類です。

### `UIService`

UI 全体のまとまりを表すルートです。  
たとえば「タイトル UI 一式」「バトル UI 一式」のように、機能単位で 1 つ作る想定です。

```csharp
using GameFramework.UISystem;
using UnityEngine;

public sealed class TitleUIService : UIService
{
    [SerializeField]
    private UIScreenContainer _screenContainer;

    public void ShowTop()
    {
        _screenContainer.Transition("Top");
    }

    public void ShowConfig()
    {
        _screenContainer.Transition("Config");
    }
}
```

呼び出し側は `UIManager.GetService<TitleUIService>()` で取得して、この service 経由で画面操作を行います。  
Package 利用者は、外側から直接 `UIScreen` を触り回すより、service に用途別メソッドを生やす形にすると使いやすくなります。

### `UIScreen`

1 枚の画面を表します。  
`OpenAsync()` と `CloseAsync()` を持ち、必要なら開閉時の処理をオーバーライドできます。

```csharp
using GameFramework.UISystem;

public sealed class TitleTopScreen : UIScreen
{
}
```

最初は空の `UIScreen` 派生だけでも問題ありません。  
アニメーションや特殊処理が必要になった時点で `PreOpen`、`OpenRoutine`、`PostClose` などを追加すれば十分です。

### `UIDialog`

選択結果を返すダイアログです。  
`UIDialogContainer` から開いて使います。

```csharp
using GameFramework.UISystem;

public sealed class ConfirmDialog : UIDialog
{
    public void OnClickOk()
    {
        SelectIndex(0);
    }

    public void OnClickCancel()
    {
        SelectIndex(1);
    }
}
```

## 画面を切り替えたいとき

複数の子画面を 1 つずつ切り替えたい場合は `UIScreenContainer` を使います。

使い方の考え方は単純です。

- 親に `UIScreenContainer` を置く
- 子に複数の `UIScreen` をぶら下げる
- それぞれに文字列キーを割り当てる
- `Transition("キー")` を呼んで切り替える

呼び出し側のイメージは次のとおりです。

```csharp
_screenContainer.Transition("Top");
_screenContainer.Transition("Config");
_screenContainer.Clear();
```

「今どの画面を見せるか」を管理したいなら、まず `UIScreenContainer` を使うと考えて大丈夫です。

## ダイアログを出したいとき

ダイアログ表示には `UIDialogContainer` を使います。  
これは `UIScreenContainer` と違って、ダイアログをスタックして扱うためのクラスです。

基本の流れは次のとおりです。

- `UIDialogContainer` にテンプレートを登録する
- `OpenDialog<TDialog>()` で開く
- 返ってきた `DialogHandle` を `await` または `yield return` で待つ

```csharp
var handle = _dialogContainer.OpenDialog<ConfirmDialog>("Confirm");
var result = await handle;
```

選択 index ではなく型付きの結果を返したい場合は `IDialog<TResult>` を実装して、`OpenDialog<TDialog, TResult>()` を使います。

```csharp
var result = await _dialogContainer.OpenDialog<ItemSelectDialog, ItemId>("ItemSelect");
```

`UIDialogContainer` は、上に新しいダイアログを積んで、閉じたら 1 つ下へ戻る使い方に向いています。  
複数ダイアログを横並びで常時見せる用途には向いていません。

## UI をロードしたいとき

`UIManager` には 3 つの登録方法があります。

- `LoadSceneAsync(assetKey)`
  - UI 配置済みシーンを読み込む
- `LoadPrefabAsync(assetKey)`
  - UI プレハブを読み込んでインスタンス化する
- `AddGameObject(gameObject)`
  - すでに存在している `GameObject` を管理下に入れる

最初に使うことが多いのは `LoadPrefabAsync()` です。

```csharp
var handle = _uiManager.LoadPrefabAsync("TitleUI");
yield return handle;

if (handle.Exception != null)
{
    Debug.LogException(handle.Exception);
    yield break;
}

var service = _uiManager.GetService<TitleUIService>();
service.ShowTop();
```

ここで返る `AssetHandle` は、ロード完了待ちにも、寿命管理にも使います。  
その UI 一式が不要になったら `Dispose()` してください。

```csharp
handle.Dispose();
```

これにより、そのアセット由来の `UIService` や生成済みプレハブもまとめて解放されます。

## `UIView` はいつ使うか

`UIView` は `UIScreen` ほど大きくない UI 部品を作るときに使います。  
たとえば、一覧アイテム、共通パネル、ステータス表示などです。

```csharp
using GameFramework.UISystem;

public sealed class RewardItemView : UIView
{
}
```

`UIView` 自体には画面切り替え機能はありません。  
開閉や遷移を持たせたいなら `UIScreen`、単なる部品なら `UIView` と考えると整理しやすいです。

## 動的にビューを増やしたいとき

動的生成した `UIView` を `UISystem` の管理下に入れたい場合は、普通の `Instantiate` だけでは足りません。  
`UIView` 側の `InstantiateView()` か `ManualInitialize()` を使って初期化してください。

```csharp
var itemView = InstantiateView(_template, _contentRoot);
```

これを通さないと、生成した view に `UIService` が渡らず、`Initialize` や更新が正しく動きません。

## 便利な補助機能

使い始めの段階では必須ではありませんが、次の機能も用意されています。

- `AnimatableUIScreen`
  - 開閉に `UIAnimationComponent` を使いたいとき
- `UIAnimationPlayer`
  - `IUIAnimation` を個別再生したいとき
- `UIViewPool<TView>`
  - view を使い回したいとき
- `RecyclableScrollList`
  - 仮想化されたスクロールリストを作りたいとき
- `SafeAreaPanel`
  - 端末の safe area に追従したいとき
- `TouchAnimation`
  - ボタン押下時の簡易アニメーションを付けたいとき

最初から全部を使う必要はありません。  
まずは `UIService`、`UIScreenContainer`、`UIDialogContainer` の 3 つを軸に組み立てるのがおすすめです。

## 最初に迷ったときの選び方

- UI 全体の入口が欲しい
  - `UIManager`
- UI ルート単位で API をまとめたい
  - `UIService`
- 1 枚の画面を作りたい
  - `UIScreen`
- 複数画面を切り替えたい
  - `UIScreenContainer`
- ダイアログを開いて結果を受け取りたい
  - `UIDialogContainer`
- 画面ではない UI 部品を作りたい
  - `UIView`

## 注意点

- `UIManager` は自動では更新されない
- `AssetHandle` を破棄すると、その UI 一式も解放される
- 動的生成した `UIView` は `InstantiateView()` などで初期化が必要
- `UIScreenContainer.Remove()` は対象 `GameObject` を破棄する

実装を読む順番に迷ったら、まず `UIManager`、次に `UIService`、その後に `UIScreen`、`UIScreenContainer`、`UIDialogContainer` の順で追うと理解しやすいです。
