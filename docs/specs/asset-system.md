# AssetSystem 仕様メモ

## 目的
本ドキュメントは、現在の `AssetSystem` の設計方針を整理するための仕様メモである。

## 対象範囲
- `Packages/com.daitokuamy.gameframework/Scripts/Runtime/AssetSystem`
- Asset 読み込み API
- Scene 読み込み API
- Loader と Storage の責務分担

## 設計目標
### 1. Request を軽くする
Request は「何を読みたいか」を表す値オブジェクトとして扱う。
Request 自身が backend の選択順やロード戦略を持ちすぎないようにする。

### 2. Loader を薄くする
Loader は単一 backend に対する入出力境界として扱う。
責務は原則として次に限定する。
- 読み込み可能かの判定
- 読み込み開始

### 3. Storage に戦略を寄せる
複数 Loader の選択、fallback、キャッシュ戦略は Storage 側で扱う。

### 4. Scene を簡素化する
AssetSystem が扱う Scene は Additive 読み込み前提とし、`Single` 遷移は別責務として分離する。

## 現在の方針
### 命名
- 境界インターフェースは `Loader` を使う
- Asset 系と Scene 系の型は分ける

### AssetRequest
`AssetRequest<TAsset>` は Asset の取得要求を表す値オブジェクトとする。
現在の最小構成は次の通りである。
- `Address`
- `IsValid`

`AssetRequest<TAsset>` は文字列から暗黙変換できる convenience request として扱う。

Storage の public API には次の 2 系統を持たせる。
- `AssetRequest<TAsset>` を直接受ける overload
- 専用 request struct を受ける `TRequest` overload

理由は次の通りである。
- `LoadAsync<GameObject>("...")` のような自然な呼び出しを成立させる
- 専用 request struct もそのまま扱えるようにする
- `LoadAsync<GameObject, AssetRequest<GameObject>>("...")` のような冗長な呼び出しを避ける

### SceneRequest
`SceneRequest` は Additive 読み込み専用の値オブジェクトとする。
現在の最小構成は次の通りである。
- `Address`
- `ActivateOnLoad`
- `IsValid`

`LoadSceneMode.Single` は扱わない。

### Loader
`IAssetLoader` は次の責務を持つ。
- `CanLoad`
- `LoadAsync`

`ISceneLoader` も同様に次の責務を持つ。
- `CanLoad`
- `LoadAsync`

解放は Loader ではなく handle 側で扱う。

### SceneStorage の戻り値
`SceneStorage` は backend の生 handle を外へ返さず、`ISceneProcess` を返す。
理由は次の通りである。
- `Release` は Storage 側で管理したい
- Scene だけは `ActivateAsync` を外へ見せたい
- `IProcess<Scene>` だけだと手動アクティブ化を表現できない

## 実装済みの型
- `AssetRequest<TAsset>`
- `SceneRequest`
- `IAssetRequest<TAsset>`
- `ISceneRequest`
- `IAssetLoadHandle<TAsset>`
- `ISceneLoadHandle`
- `ISceneProcess`
- `IAssetLoader`
- `ISceneLoader`
- `AddressablesAssetLoader`
- `AddressablesSceneLoader`
- `ResourcesAssetLoader`
- `AssetDatabaseAssetLoader`
- `AssetDatabaseSceneLoader`
- `AssetStorage`
- `SimpleAssetStorage`
- `LruAssetStorage`
- `SceneStorage`
- `SimpleSceneStorage`
- `LruSceneStorage`

## Storage の基本方針
### AssetStorage
`AssetStorage` の基底 API は次を基本とする。
- `LoadAsync<TAsset>(AssetRequest<TAsset> request)`
- `LoadAsync<TAsset, TRequest>(TRequest request)`
- `Unload<TAsset>(AssetRequest<TAsset> request)`
- `Unload<TAsset, TRequest>(TRequest request)`
- `Clear()`

SampleGame 側では `MainSystem` が Loader や Storage を共有しない。
Repository や Loader などの consumer が `AssetUtility` 経由で必要な Loader 群を生成し、自身の Storage を所有する。
これにより `Unload` と `Dispose` の責務を consumer 単位で閉じる。

### SceneStorage
`SceneStorage` の基底 API は次を基本とする。
- `LoadAsync(SceneRequest request)`
- `LoadAsync<TRequest>(TRequest request)`
- `Unload(SceneRequest request)`
- `Unload<TRequest>(TRequest request)`
- `Clear()`

### SimpleAssetStorage
`SimpleAssetStorage` は明示的に `Unload` するまで保持する単純なキャッシュ Storage とする。
性質は次の通りである。
- Storage 内では参照カウントしない
- 同じ request を何度読み込んでも、Storage が保持するキャッシュを返す
- `Unload` された時点で、Storage 内の都合で handle を解放する

`SimpleAssetStorage` は保持が明示的に管理されるため、同期取得 API を持ってよい。

### LruAssetStorage
`LruAssetStorage` は上限付きキャッシュを持ち、古い entry から自動的に解放する Storage とする。
性質は次の通りである。
- Storage 側で使用順を追跡する
- 上限超過時に LRU entry を解放する
- `Unload` と `Clear` は明示解放として扱う

`LruAssetStorage` には同期取得 API を持たせない。

### SimpleSceneStorage
`SimpleSceneStorage` は明示的に `Unload` するまで保持する単純な Scene キャッシュ Storage とする。
性質は次の通りである。
- 同じ address の request を何度読み込んでも、保持済みの Scene を返す
- `ActivateOnLoad = false` で保持中でも、後続 request が `true` なら `ActivateAsync` を呼ぶ
- `Unload` された時点で、Storage 内の都合で handle を解放する

### LruSceneStorage
`LruSceneStorage` は上限付き Scene キャッシュを持ち、古い entry から自動的に解放する Storage とする。
性質は次の通りである。
- Storage 側で使用順を追跡する
- 上限超過時に LRU entry を解放する
- `Unload` と `Clear` は明示解放として扱う

## 補足
- fallback の具体順序は Request ではなく Storage 側に寄せる
- `Single` シーン遷移は AssetSystem ではなく Scene 遷移責務で扱う
- 生 handle を利用側へ直接返さず、Storage 側で寿命を管理する
