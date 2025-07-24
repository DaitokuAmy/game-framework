# CutsceneSystems

このフォルダは、ゲーム内のカットシーン（演出シーケンス・Timeline等）制御・管理のための各種クラス・コンポーネントを収録しています。

## 主要クラス・インターフェース概要

### CutsceneManager
- **役割**: カットシーン（ICutscene）の生成・再生・停止・プーリング管理を行う中心的なクラス。
- **主な機能**:
  - `Play(GameObject prefab, ...)`/`Play(Scene scene, ...)`：PrefabやSceneからカットシーンを再生。
  - `GetHandle(GameObject prefab, ...)`/`GetHandle(Scene scene, ...)`：カットシーンインスタンスのハンドル取得（手動制御用）。
  - `Clear()`：全カットシーンとプールをクリア。
- **Handle**: カットシーン再生のハンドル。`Play()`/`Stop()`/`SetTime()`/`Dispose()` などで個別制御可能。

### ICutscene
- **役割**: カットシーンの共通インターフェース。
- **主なメソッド**:
  - `Initialize(bool)`/`Dispose()`/`OnReturn()`/`Play()`/`Stop()`/`Update(float)`/`SetSpeed(float)`/`Seek(float)`

### Cutscene / RuntimeCutscene
- **役割**: カットシーンの実体制御（MonoBehaviour実装 or PlayableDirectorラップ）。
- **主な機能**:
  - PlayableDirectorを用いたTimeline再生・停止・速度変更・シーク等。
  - `Cutscene`はMonoBehaviour、`RuntimeCutscene`はPlayableDirectorラップ。

---

## 使い方例

### 1. CutsceneManagerの生成とカットシーン再生

```csharp
using GameFramework.CutsceneSystems;
using UnityEngine;

// CutsceneManagerの生成
var cutsceneManager = new CutsceneManager();

// カットシーンPrefabから再生
var handle = cutsceneManager.Play(myCutscenePrefab);

// 再生開始
handle.Play();

// 必要に応じて停止
handle.Stop();

// 再生位置をシーク
handle.SetTime(2.5f);

// 終了時にDispose
handle.Dispose();
```

### 2. カットシーンの手動制御

```csharp
// ハンドルを取得（再生は自分で呼ぶ）
var handle = cutsceneManager.GetHandle(myCutscenePrefab);

// 必要なセットアップ後に再生
handle.Play();
```

### 3. 全カットシーンのクリア

```csharp
cutsceneManager.Clear();
```

---

## 備考

- CutsceneManagerは内部でプーリングを行い、パフォーマンスを最適化します。
- ICutsceneを実装することで独自のカットシーン制御も可能です。
- PlayableDirectorを使ったTimeline演出の再生・停止・シーク・速度変更などが容易に行えます。 