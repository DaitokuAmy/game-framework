# NavigationSystem Overview

## 目的

`NavigationSystem` は、アプリ内の画面遷移を `RootNode` / `SessionNode` / `ScreenNode` の階層として管理する仕組みです。

- どの画面がどのスコープに属するかを `Lifecycle` として定義する
- 実際にどの順序で遷移できるかを `Router` として定義する
- ノードのロード、初期化、表示、終了、破棄の順序を `NavigationEngine` が統一的に制御する

単に「次の画面へ飛ぶ」だけではなく、画面の所属関係、戻り先、シーン切り替え、プリロードをまとめて扱えるのが特徴です。

## 基本構成

主要な構成要素は次のとおりです。

- `NavigationEngine`
  - 外部から利用する中心 API
  - `TransitionTo`, `Back`, `Reset`, `PreLoad`, `Update` などを提供する
- `NavNodeTree`
  - `Lifecycle` と現在の実行状態を保持する
  - 共通親を見つけて、閉じるノードと開くノードを決定する
- `INavNodeStateRouter`
  - 戻る操作やショートカットを含む遷移ルールを管理する
  - 実装として `NavNodeStackRouter` と `NavNodeTreeRouter` がある
- `NavNode`
  - すべてのノードの基底クラス
  - 各フェーズのフックをオーバーライドして振る舞いを実装する

## ノード階層

`NavigationSystem` は、ノードを次の役割で分けます。

- `RootNode`
  - アプリ全体のルート
- `SessionNode`
  - シーン単位、大きな機能単位、まとまった遷移単位
- `ScreenNode`
  - 実際の画面やダイアログに相当する単位

典型的な構造は次のようになります。

```text
RootNode
└─ SessionNode
   ├─ ScreenNode
   └─ ScreenNode
      └─ ScreenNode
```

`SessionNode` は `RootNode` 直下に置き、`ScreenNode` は `SessionNode` または別の `ScreenNode` の子として配置します。

## Lifecycle と Router の分担

`NavigationSystem` を理解するうえで重要なのが、`Lifecycle` と `Router` を分けて考えることです。

### Lifecycle

`Lifecycle` は、ノードの親子関係を定義します。

```csharp
var engine = NavigationEngineBuilder.Create()
    .CreateLifecycle<AppRootNode>(Id.Root, root => {
        root.AddSession<IntroductionSessionNode>(Id.Introduction, introduction => {
            introduction.AddScreen<TitleTopScreenNode>(Id.TitleTop)
                .AddScreen<TitleOptionScreenNode>(Id.TitleOption);
        });

        root.AddSession<OutGameSessionNode>(Id.OutGame, outGame => {
            outGame.AddScreen<SortieScreenNode>(Id.Sortie, sortie => {
                sortie.AddScreen<SortieTopScreenNode>(Id.SortieTop);
            });
        });
    });
```

この段階で決まるのは「どのノードがどの親に属するか」までです。  
まだ「どこからどこへ遷移できるか」は決まりません。

### Router

`Router` は、`Lifecycle` 上のノードに対して実際の遷移ルートを定義します。

```csharp
.CreateRouter(container =>
    NavNodeTreeRouterBuilder.Create()
        .AddRoot(Id.TitleTop, titleTop => {
            titleTop.Connect(Id.TitleOption)
                .Connect(Id.SortieTop)
                .SetGlobalShortcut();
        })
        .Build(container))
```

ここでは、たとえば次のような情報を持たせられます。

- どの画面からどの画面へ進めるか
- `Back()` したときにどこへ戻るか
- 特定のスコープ内だけで有効なショートカット
- どこからでも飛べるグローバルショートカット

`Lifecycle` が「所属関係」、`Router` が「遷移ルール」です。

## ノードのライフサイクル

`NavNode` には、生成時、遷移時、破棄時にそれぞれフックがあります。

### エンジン生成時に 1 回呼ばれるもの

