# CutsceneSystem

## このドキュメントの対象
このドキュメントは、`GameFramework` Package を導入したプロジェクトで `CutsceneSystem` を使い始める人向けの概要である。

SampleGame 固有の仕組みや、個別プロジェクトの前提は扱わない。Package 単体で分かる範囲に絞って説明する。

## これは何をする機能か
`CutsceneSystem` は、Timeline を使った演出をコードから再生、停止、再利用するための機能である。

主に次の用途で使う。

- Timeline を Prefab から再生したい
- Scene 上に置いた Timeline を再生したい
- 演出の開始位置をずらしたい
- 演出の再生、停止、破棄をコードで管理したい

## 最初に知っておくこと
`CutsceneSystem` を使うときは、まず `CutsceneManager` を使う。

`CutsceneManager` には使い方が 2 つある。

- `Play(...)`
  - すぐ再生したいときに使う
- `GetHandle(...)`
  - 先に取得しておいて、あとから `Play()` や `Stop()` を呼びたいときに使う

返ってくる `Handle` から、次のような操作ができる。

- `Play()`
- `Stop()`
- `SetTime(float)`
- `IsPlaying`
- `Dispose()`

## 何を用意すれば使えるか
カットシーンとして使う対象は、次のどちらかを用意する。

### 1. `PlayableDirector` を持つ GameObject
もっともシンプルな使い方である。Prefab や Scene に `PlayableDirector` があれば、そのまま再生対象にできる。

### 2. `Cutscene` を継承したコンポーネント
再生開始時に追加処理を入れたい、シーク時に独自の同期をしたい、といった場合はこちらを使う。

## もっとも基本的な使い方

### Prefab をすぐ再生する
Prefab に `PlayableDirector` または `Cutscene` が付いていれば、次のように再生できる。

```csharp
var handle = cutsceneManager.Play(cutscenePrefab);
```

これがいちばん簡単な使い方である。

## 少し制御したいときの使い方

### 取得してから再生する
再生前に位置を変えたい、開始タイミングを自分で決めたい場合は `GetHandle(...)` を使う。

```csharp
var handle = cutsceneManager.GetHandle(cutscenePrefab);
handle.Play();
handle.Dispose();
```

`GetHandle(...)` で取得した `Handle` は、使い終わったら `Dispose()` する。

### 開始位置をずらして再生する
Timeline の途中から始めたい場合は、`Play()` の前に `SetTime(...)` を呼ぶ。

```csharp
var handle = cutsceneManager.GetHandle(cutscenePrefab);
handle.SetTime(1.25f);
handle.Play();
handle.Dispose();
```

## Scene 上のカットシーンを使う
Scene に置いた `PlayableDirector` や `Cutscene` も再生できる。

```csharp
var handle = cutsceneManager.Play(targetScene);
```

もう少し制御したい場合は `GetHandle(...)` を使う。

```csharp
var handle = cutsceneManager.GetHandle(targetScene);
handle.Play();
handle.Dispose();
```

Scene から取得する場合、対象の Scene には次のどちらかが必要である。

- `ICutscene` を実装した `MonoBehaviour`
- `PlayableDirector`

## 独自処理を入れたい場合
独自の処理を入れたいときは `Cutscene` を継承する。

```csharp
using GameFramework.CutsceneSystem;

public sealed class BossIntroCutscene : Cutscene {
    protected override void PlayInternal() {
        // 再生開始時の独自処理
    }

    protected override void StopInternal() {
        // 停止時の独自処理
    }

    protected override void SeekInternal(float time) {
        // シーク時の独自処理
    }
}
```

この場合も、同じ GameObject に `PlayableDirector` を付けて使う。

## どちらを使えばよいか

### `PlayableDirector` だけで十分な場合
次のようなときは、`PlayableDirector` だけで始めて問題ない。

- まずは Timeline を再生できればよい
- 特別なコード処理を追加しない
- 演出内容の大半が Timeline 側で完結している

### `Cutscene` を使うとよい場合
次のようなときは `Cutscene` 継承を検討するとよい。

- 再生開始時にコードを挟みたい
- 停止時に後片付けしたい
- シーク時に他の値も同期したい
- Timeline の外側にあるオブジェクトと連動したい

## 注意点

### `GetHandle(...)` を使ったら `Dispose()` する
`GetHandle(...)` は自動破棄ではないため、使い終わったら `Dispose()` が必要である。

特に Scene を unload する前には、関連する `Handle` を先に破棄しておくのが安全である。

### `Play(...)` はすぐ再生される
`Play(...)` は取得と同時に再生する。再生前に何か設定したい場合は `GetHandle(...)` を使う。

### `SetTime(...)` は `Play()` の前にも使える
再生前に `SetTime(...)` を呼ぶと、その位置から開始できる。

### `Cutscene` を使うなら `PlayableDirector` も必要
`Cutscene` は Timeline 再生の制御に `PlayableDirector` を使うため、単体では動かない。

## まとめ
`CutsceneSystem` は、Timeline 演出を Package 利用側から扱いやすくするための再生管理機能である。

最初は次の覚え方で十分である。

- すぐ再生するなら `Play(...)`
- あとから操作するなら `GetHandle(...)`
- 独自処理が必要なら `Cutscene` 継承
- `GetHandle(...)` を使ったら最後に `Dispose()`
