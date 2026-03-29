# AssetSystem 概要

## 対象
本ドキュメントは `Packages/com.daitokuamy.gameframework/Scripts/Runtime/AssetSystem` に含まれる `AssetSystem` の概要をまとめる。

## 役割
`AssetSystem` は、アセットと Scene の読み込み口を統一しつつ、backend ごとの差異とキャッシュ戦略を分離するための仕組みである。

このシステムは次の責務を持つ。

- `AssetRequest<TAsset>` / `SceneRequest` で読み込み要求を値として表現する
- `IAssetLoader` / `ISceneLoader` で backend ごとの読み込み境界を分離する
- `AssetStorage` / `SceneStorage` で loader 選択とキャッシュ戦略を管理する
- アセットと Scene の寿命管理を consumer 単位の storage に閉じる
- Scene の手動アクティブ化を `ISceneProcess` 経由で扱えるようにする

## 設計の考え方

### Request は軽く保つ
Request は「何を読みたいか」を表す値オブジェクトであり、backend 選択やキャッシュ戦略は持たない。

`AssetRequest<TAsset>` は `Address` を持つ最小構成で、文字列から暗黙変換できる。

`SceneRequest` は `Address` と `ActivateOnLoad` を持ち、Additive 読み込み専用として扱う。

### Loader は単一 backend の境界
`IAssetLoader` と `ISceneLoader` の責務は、基本的に次の 2 つに絞られている。

- その request を読み込めるか判定する
- 実際の読み込みを開始する

backend の解放方法は handle 側に閉じ込め、呼び出し側には直接露出しない。

### Storage に戦略を寄せる
複数 loader の並び順、fallback、キャッシュ保持期間、明示解放か自動退避かといった戦略は storage 側で扱う。

このため、利用側は「どの loader を使うか」よりも「どの storage を所有するか」を意識すればよい。

## 主要な構成

### `AssetRequest<TAsset>` / `IAssetRequest<TAsset>`
アセット読み込み要求を表す型である。標準では `Address` だけを持つが、SampleGame のように独自 request struct を追加して、アドレス組み立てを request 側へ寄せる使い方もできる。

### `SceneRequest` / `ISceneRequest`
Scene 読み込み要求を表す型である。`ActivateOnLoad` を持つため、読み込み完了とアクティブ化を分けて扱える。

### `IAssetLoader` / `ISceneLoader`
各 backend ごとの実装境界である。現在は次の loader が用意されている。

- `ResourcesAssetLoader`
- `AddressablesAssetLoader`
- `AssetDatabaseAssetLoader`
- `AddressablesSceneLoader`
- `AssetDatabaseSceneLoader`

どの loader を優先するかは固定ではなく、storage に渡した配列順で決まる。

### `IAssetLoadHandle<TAsset>` / `ISceneLoadHandle`
loader が返す backend 依存の生ハンドルである。storage 内部ではこれを保持してキャッシュし、解放時に `Release()` を呼ぶ。

### `AssetStorage`
アセット読み込みの基底 storage である。request を検証し、渡された loader 群の先頭から `CanLoad(...)` を評価して、最初に読める loader へ委譲する。

public API は大きく次の 2 系統を持つ。

- `LoadAsync<TAsset>(AssetRequest<TAsset> request)`
- `LoadAsync<TAsset, TRequest>(TRequest request)`

`Unload(...)` と `Clear()` も同様に用意されており、storage が保持しているハンドルをまとめて解放できる。

### `SceneStorage`
Scene 読み込みの基底 storage である。構造は `AssetStorage` と近いが、戻り値は `ISceneProcess` になる。

これは Scene だけが `ActivateAsync()` を外に見せる必要があるためである。

### `SimpleAssetStorage`
明示的に `Unload` するまで保持する単純なキャッシュ storage である。

- 同じ request では同じキャッシュを再利用する
- 参照カウントは持たない
- 読み込み済みなら `TryGet(...)` / `GetAsset(...)` で同期取得できる

### `LruAssetStorage`
上限数を超えたとき、もっとも古く使われていないアセットから解放する storage である。

- 使用順を内部で追跡する
- キャッシュヒット時に LRU 順を更新する
- `Unload(...)` と `Clear()` による明示解放もできる

### `SimpleSceneStorage`
Scene を明示的に `Unload` するまで保持する storage である。

- 同じ address の request では同じ Scene を再利用する
- 先に `ActivateOnLoad = false` で保持していても、後続 request が `true` なら `ActivateAsync()` を呼ぶ

### `LruSceneStorage`
Scene 用の LRU storage である。Scene でもアドレス単位で使用順を管理し、上限超過時に古い Scene から解放する。

## データの流れ

### アセット読み込み
アセット読み込みは次の流れで進む。