- `Standby`
  - `NavigationEngine` の参照や DI 解決結果を受け取る初期待機

### 通常の遷移で使われるもの

- 開く側
  - `LoadRoutine`
  - `InitializeRoutine`
  - `PreOpen`
  - `OpenRoutine`
  - `PostOpen`
  - `Activate`
- 閉じる側
  - `Deactivate`
  - `PreClose`
  - `CloseRoutine`
  - `PostClose`
  - `Terminate`
  - `Unload`

用途の目安は次のとおりです。

- `LoadRoutine`
  - アセットやシーンの読み込み
- `InitializeRoutine`
  - 読み込み後の初期化
- `OpenRoutine` / `CloseRoutine`
  - 画面アニメーション
- `Activate` / `Deactivate`
  - 実行開始、実行停止
- `Terminate` / `Unload`
  - 終了処理とリソース解放

`ScreenNode` だけが `PreOpen` / `OpenRoutine` / `PostOpen` と `PreClose` / `CloseRoutine` / `PostClose` を持ち、表示演出を担当します。

なお、実際の呼び順は `ITransition` 実装に依存します。

- `OutInTransition`
  - 閉じる処理を終えてから次を開く
- `CrossTransition`
  - 読み込みや開閉演出を並行気味に進める

### エンジン破棄時に呼ばれるもの

- `Shutdown`
  - 強制終了時の後始末
- `Release`
  - `Standby` で確保した状態や DI スコープの解放

## 遷移時の挙動

`NavNodeTree` は、現在ノードと遷移先ノードの共通親を探し、そこを境に閉じるノード列と開くノード列を組み立てます。  
そのため、同じ `SessionNode` 配下の画面切り替えでは差分だけが処理され、別セッションへ移るときは親側も含めて再構築されます。

`TransitionOption` には `Refresh` フラグがあります。

- `Refresh == false`
  - 共通親を活かして差分遷移する
- `Refresh == true`
  - 共通親を使わず、ルート側から開き直す

アプリの再起動に近い画面復帰や、同じノードを完全に作り直したいときは `refresh: true` を使います。

## NavigationEngine の主要 API

- `TransitionTo<TScreen>(nodeId, ...)`
  - 指定ノードへ遷移する
  - 遷移前に `setupAction` で遷移先ノードを設定できる
- `Back(depth, ...)`
  - `Router` が持つ履歴を使って戻る
- `Reset(...)`
  - 現在のノード階層を閉じて同じ階層を再構築する
- `GetCurrentNode()`
  - 現在のノードを取得する
- `GetNodeInParent<TNode>()`
  - 現在ノードの親階層から特定型を探す
- `GetBackNodeInParent<TNode>()`
  - 戻り先の親階層から特定型を探す
- `TryGetChildNodeId<TNode>(out nodeId)`
  - 現在ノード配下から特定型のノード ID を探す
- `PreLoad(nodeId)` / `UnPreLoad(nodeId)`
  - 遷移前にロードだけ先行しておく

`Back()` は Router 前提の API なので、Router を構築しない場合は使えません。

## シーン遷移向けの SessionNode

`SessionNode` にはシーン切り替え用の派生基底が用意されています。

- `SceneSessionNode`
  - 指定シーンを `LoadSceneMode.Single` で読み込む
  - 必ず `OutInTransition` を使う
- `AdditiveSceneSessionNode`
  - 指定シーンを `LoadSceneMode.Additive` で読み込む
  - こちらも `OutInTransition` を使う

シーンをまたぐ大きな遷移を `SessionNode` として表し、その内部の画面差分を `ScreenNode` で表現するのが基本パターンです。

## VContainer 利用時の挙動

VContainer が導入されている場合、各 `NavNode` が `IObjectResolver` を持ち、親ノードから子ノードへ DI スコープが連鎖します。

### エンジン構築時

`NavigationEngineBuilder.Build()` は、VContainer 利用時に親 `IObjectResolver` を受け取れます。

