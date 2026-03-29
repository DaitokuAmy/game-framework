# PlayableSystem 仕様メモ

## 目的
本ドキュメントは、現在の `PlayableSystem` の設計方針と更新モデルを整理するための仕様メモである。

## 対象範囲
- `Packages/com.daitokuamy.gameframework/Scripts/Runtime/PlayableSystem`
- `MotionPlayer`
- `MotionHandle`
- `MotionLayerHandler`
- `MotionCrossFader`
- `AnimationJobConnector`
- `TimelinePlayer`

## 何を担当するか

### `MotionPlayer`
- `Animator` 向けの `PlayableGraph` を構築する
- ベースレイヤーと拡張レイヤーを管理する
- `SkipFrame`、`DirectorUpdateMode`、再生速度を制御する

### `MotionHandle`
- ベースレイヤーまたは拡張レイヤーに対する操作窓口
- `Playable` の差し替えとレイヤー weight 制御を行う

### `MotionLayerHandler`
- ベースレイヤーと拡張レイヤーを束ねる
- 各レイヤーの `MotionCrossFader` を更新する

### `MotionCrossFader`
- `Playable` の接続、切り替え、クロスフェード、破棄を担当する
- 再生時間そのものは持たず、weight と port 管理に寄せる

### `AnimationJobConnector`
- `AnimationScriptPlayable` を既存 graph に直列接続する
- `IAnimationJobComponent` の寿命管理と順序制御を行う

### `TimelinePlayer`
- `PlayableDirector` を使った Timeline 再生を担当する
- `MotionPlayer` とは独立した単機能の player

## `MotionPlayer` の構造
`MotionPlayer` は 1 つの `Animator` に対して 1 つの `PlayableGraph` を持つ。

graph の root には `MotionLayerHandler` が生成する `AnimationLayerMixerPlayable` が接続される。ベースレイヤーと追加レイヤーは、その下にある `MotionCrossFader` が個別に管理する。

概念上の接続は次のとおりである。

```text
AnimationPlayableOutput
  -> AnimationLayerMixerPlayable
    -> base MotionCrossFader
    -> extension MotionCrossFader ...
```

Animation Job を使う場合は、`AnimationJobConnector` が output と root playable の間に `AnimationScriptPlayable` 群を差し込む。

## 更新モデル
`MotionPlayer.Update()` では、次の 2 種類の時間を扱う。

- `pendingEvaluateDelta`
  - graph にまだ反映していない実時間
- `pendingSimulationDelta`
  - クロスフェードや job component 更新に使う、speed 適用後の時間

この 2 つを分ける理由は、`Evaluate` に渡す時間と playable 側 speed の責務を分離するためである。

## `Manual` と非 `Manual`
`DirectorUpdateMode.Manual` とそれ以外では、graph を誰が進めるかが異なる。

### `Manual`
- graph は `Play()` しない
- `Update()` ごとに pending を蓄積する
- 反映フレームになったら `FlushPendingTime()` を実行する
- `FlushPendingTime()` は次をまとめて行う
  - `MotionLayerHandler.Update(pendingSimulationDelta)`
  - `AnimationJobConnector.Update(pendingSimulationDelta)`
  - `_graph.Evaluate(pendingEvaluateDelta)`

つまり `Manual` では、graph の進行責務は `MotionPlayer` 側にある。

### 非 `Manual`
- 通常時は `Play()` して Unity の更新サイクルに任せる
- `SkipFrame` 中は `Stop()` して pending だけ蓄積する
- 再開フレームでは `FlushPendingTime()` で停止中の時間を先に反映する
- その後、今フレーム分の simulation 更新を行い、`Play()` に戻す

つまり非 `Manual` では、通常の graph 進行は Unity 側に任せつつ、`SkipFrame` 中だけ `MotionPlayer` が補助的に時間を保持する。

## `SkipFrame` の考え方
`SkipFrame` は「ロジック時間を止めずに、骨反映を間引く」ための機能として扱う。

そのため、停止中も時間は pending に蓄積され、再開時にまとめて catch-up される。

現在の挙動は次のとおりである。

- スキップ中
  - graph を `Stop()` する
  - `pendingEvaluateDelta` と `pendingSimulationDelta` に時間を積む
- 再開時
  - 蓄積分を `FlushPendingTime()` でまとめて反映する
  - 非 `Manual` ならその後 `Play()` に戻す

この設計により、再生時間やクロスフェード時間を極力実時間に合わせたまま、評価頻度だけを落とせる。

## 再生速度
`MotionPlayer.SetSpeed()` は、次の 2 つに対して速度を反映する。

- `MotionLayerHandler`
- `AnimationJobConnector`

speed 変更時は、先に `FlushPendingTime()` を実行して未反映時間を旧 speed のまま消化する。これにより、speed 変更前に積まれていた pending が、新しい speed で誤って解釈されることを防ぐ。

## `MotionHandle` でできること
`MotionHandle` は、`MotionCrossFader` に対する操作窓口である。

- `Change(playable, blendDuration, autoDispose)`
  - 現在の `Playable` を差し替える
  - 必要に応じてクロスフェードする
- `SetWeight(weight)`
  - 拡張レイヤーの weight を変更する
- `GetWeight()`
  - 現在のレイヤー weight を取得する

`MotionHandle` は無効化後も安全に呼べるように設計されており、破棄済みハンドルに対しては no-op になる。

## `AnimationJobConnector` の役割
`AnimationJobConnector` は `IAnimationJobComponent` を順序付きで保持し、`AnimationScriptPlayable` を graph に直列接続する。

- `AddComponent(component, order)`
  - component を初期化して graph に登録する
- `Update(deltaTime)`
  - 無効化された component を除去する
  - 生存中の component に `Update(deltaTime)` を流す
- `SetSpeed(speed)`
  - 既存 playable の speed を更新する

component 本体が `deltaTime` をどう使うかは各実装次第である。`AnimationJobConnector` 自体は job の評価を直接進めるのではなく、component 更新と graph 接続管理を担当する。

## `TimelinePlayer` との違い
`TimelinePlayer` は `PlayableDirector` ベースの別系統である。`MotionPlayer` の graph 共有やクロスフェードには関与しない。

主な役割は次のとおりである。

- `Play(TimelineAsset, loop)`
  - Timeline を再生する
- `Stop()`
  - 再生を停止する
- `ManualUpdate(deltaTime)`
  - `DirectorUpdateMode.Manual` 時に手動で進める
- `SetSpeed(speed)`
  - root playable の speed を変更する

## ざっくりした使い分け
- キャラクターのモーション切り替え
  - `MotionPlayer` と `MotionHandle`
- Animation Job で root motion や姿勢を補正したい
  - `MotionPlayer.JobConnector`
- Timeline の再生だけを行いたい
  - `TimelinePlayer`

## 実装上の注意
- `MotionCrossFader` は再生時間の source of truth を持たない
- graph の進行責務は `Manual` と非 `Manual` で分かれる
- `SkipFrame` 中の時間は捨てずに pending へ積む
- speed や update mode を切り替える前には、必要なら pending を flush して境界を明確にする

PlayableSystem を読むときは、まず `MotionPlayer` を入口にして、次に `MotionLayerHandler`、`MotionCrossFader`、`AnimationJobConnector` の順で追うと把握しやすい。
