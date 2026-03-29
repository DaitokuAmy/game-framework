# NavigationSystem Runtime / DI Lifecycle Spec

## 目的

`NavigationSystem` における Node の runtime 寿命と、`Configure` / `ObjectResolver` / `Inject` の寿命を整理するための仕様メモ。

本メモは、議論の履歴ではなく、最終的な取り扱い方針だけを記載する。

## 対象

- `NavNode`
- `NavNodeTree`
- `NavigationEngineBuilder`
- `PreLoad` / `UnPreLoad`
- VContainer 利用時の `Configure` / `ObjectResolver` / `Inject`

## 基本方針

- Node インスタンス自体は `Build()` 時に生成し、`NavigationEngine` の寿命中は常駐させる
- `Setup(...)` は構造構築専用とし、DI 初期化は行わない
- runtime context は利用時に `Standby(...)` で立ち上げ、未使用化時に `Release()` で破棄する
- `Shutdown(...)` は対になるライフサイクルではなく、Node を安全に `Release` 状態まで戻すための強制コマンドとして扱う

## ライフサイクル整理

### 構造フェーズ

- `Setup(...)`
  - `NodeId`
  - 親子関係
  - 構造上必要な参照

`Setup(...)` は 1 回だけ呼ばれる構造初期化であり、runtime context はここで作らない。

### runtime フェーズ

- `Standby(...)`
  - runtime context の開始
  - VContainer 利用時は resolver 生成、`Configure(...)`、`Inject(this)` をここに含む
- `LoadRoutine(...)`
- `InitializeRoutine(...)`
- `Activate(...)`
- `PreOpen(...)` / `OpenRoutine(...)` / `PostOpen(...)`
- `PreClose(...)` / `CloseRoutine(...)` / `PostClose(...)`
- `Deactivate(...)`
- `Terminate(...)`
- `Unload(...)`
- `Release()`
  - `Standby(...)` で立ち上げた runtime context の終了

### 強制終了

- `Shutdown(...)`
  - 現在の状態に応じて `Deactivate` / `Terminate` / `Unload` / `Release` を安全に流す
  - 目的は「その Node を未使用状態に戻す」こと

## `Standby` / `Release` の意味

`Standby > Release` を runtime context の開始と終了の対として扱う。

- `Standby(...)`
  - その Node が利用される前提を作る
- `Release()`
  - その Node が未使用になったあと、runtime context を破棄する

このため、`Configure` 由来の依存や `ObjectResolver` の寿命は、Node インスタンスの寿命ではなく、`Standby > Release` の区間に属する。

## `Setup(...)` の責務

`Setup(...)` で行うのは構造構築のみとする。

- 親子関係の接続
- `NodeId` の設定
- runtime に依存しない構造情報の保持

`Setup(...)` では次を行わない。

- `Configure(...)`
- resolver 生成
- `Inject(this)`
- `Standby(...)`

## 通常遷移の仕様

通常の `TransitionTo(...)` では、`LoadRoutine(...)` 実行前に、対象 Node までの path を親から順に `Standby(...)` する。

- 対象 path は `Root -> ... -> target`
- `Standby(...)` は `NavNodeTree` 側で path 制御する
- `NavNode` 自身は、自分自身の `Standby(...)` / `Release()` のみを担当する

不要になった Node は、`Unload(...)` 後に `Release()` 対象となる。

ただし、次のいずれかに該当する Node は保持する。

- 現在 `running` 中の path に含まれる
- いずれかの `PreLoad` path に含まれる

## `PreLoad` の仕様

### 基本動作

`PreLoad(target)` は target 単体の先読みではなく、target までの path を準備する操作として扱う。

`PreLoad(target)` で保証するのは次の内容である。

- `Root -> ... -> target` の path が `Standby(...)` 済みである
- target の `LoadRoutine(...)` が先行実行される

### 保証しないこと

`PreLoad(target)` は、親 Node の `LoadRoutine(...)` / `InitializeRoutine(...)` 完了を保証しない。

つまり、`PreLoad` 時点で保証されるのは次までである。

- 親 path の runtime context は存在する
- ただし親の構築済みリソースまでは保証しない

このため、親の構築処理に依存する初期化は `LoadRoutine(...)` ではなく `InitializeRoutine(...)` に記述する前提とする。

### `UnPreLoad`

`UnPreLoad(target)` は preload 参照を 1 つ解放する。

- 同一 target への複数 `PreLoad` は参照カウントで管理する
- 参照が 0 になり、かつ `running` 中でない場合のみ `Unload(...)` と `Release()` の対象にする
- 共通親を持つ別の preload path が残っている場合、親 Node は `Release()` しない

## VContainer 利用時の仕様

VContainer が利用可能な場合、resolver は `Standby(...)` 時に lazy 生成する。

### 生成順序

- `RootNode`
  - `Build(parentResolver)` で渡された親 resolver を親として scope を作る
- `SessionNode` / `ScreenNode`
  - 親 Node の resolver を親として scope を作る

つまり、resolver の親子関係は Node 階層と一致する。

### `Configure(...)`

`Configure(IContainerBuilder builder)` は、その Node の runtime context 用登録を行う場所とする。

登録した依存は、その Node と子孫 Node から参照できる。

### `Inject(this)`

`Inject(this)` は resolver 生成後、Node の `Standby(...)` 本体が呼ばれる前に実行する。

そのため、`Standby(...)`、`LoadRoutine(...)`、`InitializeRoutine(...)` では注入済み依存を利用できる。

### 解放

resolver は `Release()` で破棄する。

そのため、`Configure(...)` 由来の依存寿命は `Standby > Release` に一致する。

## 役割分担

### `NavNode`

- 自分自身の `Standby(...)`
- 自分自身の `Release()`
- 自分自身の `Load` / `Initialize` / `Activate` / `Unload`

### `NavNodeTree`

- path の `Standby(...)` 順序制御
- 遷移時の path 差分計算
- `PreLoad` / `UnPreLoad` の保持管理
- 不要 path の `Release()` 判定

## 非保証事項

次は `NavigationSystem` の保証対象に含めない。

- `PreLoad` が親の scene / UI / manager 構築まで完了していること
- preload 済み Node が常に即時表示可能であること
- preload 保持理由の内部表現

利用側は、親の構築完了が必要な処理を `InitializeRoutine(...)` へ寄せて扱うことを前提とする。
