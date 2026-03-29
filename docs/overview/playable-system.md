# PlayableSystem 概要

## 対象
本ドキュメントは `Packages/com.daitokuamy.gameframework/Scripts/Runtime/PlayableSystem` に含まれる `PlayableSystem` の概要をまとめる。

## 役割
`PlayableSystem` は、`Animator` と `PlayableGraph` を使ったアニメーション再生を、ゲーム側から扱いやすくするための仕組みである。

このシステムは次のような用途をまとめて受け持つ。

- キャラクターの通常モーションを切り替える
- 上半身だけ別モーションを重ねる
- Animation Job で姿勢や root motion を補正する
- Timeline を手軽に再生する
- 更新頻度を落として負荷を調整する

## どんなときに使うか

### 通常のモーション再生をまとめたいとき
移動、待機、攻撃、被弾のように複数のモーションを切り替えたい場合、`MotionPlayer` と `MotionHandle` を使う。

実際には `Playable` を毎回手で組み立てるより、`AnimationClip` や `RuntimeAnimatorController` を拡張メソッド経由で渡して再生することが多い。単純なモーション切り替えを、そのままコードに書きやすいのが利点である。

### 一時的なアクションを重ねたいとき
「下半身は移動のまま、上半身だけ攻撃する」のようなケースでは、拡張レイヤーに別の `Playable` を流して重ねられる。

この使い方では、ベースモーションを止めずに一部の表現だけ差し込めるため、戦闘やリアクション演出と相性がよい。

### モーションの後段で補正を入れたいとき
足の高さ補正、root motion の補助、最終姿勢の微調整のように、「再生した結果にあとから手を入れたい」場合は `AnimationJobConnector` を使う。

これにより、モーションそのものを作り直さずに、ゲーム中の条件に応じた補正を追加しやすい。

### Timeline を簡単に再生したいとき
演出や一連のアニメーションを Timeline で再生したい場合は `TimelinePlayer` を使う。

`MotionPlayer` と責務を分けているため、「キャラクターの通常モーション制御」と「Timeline 再生」を用途ごとに選びやすい。

### 更新負荷を調整したいとき
遠距離のキャラクターや重要度の低い対象では、毎フレーム完全に評価しなくても見た目上問題ないことがある。

`PlayableSystem` はそのようなケースを想定しており、更新頻度を落とす設計を取り込みやすい。

## 主要な構成

### `MotionPlayer`
`Animator` ごとの再生の入口であり、`PlayableGraph` の生成、レイヤー構成、更新モード、再生速度の制御を担当する。

「1 体のキャラクターをどう再生するか」を考えるときは、まずこのクラスから見るとよい。

### `MotionHandle`
各レイヤーに対する操作窓口である。モーションの切り替え、クロスフェード、レイヤー weight の調整を呼び出し側から扱う。

### `MotionCrossFader`
`Playable` の接続と切り替えを担当する。役割は「何をつなぎ、どう混ぜ、いつ外すか」に寄っており、再生時間そのものは graph 側に任せる設計である。

### `AnimationJobConnector`
`AnimationScriptPlayable` を差し込み、`IAnimationJobComponent` を順序付きで管理する。

モーションの後処理や補正を足したいときの拡張点になる。

### `TimelinePlayer`
`PlayableDirector` を使った Timeline 再生用の player である。用途は近いが、`MotionPlayer` とは別系統として扱う。

## 利用イメージ

### 1. `AnimationClip` を切り替える
```csharp
var motionPlayer = new MotionPlayer(animator);
motionPlayer.Change(idleClip, 0.0f);
motionPlayer.Change(runClip, 0.15f);
```

待機から移動へ自然に遷移したい、という最も基本的な用途である。`Playable` を自前で作らなくても、`AnimationClip` をそのまま渡せる。

### 2. `RuntimeAnimatorController` を切り替える
```csharp
motionPlayer.Change(locomotionController, 0.0f, autoDispose: false);
motionPlayer.Change(battleController, 0.2f, autoDispose: false);
```

locomotion 用の controller と戦闘用の controller を切り替えるような使い方である。状態機械を含めたモーション遷移を扱いたいときに向いている。

### 3. 上半身だけ別モーションを重ねる
```csharp
var upperBodyHandle = motionPlayer.GetExtensionHandle(1);
upperBodyHandle.SetWeight(1.0f);
upperBodyHandle.Change(attackClip, blendDuration: 0.1f);
```

ベースの locomotion を残したまま、追加レイヤーに攻撃やリアクションを流すイメージで使う。ここでも `AnimationClip` や `RuntimeAnimatorController` を直接渡せる。

### 4. Animation Job で補正を差し込む
```csharp
motionPlayer.JobConnector.AddComponent(new AdjustHeightAnimationJobComponent(), order: 100);
```

キャラクターの最終姿勢に補正を入れたいときの入り口になる。

### 5. Timeline を再生する
```csharp
var timelinePlayer = new TimelinePlayer(director);
timelinePlayer.Play(timelineAsset);
```

会話演出やイベント再生のように、アニメーション全体を Timeline でまとめて扱いたい場合に向いている。

## よくある入口
`MotionPlayer` は内部的には `Playable` を扱うが、ゲームコード側では拡張メソッドを通して次のような素材を渡す使い方が中心になる。

- `AnimationClip`
  - 単発モーションや短いアクションを切り替えたいとき
- `RuntimeAnimatorController`
  - 状態機械ごと切り替えたいとき
- `TimelineAsset`
  - Timeline を `MotionHandle` 経由で差し込みたいとき

`Playable` を直接渡すのは、既に別の場所で `Playable` を生成済みのケースや、独自の構成を組みたいケースで使う上級寄りの入口と考えると分かりやすい。

## 更新モードの考え方
`PlayableSystem` は、用途に応じて更新の持ち方を選べる。

- 通常のキャラクター再生
  - `GameTime` や `UnscaledGameTime` を使う
- オーディオ基準で揃えたい
  - `DSPClock` を使う
- 更新の主導権をゲーム側で持ちたい
  - `Manual` を使う

特に、LOD 的に評価頻度を調整したい場合は `Manual` が使いやすい。`Manual` では「時間の進行」と「実際の評価タイミング」を分けやすいため、更新頻度の制御と相性がよい。

## 使い分けの目安

- キャラクターの通常モーションを切り替えたい
  - `MotionPlayer.Change(AnimationClip, ...)`
  - `MotionPlayer.Change(RuntimeAnimatorController, ...)`
- レイヤーを分けて一時的な動作を重ねたい
  - 拡張レイヤーの `MotionHandle`
- 再生後の姿勢補正を入れたい
  - `AnimationJobConnector`
- Timeline をそのまま再生したい
  - `TimelinePlayer`
- 更新頻度を落とす設計を組み込みたい
  - `MotionPlayer` の更新モードとスキップ制御

## まとめ
`PlayableSystem` は、単なる `PlayableGraph` の薄いラッパーではなく、「モーション再生」「レイヤー合成」「Job による補正」「Timeline 再生」をゲーム実装で使いやすい形にまとめた基盤である。

普段は `AnimationClip` や `RuntimeAnimatorController` を拡張メソッド経由で流し、必要なときだけ `MotionHandle` や `Playable` 直操作へ降りていく、という見方で捉えると使いやすい。
