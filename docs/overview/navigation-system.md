# NavigationSystem Overview

## 目的

`NavigationSystem` は、画面遷移を `RootNode` / `SessionNode` / `ScreenNode` の階層で管理する仕組みです。

利用者は主に次の 3 つを定義します。

- どんな画面構造にするか
- どこからどこへ遷移できるか
- 各画面で何を読み込み、いつ解放するか

## まず押さえること

### Node の役割

- `RootNode`
  - アプリ全体のルート
- `SessionNode`
  - 大きな機能単位やシーン単位
- `ScreenNode`
  - 実際の画面やダイアログ

典型的には次のような構造になります。

```text
RootNode
└─ SessionNode
   ├─ ScreenNode
   └─ ScreenNode
      └─ ScreenNode
```

### Lifecycle と Router は別物

- `Lifecycle`
  - Node の親子関係を定義する
- `Router`
  - 実際の遷移ルールを定義する

「どこに属する画面か」と「どこへ遷移できるか」は分けて考えるのが基本です。

## 基本的な使い方

### 1. Lifecycle を定義する

```csharp
var builder = NavigationEngineBuilder.Create()
    .CreateLifecycle<AppRootNode>(Id.Root, root => {
        root.AddSession<MainSessionNode>(Id.Main, session => {
            session.AddScreen<HomeScreenNode>(Id.Home)
                .AddScreen<SettingsScreenNode>(Id.Settings);
        });
    });
```

ここで定義するのは構造だけです。  
この時点では、まだ遷移ルールは決まりません。

### 2. 必要なら Router を定義する

```csharp
builder = builder.CreateRouter(container =>
    NavNodeTreeRouterBuilder.Create()
        .AddRoot(Id.Home, home => {
            home.Connect(Id.Settings);
        })
        .Build(container));
```

`Router` を作ると、`Back()` や遷移履歴を使った制御ができます。  
`Router` を使わない場合は、`TransitionTo(...)` を直接呼ぶ形が中心になります。

### 3. Engine を生成する

```csharp
var engine = builder.Build();
```

VContainer を使う場合は、親 resolver を渡して build できます。

```csharp
var engine = builder.Build(globalResolver);
```

## `NavigationEngine` の主な API

- `TransitionTo<TScreen>(nodeId, ...)`
  - 指定した画面へ遷移する
- `Back(...)`
  - Router の履歴を使って戻る
- `Reset(...)`
  - 現在の path を閉じて同じ path を開き直す
- `PreLoad(nodeId)`
  - 遷移前に対象画面のロードを先行する
- `UnPreLoad(nodeId)`
  - 先行ロード状態を解除する
- `Update()`
  - 毎フレーム呼ぶ

`Back(...)` は Router を構築している場合に使います。

## 自前の Node を作る

利用者が最も触るのは `RootNode` / `SessionNode` / `ScreenNode` の実装です。  
最初は「Root は全体」「Session はまとまり」「Screen は実画面」と分けて考えると分かりやすくなります。

### 最小構成の例

```csharp
public static class ScreenIds {
    public const int Root = 1;
    public const int MainSession = 100;
    public const int Home = 1000;
    public const int Settings = 1001;
}

public sealed class AppRootNode : RootNode {
}

public sealed class MainSessionNode : SessionNode {
}

public sealed class HomeScreenNode : ScreenNode {
}

public sealed class SettingsScreenNode : ScreenNode {
}
```

これを Lifecycle に登録します。

```csharp
var engine = NavigationEngineBuilder.Create()
    .CreateLifecycle<AppRootNode>(ScreenIds.Root, root => {
        root.AddSession<MainSessionNode>(ScreenIds.MainSession, session => {
            session.AddScreen<HomeScreenNode>(ScreenIds.Home)
                .AddScreen<SettingsScreenNode>(ScreenIds.Settings);
        });
    })
    .Build();
```

### `ScreenNode` の基本実装例

利用者が最もよく書くのは `ScreenNode` です。  
最初の 1 画面は次のような形で実装すると整理しやすくなります。