1. 利用側が `AssetRequest<TAsset>` または独自 request struct を作る
2. `AssetStorage.LoadAsync(...)` が request の妥当性を確認する
3. 渡された loader 群を順に見て、`CanLoad(...)` が true の loader を選ぶ
4. loader が `IAssetLoadHandle<TAsset>` を返す
5. storage がその handle をキャッシュし、`IProcess<TAsset>` として利用側へ返す

利用側は backend を直接意識せず、storage に対して `LoadAsync(...)` と `Unload(...)` を呼ぶだけでよい。

### Scene 読み込み
Scene 読み込みも概ね同じだが、戻り値は `ISceneProcess` になる。

1. `SceneRequest` または独自 `ISceneRequest` 実装を作る
2. `SceneStorage.LoadAsync(...)` が読み込み可能な loader を選ぶ
3. loader が `ISceneLoadHandle` を返す
4. storage が handle を保持し、`ISceneProcess` を返す
5. `ActivateOnLoad = false` の場合は、必要なタイミングで `ActivateAsync()` を呼ぶ

Scene は Additive 読み込み前提であり、`LoadSceneMode.Single` による遷移はこのシステムの責務外である。

## SampleGame での組み立て
SampleGame では `Assets/SampleGame/Scripts/Runtime/Infrastructure/Common/AssetUtility.cs` で loader 群を構築している。

現在の組み立て順は次の通りである。

- Asset: `AssetDatabaseAssetLoader` → `AddressablesAssetLoader` → `ResourcesAssetLoader`
- Scene: `AssetDatabaseSceneLoader` → `AddressablesSceneLoader`

この順序により、Editor 上ではまず `AssetDatabase` を優先し、実運用では Addressables や Resources に自然に流れる構成にしている。

また、SampleGame では UI やテーブル、キャラ prefab、フィールド scene ごとに独自 request struct を用意し、呼び出し側が生のアドレス文字列を組み立てなくても済むようにしている。

## 利用イメージ

### 文字列 request をそのまま使う場合
```csharp
using var storage = new SimpleAssetStorage(AssetUtility.CreateAssetLoaders());
var process = storage.LoadAsync<GameObject>("Assets/SampleGame/UI/Root/pfb_ui_title.prefab");
yield return process;

var prefab = process.Result;
storage.Unload<GameObject>("Assets/SampleGame/UI/Root/pfb_ui_title.prefab");
```

### 独自 request struct を使う場合
```csharp
using var storage = new SimpleAssetStorage(AssetUtility.CreateAssetLoaders());
var process = storage.LoadAsync<GameObject, UIPrefabAssetRequest>(new UIPrefabAssetRequest("title"));
yield return process;

var prefab = process.Result;
```

### Scene を手動アクティブ化したい場合
```csharp
using var storage = new SimpleSceneStorage(AssetUtility.CreateSceneLoaders());
var process = storage.LoadAsync(new UISceneRequest("title"));
yield return process;

yield return process.ActivateAsync();
```

## 実装時の注意点

### storage が寿命管理を持つ
利用側が backend の生 handle を握る設計ではないため、解放は `Unload(...)`、`Clear()`、`Dispose()` を通して行う。

consumer ごとに storage を所有すると、どこで解放責務を持つかが明確になる。

### request が同じなら同じキャッシュを再利用する
`SimpleAssetStorage` と `SimpleSceneStorage` は、同じ request に対して同じ保持済みハンドルを返す。

そのため、同じアドレスを複数箇所で個別に寿命管理したい場合は、storage 自体を分ける設計が向いている。

### `ActivateOnLoad = false` は「読み込み完了」と「表示可能」を分ける
特に `AssetDatabaseSceneLoader` では、アクティブ化前でも `IsDone` が true になりうる。

「ロード完了後に任意のタイミングで見せたい」Scene ではこの挙動を前提に、必要なところで `ActivateAsync()` を呼ぶ。

### loader の優先順が挙動を決める
どの backend を使うかは request ではなく、storage に渡す loader 順で決まる。

Editor と実機で期待する backend が異なる場合は、`AssetUtility` のような組み立てポイントで順序を統一しておくと把握しやすい。

## まとめ
`AssetSystem` は、request を軽く、loader を薄く、storage を戦略の中心に置くことで、複数 backend にまたがる読み込みを統一的に扱うシステムである。

使い分けの基準は次の通りである。

- 読み込んだものを明示的に保持したいなら `SimpleAssetStorage` / `SimpleSceneStorage`
- 上限付きキャッシュで自動退避したいなら `LruAssetStorage` / `LruSceneStorage`
- backend 固有処理を増やしたいなら `IAssetLoader` / `ISceneLoader` の実装追加
- 呼び出し側から文字列アドレスを隠したいなら独自 request struct の追加
