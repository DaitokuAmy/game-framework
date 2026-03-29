# AssetSystem 概要

## 対象
本ドキュメントは `Packages/com.daitokuamy.gameframework/Scripts/Runtime/AssetSystem` に含まれる `AssetSystem` を、Package 利用者向けに説明する。

## これは何か
`AssetSystem` は、アセットや Scene の読み込み方法を 1 つの形にそろえるための仕組みである。

たとえば、プロジェクトによって次のように読み込み元が異なることがある。

- `Resources`
- `Addressables`
- `AssetDatabase` での Editor 実行

`AssetSystem` を使うと、呼び出し側は「どこから読むか」の違いをできるだけ意識せずに、`LoadAsync(...)` と `Unload(...)` を中心に扱える。

## 最初に覚えること
Package 利用者が最初に覚えるのは、次の 3 つだけでよい。

- `Loader`
  - どこから読むかを担当する
- `Storage`
  - 読み込みとキャッシュをまとめて管理する
- `Request`
  - 何を読みたいかを表す

普段使うときは、まず `Storage` を作り、そこへ `Request` を渡して読み込む。

ただし、`Storage` は通常「読み込みのたびに毎回 new するもの」ではない。  
多くの場合は、ある機能や service が `Storage` を保持し、その寿命の中で複数回 `LoadAsync(...)` を呼ぶ。

## まず何を選べばよいか

### 1. どの Loader を使うか
用途ごとの目安は次の通りである。

- `ResourcesAssetLoader`
  - `Resources` フォルダから読み込みたい
- `AddressablesAssetLoader`
  - Addressables を使ってアセットを読み込みたい
- `AssetDatabaseAssetLoader`
  - Editor 実行中に AssetDatabase から直接読みたい
- `AddressablesSceneLoader`
  - Addressables で Scene を読み込みたい
- `AssetDatabaseSceneLoader`
  - Editor 実行中に Scene を直接読みたい

複数の loader を storage に渡すこともできる。その場合は、渡した順に「読めるか」が判定され、最初に使える loader が選ばれる。

### 2. どの Storage を使うか
storage の選び方は次の理解で十分である。

- `SimpleAssetStorage` / `SimpleSceneStorage`
  - 自分で `Unload(...)` するまで保持したい
- `LruAssetStorage` / `LruSceneStorage`
  - 上限数を決めて、古いものから自動で外したい

迷ったら、まずは `SimpleAssetStorage` または `SimpleSceneStorage` から始めると分かりやすい。

## 一番簡単な使い方

### アセットを読む
`Resources` から prefab を読む例を、`Storage` を所有する class も含めて示す。

```csharp
using GameFramework.AssetSystem;
using UnityEngine;

public sealed class TitlePrefabRepository : System.IDisposable {
    private readonly SimpleAssetStorage _storage;

    public TitlePrefabRepository() {
        _storage = new SimpleAssetStorage(new ResourcesAssetLoader());
    }

    public IEnumerator LoadAsync() {
        var process = _storage.LoadAsync<GameObject>("UI/pfb_title");
        yield return process;

        var prefab = process.Result;
    }

    public void Unload() {
        _storage.Unload<GameObject>("UI/pfb_title");
    }

    public void Dispose() {
        _storage.Dispose();
    }
}
```

ここで大事なのは次の流れである。

1. owner class が `Storage` を保持する
2. 必要なタイミングで `LoadAsync(...)` する
3. `yield return process` で完了を待つ
4. `process.Result` で結果を受け取る
5. 不要になったら `Unload(...)` または owner ごと `Dispose()` する

### Scene を読む
Scene の読み込みもほぼ同じである。

```csharp
using GameFramework.AssetSystem;

public sealed class FieldSceneLoader : System.IDisposable {
    private readonly SimpleSceneStorage _storage = new SimpleSceneStorage(new AddressablesSceneLoader());

    public IEnumerator LoadAsync() {
        var process = _storage.LoadAsync(new SceneRequest("scn_field_a"));
        yield return process;

        var scene = process.Scene;
    }

    public void Unload() {
        _storage.Unload(new SceneRequest("scn_field_a"));
    }

    public void Dispose() {
        _storage.Dispose();
    }
}
```

Scene は Additive 読み込み前提である。`Single` でのシーン遷移は `AssetSystem` の責務ではない。

## Scene をあとから表示したいとき
Scene は `ActivateOnLoad = false` を使うと、読み込み完了と表示開始を分けられる。