```csharp
public sealed class HomeScreenNode : ScreenNode {
    private HomeScreenView _view;

    protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
        _view = HomeScreenView.Create();
        _view.SetVisible(false);
        yield break;
    }

    protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
        _view.Initialize();
        yield break;
    }

    protected override void Activate(TransitionHandle<INavNode> handle, IScope scope) {
        _view.OnClickSettings += OnClickSettings;
    }

    protected override void Deactivate(TransitionHandle<INavNode> handle, IScope scope) {
        _view.OnClickSettings -= OnClickSettings;
    }

    protected override IEnumerator OpenRoutine(TransitionHandle<INavNode> handle, IScope scope) {
        _view.SetVisible(true);
        yield return _view.PlayOpenAnimation();
    }

    protected override IEnumerator CloseRoutine(TransitionHandle<INavNode> handle, IScope scope) {
        yield return _view.PlayCloseAnimation();
        _view.SetVisible(false);
    }

    protected override void Unload(TransitionHandle<INavNode> handle, IScope scope) {
        _view.Dispose();
        _view = null;
    }

    private void OnClickSettings() {
        // 遷移用のサービスやナビゲータを呼ぶ
    }
}
```

この例では次の分担になっています。

- `LoadRoutine(...)`
  - View やアセットを用意する
- `InitializeRoutine(...)`
  - 用意したものを初期化する
- `Activate(...)`
  - イベント購読を開始する
- `Deactivate(...)`
  - イベント購読を解除する
- `OpenRoutine(...)` / `CloseRoutine(...)`
  - 表示演出を行う
- `Unload(...)`
  - `LoadRoutine(...)` で作ったものを解放する

### `SessionNode` の基本実装例

`SessionNode` には、そのセッション配下で共有したい前提を置きます。

```csharp
public sealed class MainSessionNode : SessionNode {
    private MainSessionContext _context;

    protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
        _context = new MainSessionContext();
        yield break;
    }

    protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
        _context.Initialize();
        yield break;
    }

    protected override void Unload(TransitionHandle<INavNode> handle, IScope scope) {
        _context.Dispose();
        _context = null;
    }
}
```

複数の `ScreenNode` で共有する前提があるなら、`ScreenNode` ごとに重複実装せず `SessionNode` 側へ寄せると扱いやすくなります。

### `RootNode` の基本実装例

`RootNode` は必須ですが、特別な処理がなければ空実装でも問題ありません。

```csharp
public sealed class AppRootNode : RootNode {
}
```

アプリ全体で共有する仕組みを置きたい場合だけ、`RootNode` に処理を追加します。

## 何をどこに書くべきか

### よく使うフック

- `LoadRoutine(...)`
  - アセット、Prefab、データなどの読み込み
- `InitializeRoutine(...)`
  - 読み込み後の初期化
- `Activate(...)`
  - 画面を使い始めるタイミングの処理
- `Deactivate(...)`
  - 画面を使い終えるタイミングの処理
- `Unload(...)`
  - `LoadRoutine(...)` で確保したものの解放
- `Release()`
  - `Standby(...)` で立ち上がった runtime context の解放

`ScreenNode` では、さらに次の表示演出用フックを使えます。

- `PreOpen(...)`
- `OpenRoutine(...)`
- `PostOpen(...)`
- `PreClose(...)`
- `CloseRoutine(...)`
- `PostClose(...)`

### 実装時の目安

- `LoadRoutine(...)`
  - その Node 自身で完結する読み込み
- `InitializeRoutine(...)`
  - 親 Node の構築結果に依存する初期化
- `Activate(...)`
  - 購読開始、入力受付開始、表示中だけ必要な処理
- `Deactivate(...)`
  - 購読解除、入力停止
- `Unload(...)`
  - 読み込んだリソースの解放

特に重要なのは、親 Node の構築に依存する処理を `LoadRoutine(...)` に書かないことです。  
そうした処理は `InitializeRoutine(...)` に寄せてください。

## runtime の寿命

### Node インスタンスは常駐する

Node 自体は `Build()` 時に生成され、`NavigationEngine` の寿命中は常駐します。

### runtime context は利用時に作られる