```csharp
_engine = NavigationEngineBuilder.Create()
    .CreateLifecycle<AppRootNode>(Id.Root, SetupLifecycle)
    .CreateRouter(container => SetupRouter(container))
    .Build(globalResolver);
```

このときのスコープ生成は次のようになります。

- `RootNode`
  - `Build(globalResolver)` で渡した `globalResolver` を親として `CreateScope(Configure)` する
- `SessionNode` / `ScreenNode`
  - 親ノードの `ObjectResolver` を親として `CreateScope(Configure)` する

つまり、ノード階層と同じ形で VContainer の子スコープが作られます。

### `Configure` の役割

`NavNode` では `Configure(IContainerBuilder builder)` をオーバーライドして、そのノード専用の登録を追加できます。

```csharp
protected override void Configure(IContainerBuilder builder) {
    base.Configure(builder);

    builder.Register<ModelViewerDomainService>(Lifetime.Singleton);
    builder.Register<ModelViewerAppService>(Lifetime.Singleton);
}
```

この登録は、そのノード自身と子孫ノードから参照できます。  
`ModelViewerSessionNode` のように、セッション単位で Application / Domain / Infrastructure をまとめて登録する使い方が基本になります。

### `Inject` と `Standby` のタイミング

各ノードでは、スコープ生成後に `Standby(engine)` が呼ばれます。  
VContainer 利用時は、この `Standby` の直前に `ObjectResolver.Inject(this)` が実行されます。

そのため、次のようなフィールドインジェクションを使えます。

```csharp
[Inject]
private ModelViewerAppService _appService;
```

以降、`Standby`, `LoadRoutine`, `InitializeRoutine` などでは注入済みの依存を利用できます。

### 利用時の見方

- アプリ全体の共有依存
  - `Build(globalResolver)` に渡す親コンテナに登録する
- 特定セッションだけで使う依存
  - その `SessionNode.Configure()` に登録する
- 特定画面だけで使う依存
  - その `ScreenNode.Configure()` に登録する

この分け方にすると、ノードのライフサイクルと DI の寿命が一致しやすくなります。

### 破棄時

ノード破棄時の `Release()` 内で、そのノードの `ObjectResolver` も `Dispose()` されます。  
そのため、ノードスコープに閉じた依存はノード終了と一緒に解放されます。

## サンプル実装との対応

`Assets/SampleGame/Scripts/Runtime/Lifecycle/AppNavigator.cs` では、`NavigationEngineBuilder` を使って `Lifecycle` と `Router` を組み立てています。

- `SetupIntroductionLifecycle`
  - Introduction 配下のノード構造を定義する
- `SetupOutGameLifecycle`
  - OutGame 配下のノード構造を定義する
- `ConnectIntroductionTitleTopTreeNode`
  - Title 系画面からの遷移ルールを定義する
- `ConnectOutGameSortieTopTreeNode`
  - Sortie 系画面からの遷移ルールを定義する

また、`GetDefaultTransitionInfo()` では現在の `SessionNode` と遷移先の `SessionNode` を比較し、同一セッション内なら `CrossTransition`、別セッションなら `OutInTransition + LoadingEffect` に切り替えています。  
このように、`NavigationEngine` の補助ロジックをアプリ側のナビゲータにまとめると、UI からは用途別メソッドだけを呼べる構成にしやすくなります。

## 運用の目安

- ノード型は責務ごとに別ファイルへ分ける
- ノード ID は集中管理する
- `Lifecycle` と `Router` の責務を混ぜない
- 画面演出は `ScreenNode` に寄せる
- シーン単位の切り替えは `SessionNode` に寄せる
- 画面から直接 `NavigationEngine` を散発的に触らず、`AppNavigator` のような窓口を置く

この方針にすると、画面構造の把握、戻る動作の制御、再利用可能な遷移ロジックの共有がしやすくなります。