```csharp
using GameFramework.AssetSystem;

public sealed class BossScenePreloader : System.IDisposable {
    private readonly SimpleSceneStorage _storage = new SimpleSceneStorage(new AddressablesSceneLoader());

    public IEnumerator PreloadAsync() {
        var process = _storage.LoadAsync(new SceneRequest("scn_boss", activateOnLoad: false));
        yield return process;

        // 必要になったタイミングで表示開始
        yield return process.ActivateAsync();
    }

    public void Unload() {
        _storage.Unload(new SceneRequest("scn_boss", activateOnLoad: false));
    }

    public void Dispose() {
        _storage.Dispose();
    }
}
```

「先に読み込んでおき、見せるタイミングだけ後ろにずらしたい」場合はこの形を使う。

## Loader を複数使いたいとき
Editor では `AssetDatabase` を優先し、ビルドでは Addressables を使いたい、といった構成もできる。

```csharp
using GameFramework.AssetSystem;

AssetStorage CreateStorage() {
    return new SimpleAssetStorage(
#if UNITY_EDITOR
        new AssetDatabaseAssetLoader(),
#endif
#if USE_ADDRESSABLES
        new AddressablesAssetLoader(),
#endif
        new ResourcesAssetLoader()
    );
}
```

このときのポイントは、storage に渡した順がそのまま優先順になることだけである。

## `Simple` と `LRU` の使い分け

### `Simple`
`SimpleAssetStorage` と `SimpleSceneStorage` は、明示的に外すまで保持する。

次のようなケースに向いている。

- 画面や機能ごとに読み込んだものを自分で管理したい
- どこで解放するかを明確にしたい
- まずは分かりやすく使い始めたい

### `LRU`
`LruAssetStorage` と `LruSceneStorage` は、上限を超えたときに古いものから自動で外す。

次のようなケースに向いている。

- キャッシュ数を制限したい
- 古いものは自動で外れてよい
- 明示的な保持期間よりもメモリ上限を優先したい

## よくある設計
Package 利用者としては、storage を「1 個だけ全体共有する」より、「その機能が責任を持てる単位で所有する」方が扱いやすいことが多い。

たとえば次のような単位で持つと、解放タイミングを決めやすい。

- ある UI 機能専用の storage
- ある repository 専用の storage
- あるシーン管理クラス専用の storage

storage を破棄すると、内部で保持していたハンドルもまとめて解放される。

逆に、`LoadAsync(...)` を呼ぶたびに毎回 `Storage` を new すると、キャッシュの意味が薄くなり、どこで解放するかも追いにくくなる。  
まずは「owner が 1 つの storage を持つ」と考えるのが分かりやすい。

## Request について
最初は文字列 request だけで十分である。

```csharp
var process = storage.LoadAsync<Texture2D>("Icons/icn_sword");
```

ただし、アドレスの組み立てを毎回書きたくない場合は、独自 request struct を作ることもできる。

```csharp
using GameFramework.AssetSystem;
using UnityEngine;

public readonly struct IconRequest : IAssetRequest<Sprite> {
    public string Address { get; }
    public bool IsValid => !string.IsNullOrEmpty(Address);

    public IconRequest(string iconName) {
        Address = $"Icons/{iconName}";
    }
}
```

これを使うと、呼び出し側は「どういう命名規則で保存されているか」を知らずに済む。

## 実装上の注意点

### 解放は storage 経由で行う
利用側は loader の生ハンドルを直接扱わない。基本的には次のどちらかで解放する。

- `Unload(...)`
- `Dispose()` / `Clear()`

### 同じ request は同じキャッシュを再利用する
`SimpleAssetStorage` と `SimpleSceneStorage` では、同じ request を渡すと保持済みの結果を再利用する。

同じアドレスでも別々の寿命で管理したい場合は、storage 自体を分ける方が分かりやすい。

### Scene は Additive 専用
`SceneRequest` は Additive 読み込み専用である。画面遷移全体の制御までは担当しない。

## まとめ
`AssetSystem` は、「読み込み元の違い」を `Loader` に閉じ込め、「使い方の窓口」を `Storage` にそろえるための仕組みである。

使い始めるときは、次の順で考えると迷いにくい。

1. 読み込み元に合った loader を選ぶ
2. `Simple` か `LRU` の storage を選ぶ
3. `LoadAsync(...)` して `Result` を受け取る
4. 使い終わったら `Unload(...)` または `Dispose()` する
