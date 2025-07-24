# AssetSystems

このフォルダは、ゲーム内のアセット（Prefab, Texture, Scene等）管理・非同期ロード・プロバイダ切替のための各種クラス・コンポーネントを収録しています。

## 主要クラス・インターフェース概要

### AssetManager
- **役割**: 複数のアセットプロバイダ（IAssetProvider）をまとめて管理し、アセットのロード要求を振り分ける中心的なクラス。
- **主な機能**:
  - `Initialize(params IAssetProvider[])`：プロバイダの初期化・登録。
  - `GetProvider(int)`/`GetProvider<T>()`：プロバイダの取得。

### IAssetProvider
- **役割**: アセットのロード/アンロード/存在確認を抽象化するインターフェース。
- **主な実装例**:
  - `AddressablesAssetProvider`：Addressables対応
  - `ResourcesAssetProvider`：Resources.Load対応
  - `AssetDatabaseAssetProvider`：Editor用AssetDatabase対応
- **主なメソッド**:
  - `LoadAsync<T>(string)`/`Contains<T>(string)`/`LoadSceneAsync(string, LoadSceneMode)`/`ContainsScene(string)`

### AssetRequest<T> / SceneAssetRequest
- **役割**: アセット/シーンのロード要求を表現し、複数プロバイダへのフォールバックやアドレス指定を管理。
- **主な機能**:
  - `Address`/`ProviderIndices`：アドレス・プロバイダ優先順位指定。
  - `LoadAsync(AssetManager, IScope)`：非同期ロード・アンロードスコープ指定。

### AssetHandle<T> / SceneAssetHandle
- **役割**: 非同期ロード結果のハンドル。アセット/シーンの参照・解放・状態管理。
- **主な機能**:
  - `IsValid`/`IsDone`/`Asset`/`Release()` など

### AssetStorage / SceneAssetStorage
- **役割**: アセット/シーンのキャッシュ・プール・アンロード管理を行う基底クラス。
- **主な派生例**:
  - `SimpleAssetStorage`/`PoolAssetStorage`/`PreloadAssetStorage` など

---

## 使い方例

### 1. AssetManagerの初期化とアセットロード

```csharp
using GameFramework.AssetSystems;

// AssetManagerの初期化
var assetManager = new AssetManager();
assetManager.Initialize(new AddressablesAssetProvider(), new ResourcesAssetProvider());

// AssetRequestを作成（独自継承クラスでアドレス・プロバイダ指定）
var request = new MyPrefabAssetRequest();

// アセットを非同期ロード
var handle = request.LoadAsync(assetManager);

// ロード完了後にアセット取得
if (handle.IsDone && handle.IsValid) {
    var prefab = handle.Asset;
}

// 解放
handle.Release();
```

### 2. シーンアセットのロード

```csharp
var sceneRequest = new MySceneAssetRequest();
var sceneHandle = sceneRequest.LoadAsync(assetManager);

// シーンのアクティブ化
sceneHandle.ActivateAsync();

// 解放
sceneHandle.Release();
```

### 3. AssetStorageによるキャッシュ管理

```csharp
var storage = new PoolAssetStorage<MyAssetType>(assetManager, amount: 5);
var handle = storage.LoadAssetAsync(request);
var asset = storage.GetAsset(request);

// キャッシュ解放
storage.UnloadAssets();
```

---

## 備考

- 複数のIAssetProviderを組み合わせて、Addressables/Resources/Editor/独自ストレージ等に柔軟対応。
- AssetRequest/SceneAssetRequestを継承して、アドレス・プロバイダ優先順位をデータ駆動で管理可能。
- AssetStorageを使うことで、キャッシュ・プール・プリロード等の高度なアセット管理も実現できます。 