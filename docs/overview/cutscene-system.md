# CutsceneSystem 概要

## 対象
本ドキュメントは `Packages/com.daitokuamy.gameframework/Scripts/Runtime/CutsceneSystem` に含まれる `CutsceneSystem` の概要をまとめる。

## 役割
`CutsceneSystem` は、Timeline ベースのカットシーンを統一的に再生、停止、回収するための仕組みである。

このシステムは次の責務を持つ。

- `ICutscene` を通した再生制御の共通化
- Prefab ベースのカットシーンの Pool 管理
- Scene 上に常駐するカットシーンの取得と再利用
- `LayeredTime` による再生速度と更新時間の制御
- `Handle` を通した再生中インスタンスの寿命管理

## 主要な構成

### `ICutscene`
`ICutscene` はカットシーンの共通インターフェースであり、`Play`、`Stop`、`Update`、`SetSpeed`、`Seek` などの基本操作を定義する。

`CutsceneManager` は具体型に依存せず、このインターフェース経由でカットシーンを扱う。

### `Cutscene`
`Cutscene` は `MonoBehaviour` ベースの標準実装である。`PlayableDirector` を内部で保持し、派生クラスで `InitializeInternal`、`PlayInternal`、`StopInternal`、`SeekInternal` などを override して挙動を拡張できる。

Timeline に独自のバインドや再生開始時処理を足したい場合は、このクラスを継承する使い方が基本になる。

### `RuntimeCutscene`
`RuntimeCutscene` は `PlayableDirector` だけが置かれているケースを扱う軽量実装である。`ICutscene` 実装が見つからない場合に `CutsceneManager` が内部で生成して使う。

Scene や Prefab に専用の `Cutscene` コンポーネントを置かなくても、`PlayableDirector` があれば再生対象として扱える。

### `CutsceneManager`
`CutsceneManager` はシステムの中心となる管理クラスである。Prefab と Scene の両方から `CutsceneInfo` を作り、再生中の状態を `PlayingInfo` で管理する。

公開 API は大きく次の 2 系統に分かれる。

- `Play(...)`
  - 取得直後に再生する
  - 再生完了時は自動解放される
- `GetHandle(...)`
  - インスタンスだけ取得して、再生開始や停止は呼び出し側が制御する
  - 使用後は `Handle.Dispose()` が必要になる

## データの流れ

### Prefab ベース
Prefab を使う場合、`CutsceneManager` は内部 Pool から `CutsceneInfo` を取り出して再利用する。

- `ICutscene` 実装が Prefab にあればそれを使う
- 見つからなければ `PlayableDirector` を見つけて `RuntimeCutscene` を作る
- 再生終了または `Dispose` 時に `OnReturn` を呼び、非表示化して Pool に戻す

この構造により、短時間に何度も再生される演出でも生成コストを抑えやすい。

### Scene ベース
Scene を使う場合、`CutsceneManager` は対象 Scene の root object から `ICutscene` または `PlayableDirector` を探し、1 Scene につき 1 つの `CutsceneInfo` を保持する。

Scene ベースは Pool には戻さず、その Scene に属する常駐インスタンスを再利用する。新しい `Handle` を取り直したときは、同じ Scene に対する古い `PlayingInfo` は先に解放される。

## `Handle` の考え方
`Handle` は再生中インスタンスへの操作窓口であり、内部的には `CutsceneManager` が払い出した ID で対象を引く。

この方式には次の意味がある。

- 古い `Handle` が再利用済みインスタンスを誤って触らない
- `IsPlaying`、`Stop`、`SetTime` が解放済み対象に対して安全に no-op になる
- `Dispose` 時に即座に管理対象から外せる

`GetHandle(...)` を使った場合は、利用側が `Dispose` を責務として持つ。`Play(...)` を使った場合でも途中停止や明示破棄をしたいなら、返ってきた `Handle` をそのまま使える。

また、`Handle` は `IEventProcess` を実装しているため、停止完了の待機に利用できる。

## 再生と時間制御

### 更新モード
`ICutscene.Initialize(bool updateGameTime)` により、`PlayableDirector` の更新方法が決まる。

- `true`
  - `DirectorUpdateMode.GameTime` で Unity の時間に追従する
- `false`
  - `DirectorUpdateMode.Manual` で `CutsceneManager` の更新から進める

`CutsceneManager` 自体は `DisposableLateUpdatable` を継承しており、`LateUpdate` 相当のタイミングで再生中の `PlayingInfo` を更新する。

### `LayeredTime`
`LayeredTime` を渡すと、`deltaTime` と time scale がその値に従う。これにより、特定のカットシーンだけをスロー再生したり、ゲーム全体とは別の時間系で演出を進めたりできる。

### `SetTime`
`Handle.SetTime(float)` は `ICutscene.Seek(float)` を呼び出して再生位置を更新する。

再生前に `SetTime` を呼んだ場合、その値は開始位置として保持され、次の `Play()` はその時間から始まる。再生中に `SetTime` を呼んだ場合は、その場でシークされる。

manual update のときは `Seek` 時に即 `Evaluate` されるため、停止中でも指定位置の見た目を反映しやすい。

## 利用イメージ

### すぐ再生したい場合
```csharp
var handle = cutsceneManager.Play(prefab);
```

### 事前設定してから再生したい場合
```csharp
var handle = cutsceneManager.GetHandle(scene);
handle.SetTime(1.25f);
handle.Play();
handle.Dispose();
```

### 独自実装を使いたい場合
```csharp
public sealed class BossIntroCutscene : Cutscene {
    protected override void PlayInternal() {
        // 再生開始時の独自処理
    }

    protected override void SeekInternal(float time) {
        // シーク時の独自同期
    }
}
```

## 実装時の注意点

### Scene 側の `ICutscene` は `MonoBehaviour` 前提
Scene から取得する `ICutscene` 実装は、対応する `GameObject` を見つける必要があるため `MonoBehaviour` であることが前提になる。

### `GetHandle(...)` の後始末
`GetHandle(...)` で取得した `Handle` は、使い終わったら必ず `Dispose()` する。Scene unload より前に解放しておくと、破棄済みオブジェクトへのアクセスを避けやすい。

### Prefab と Scene で寿命管理が異なる
Prefab ベースは Pool に戻る前提であり、`OnReturn` で状態をリセットする設計が重要になる。一方で Scene ベースは同じ実体を使い回すため、`OnReturn` や `Stop` のあとに次回再生へ持ち越したくない状態を残さないことが重要になる。

### `Cutscene` 継承時は `PlayableDirector` が必要
`Cutscene` は内部で `PlayableDirector` を取得して使うため、同じ GameObject に `PlayableDirector` が存在する前提で使う。

## まとめ
`CutsceneSystem` は、Timeline 演出の実体差を `ICutscene` に隠しつつ、`CutsceneManager` と `Handle` で寿命管理まで含めて扱うためのシステムである。

使い分けの基準は次の通りである。

- すぐ再生して自動解放したいなら `Play(...)`
- 再生タイミングやシーク位置を自前で制御したいなら `GetHandle(...)`
- 独自処理を挟みたいなら `Cutscene` 継承
- 素の Timeline をそのまま扱いたいなら `PlayableDirector` + `RuntimeCutscene`
