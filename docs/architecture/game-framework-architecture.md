# GameFramework アーキテクチャ概要

## 対象範囲
本ドキュメントの対象は `Packages/com.daitokuamy.gameframework` に含まれる `GameFramework` パッケージ本体のみとする。

## 全体構造
GameFramework は、Unity 上で動作する機能群をそのまま 1 つのレイヤーに詰め込むのではなく、用途ごとに責務を分けながら構成している。

大きな構造は次の 4 層で整理される。

- `GameFramework`
  - Unity ランタイム上で利用する本体
- `GameFramework.NoEngine`
  - UnityEngine に依存しない基盤
- `GameFramework.Editor`
  - Editor 拡張
- `GameFramework.Tests.*`
  - テスト

この構造の中心は、Unity 依存のあるコードと Unity 非依存のコードを明確に分けることにある。

## アセンブリ分割の考え方

### GameFramework
`GameFramework` は Unity 上で利用するメインアセンブリである。`MonoBehaviour`、`ScriptableObject`、Scene、Prefab、Renderer、Animator など、Unity ランタイムと直接結びつく機能は基本的にここに属する。

### GameFramework.NoEngine
`GameFramework.NoEngine` は UnityEngine 参照を持たない基盤アセンブリである。状態管理、スコープ管理、時間管理、逐次処理の抽象など、Unity に依存しない概念をここに置く。

このアセンブリは、Unity 上の機能を支える土台であり、パッケージ全体の再利用性と見通しを支える役割を持つ。

### GameFramework.Editor
`GameFramework.Editor` は Inspector 拡張や Editor Tool など、開発支援のための層である。ランタイム本体とは分離され、実行時コードへ Editor 依存を持ち込まない。

### Tests
テストは Editor と Runtime に分けて配置される。Unity 非依存で検証できるものは EditMode 側に寄せ、ランタイム依存があるものだけを Runtime テスト側に置く構成になっている。

## 依存関係の基本方針
依存方向は次のように整理される。

- `GameFramework.NoEngine` は最下層の基盤とする
- `GameFramework` は `GameFramework.NoEngine` の上に乗る
- `GameFramework.Editor` はランタイム側を参照する外側の層とする
- テストはさらに外側から本体を検証する層とする

つまり、内側の基盤ほど Unity 依存を持たず、外側へ行くほど Unity 固有の責務を扱う構成である。

## 主要な設計方針

### 1. 起動責務を BootSystem に集約する
起動処理は `BootManager`、`MainSystemBase`、`MainSystemStarter` を中心に扱う。これにより、アプリケーション開始処理とリブート処理の入口を一箇所に寄せている。

### 2. Unity 非依存の概念を NoEngine に寄せる
状態、寿命、時間、逐次処理のような概念は、可能な限り `GameFramework.NoEngine` に分離する。

これにより、次の利点が得られる。

- Unity API への直接依存を減らせる
- テストしやすい単位を作りやすい
- 各システムの責務を整理しやすい
- Unity 上の見た目や入出力と、純粋な制御ロジックを分離しやすい

### 3. 各システム内でも Unity 非依存コードを切り出す
Unity 非依存のコードは `NoEngine` フォルダ配下だけに限定されない。各種システムフォルダ内でも、Unity に依存しないロジックが必要な場合は、そのシステム配下に `NoEngine` ディレクトリを持たせて分離する。

その場合は `AssemblyReference` を使って `GameFramework.NoEngine` に含める。

実例:
- `Scripts/Runtime/ActorSystem/NoEngine/AssemblyReference.asmref`

この構成により、機能ごとのまとまりを保ったまま、Unity 非依存コードだけを `GameFramework.NoEngine` 側へ参加させることができる。

### 4. 更新と寿命を明示的に扱う
GameFramework では、更新順や寿命を暗黙に散らさず、基盤側で明示的に扱う方針を取る。

代表例:
- `UpdateScheduler` による更新順管理
- `Logic` による activate / deactivate / dispose 管理
- `IScope` による寿命単位の管理
- `IProcess` による逐次処理の表現

この方針により、処理の開始、継続、終了の責務がコード上で追いやすくなる。

### 5. 機能システムは共通パターンの上に構築する
UI、VFX、Tween、Asset などの各システムは独立した責務を持つが、内部では共通の考え方を共有している。

代表的なパターン:
- マネージャを中心に責務を集約する
- ハンドル経由で実行中の処理を扱う
- 時間や更新を基盤層から受け取る
- Unity 依存部分と制御ロジックを分ける

### 6. Runtime 直下の機能は基本的に `*System` として整理する
`Runtime` 直下に置く主要な機能フォルダは、原則として `BootSystem`、`ActorSystem`、`UiSystem` のように `System` 接尾辞を持つ単位として定義する。

名前空間も基本的にはそのフォルダ名に合わせ、機能のまとまりとコード上の境界が対応する状態を維持する。

ただし、`Common` は例外とする。`Common` 配下のコードについては、共通基盤としての性質を優先し、名前空間を `GameFramework` とした構成を許容する。

この方針により、Runtime 配下の各要素がどの機能システムに属するかをディレクトリ構造と名前空間の両面から把握しやすくする。

### 7. 各種 System は相互参照をできるだけ避ける
各種 `*System` は、他の `*System` をなるべく直接参照しない設計を基本とする。System 間の依存が増えると責務境界が崩れやすくなり、変更影響の見通しも悪くなるためである。

そのため、各 System は基本的に `Common` にのみ依存する形を想定して構成する。複数の System で共有したい概念や部品がある場合は、特定の System に置いて横断参照するのではなく、`Common` 側へ寄せて扱うことを優先する。

この依存方針により、各 System は独立性を保ちやすくなり、機能追加や差し替え、保守時の影響範囲も限定しやすくなる。

## このパッケージにおける NoEngine の位置づけ
`GameFramework.NoEngine` は単なる補助アセンブリではなく、パッケージ全体の設計方針を支える基盤である。

GameFramework では、Unity 上で必要な機能を提供しつつも、設計の中心には次の考えがある。

- Unity 固有の表現と、ゲームロジック上の概念を分離する
- 純粋な C# オブジェクトとして扱える部分を増やす
- 状態遷移、時間、寿命、逐次処理を共通基盤として扱う

そのため、アーキテクチャを理解するうえでは、`GameFramework` 本体だけでなく `GameFramework.NoEngine` を基盤層として捉えることが重要になる。

## まとめ
GameFramework のアーキテクチャは、次の考え方で整理できる。

- Unity 上で動く本体を `GameFramework` に置く
- Unity 非依存の基盤を `GameFramework.NoEngine` に置く
- Editor 拡張とテストは外側の層として分離する
- Runtime 配下は基盤フォルダと機能システムフォルダに分ける
- Runtime 直下の主要フォルダは原則として `*System` で定義し、名前空間もそれに合わせる
- `Common` は例外として `GameFramework` 名前空間を許容する
- 各 `*System` は他 System への直接依存を避け、基本的には `Common` に依存する
- 各機能システム内でも、Unity 非依存のロジックは `NoEngine` と `AssemblyReference` を使って分離する
- 更新、寿命、状態、時間といった横断的な概念は基盤側で共通化する

この構造により、GameFramework は Unity 固有の実装を扱いながらも、設計の中心を Unity 非依存な基盤に置く形になっている。