現在の設計では、runtime context は利用時に `Standby(...)` で立ち上がります。

- `Build()` しただけでは `Standby(...)` されない
- 遷移時や `PreLoad` 時に、必要な path が `Standby(...)` される
- 不要になった Node は `Release()` で runtime context が破棄される

このため、Node の寿命と、DI や注入済み依存の寿命は同じではありません。

## VContainer 利用時

### `Configure(...)` の役割

VContainer を使う場合、各 Node は `Configure(IContainerBuilder builder)` で、その Node 用の依存を登録できます。

```csharp
protected override void Configure(IContainerBuilder builder) {
    base.Configure(builder);
    builder.Register<InventoryService>(Lifetime.Singleton);
}
```

登録した依存は、その Node と子孫 Node から参照できます。

### いつ生成されるか

resolver は `Build()` 時には作られず、その Node が `Standby(...)` される時点で生成されます。

- `RootNode`
  - `Build(parentResolver)` で渡した親 resolver を親にする
- `SessionNode` / `ScreenNode`
  - 親 Node の resolver を親にする

### いつ注入されるか

`Inject(this)` は resolver 生成後、`Standby(...)` の前に実行されます。  
そのため、`Standby(...)`、`LoadRoutine(...)`、`InitializeRoutine(...)` では注入済み依存を利用できます。

### いつ破棄されるか

resolver は `Release()` で破棄されます。  
つまり、`Configure(...)` で登録した依存の寿命は「その Node が使われている runtime 期間」です。

## `PreLoad` の考え方

`PreLoad` は便利ですが、通常の画面実装より少し応用的な機能です。  
まずは通常遷移を正しく実装し、そのあと必要になったら使うのがおすすめです。

### `PreLoad` がやること

`PreLoad(target)` は、target 単体の先読みではなく、target までの path を利用可能状態にしてから target の `LoadRoutine(...)` を先に実行します。

保証されるのは次の内容です。

- target までの path が `Standby(...)` 済み
- target の `LoadRoutine(...)` が実行済み

### `PreLoad` が保証しないこと

次は保証しません。

- 親 Node の `LoadRoutine(...)` 完了
- 親 Node の `InitializeRoutine(...)` 完了
- 親の scene / UI / manager の構築完了

そのため、`PreLoad` は「今すぐ表示できる状態を作る」APIではなく、「事前に読めるものだけを読む」APIとして使うのが安全です。

### `UnPreLoad`

`UnPreLoad(target)` は preload 状態を解除します。  
ただし、その Node や親 path がまだ使用中なら、すぐには解放されません。

## 自前の Node を実装するときの注意点

- `Lifecycle` と `Router` の責務を混ぜない
- `LoadRoutine(...)` に親依存の初期化を書かない
- 表示中だけ必要な購読やハンドラは `Activate(...)` / `Deactivate(...)` に寄せる
- `LoadRoutine(...)` で確保したものは `Unload(...)` で解放する
- `Configure(...)` には、その Node 配下で共有したい依存だけを置く
- Node ID は一箇所で管理する
- `NavigationEngine` をアプリ全体で直接散発的に触るより、用途別の窓口クラスを 1 つ用意したほうが扱いやすい

## どの Node を選ぶか

- アプリ全体で 1 つだけ持つまとまり
  - `RootNode`
- 大きな画面グループ、シーン単位のまとまり
  - `SessionNode`
- 実際の画面、ダイアログ、子画面
  - `ScreenNode`

シーン切り替えを伴うセッションには、`SceneSessionNode` や `AdditiveSceneSessionNode` も利用できます。

## まとめ

`NavigationSystem` は、「画面遷移」だけでなく「画面の所属構造」「ロード順序」「解放順序」をまとめて扱うための仕組みです。

利用時は次の考え方を基準にすると扱いやすくなります。

- 構造は `Lifecycle`
- 遷移ルールは `Router`
- 読み込みは `LoadRoutine(...)`
- 親依存の初期化は `InitializeRoutine(...)`
- 利用時の runtime 開始は `Standby(...)`
- 未使用化時の runtime 解放は `Release()`
