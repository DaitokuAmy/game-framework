# GimmickSystems

このフォルダは、ゲーム内のギミック（仕掛け・演出・状態変化など）制御・管理のための各種クラス・コンポーネントを収録しています。

## 主要クラス・インターフェース概要

### GimmickPlayer
- **役割**: 複数のギミック（IGimmick）をまとめて管理・再生・更新するクラス。
- **主な機能**:
  - `Setup(IEnumerable<GimmickGroup>)`：ギミックグループからギミックを収集・初期化。
  - `GetKeys()`/`GetKeys<T>()`：登録されているギミックのキー一覧取得。
  - `GetGimmicks<T>(string key)`：キー・型指定でギミック取得。
  - `Update(float)`/`LateUpdate(float)`：ギミックの更新・後更新。
  - `SetSpeed(float)`：ギミックの速度一括設定。

### GimmickGroup
- **役割**: 複数のギミック（Gimmick）をまとめて管理するMonoBehaviour。
- **主な機能**:
  - `GimmickInfos`：登録ギミック情報リスト。
  - `GetGimmicks<T>(string key)`：キー・型指定でギミック取得。

### IGimmick / Gimmick
- **役割**: ギミックの共通インターフェース・基底クラス。
- **主なメソッド**:
  - `Initialize()`/`Dispose()`/`SetSpeed(float)`/`UpdateGimmick(float)`/`LateUpdateGimmick(float)`

### 代表的なギミック基底クラス
- **ActiveGimmick**: アクティブ状態の制御用
- **AnimationGimmick**: アニメーション再生用
- **StateGimmick/StateGimmickBase**: ステート（状態）切り替え用
- **ChangeGimmick<T>**: 値の変更用
- **InvokeGimmick**: 任意の処理実行用

### GimmickExtensions
- **役割**: ギミック配列に対する一括操作の拡張メソッド群。
- **主なメソッド例**:
  - `Activate()`/`Deactivate()`/`Play()`/`Resume()`/`Invoke()`/`Change()` など

---

## 使い方例

### 1. GimmickPlayerのセットアップとギミック操作

```csharp
using GameFramework.GimmickSystems;

// GimmickGroupのリストを用意
var gimmickGroups = new List<GimmickGroup> { group1, group2 };

// GimmickPlayerのセットアップ
var player = new GimmickPlayer();
player.Setup(gimmickGroups);

// ギミックの更新
player.Update(Time.deltaTime);
player.LateUpdate(Time.deltaTime);

// 速度変更
player.SetSpeed(0.5f);

// キー・型指定でギミック取得
var animGimmicks = player.GetGimmicks<AnimationGimmick>("openDoor");
animGimmicks.Play(); // 拡張メソッドで一括再生
```

### 2. GimmickGroupからギミック取得

```csharp
// キー・型指定でギミック取得
var stateGimmicks = group.GetGimmicks<StateGimmick>("doorState");
stateGimmicks.Change("Open");
```

### 3. GimmickExtensionsによる一括操作

```csharp
// ActiveGimmick配列を一括でアクティブ化
activeGimmicks.Activate();
// AnimationGimmick配列を一括で再生
animationGimmicks.Play(reverse: false, immediate: true);
// StateGimmick配列を一括で状態変更
stateGimmicks.Change("Close");
```

---

## 代表的なギミックの用途例

- **GameObjectActiveGimmick**: GameObjectのアクティブ状態を制御
- **ClipAnimationGimmick**: AnimationClipによるアニメーション再生
- **GameObjectStateGimmick**: ステートごとにGameObjectのアクティブ切り替え
- **MaterialStateGimmick**: ステートごとにMaterialの値を変更
- **ParticleSystemStateGimmick**: ステートごとにParticleSystemを再生
- **PlayableDirectorAnimationGimmick**: Timeline/PlayableDirectorによるアニメーション制御

---

## 備考

- Gimmickは拡張性が高く、独自のギミックを追加可能です。
- GimmickPlayerやGimmickGroupを使うことで、複数ギミックの一括管理・制御が容易になります。
- GimmickExtensionsで配列操作も簡単に行えます。 