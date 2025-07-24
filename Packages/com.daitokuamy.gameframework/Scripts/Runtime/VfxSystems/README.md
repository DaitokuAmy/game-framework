# VfxSystems

このフォルダは、ゲーム内のVFX（エフェクト）制御・管理のための各種クラス・コンポーネントを収録しています。

## 主要クラス・インターフェース概要

### VfxManager
- **役割**: VFX（エフェクト）の生成・再生・停止・プーリング管理を行う中心的なクラス。
- **主な機能**:
  - `Play(VfxContext, ...)`：VFXを再生し、完了時に自動で破棄。
  - `Get(VfxContext, ...)`：VFXインスタンスを取得し、手動で制御・破棄。
  - `Clear()`：全VFXとプールをクリア。
- **Handle**: VFX再生のハンドル。`Play()`/`Stop()`/`Dispose()` などで個別制御可能。

### VfxContext
- **役割**: VFX再生時のパラメータ（Prefab、位置、回転、スケール等）をまとめた構造体。

### IVfxComponent
- **役割**: VFX制御用の共通インターフェース。各種VFXコンポーネント（ParticleSystem, Animator, Volume等）が実装。
- **主なメソッド**:
  - `Play()`/`Stop()`/`StopImmediate()`/`Update(float)`/`SetSpeed(float)`/`SetLodLevel(int)`

### 代表的なVfxComponent実装
- **ActiveVfxComponent**: GameObjectのアクティブ制御
- **ParticleSystemVfxComponent**: ParticleSystem制御
- **PlayableDirectorVfxComponent**: PlayableDirector制御
- **AnimationClipVfxComponent**: AnimationClip制御
- **VolumeVfxComponent**: Volume（ポストプロセス等）制御
- **LodVfxComponent**: LODレベルによる表示制御
- **CinemachineImpulseVfxComponent**: CinemachineImpulse制御（Cinemachine利用時）

---

## 使い方例

### 1. VfxManagerの生成とVFX再生

```csharp
using GameFramework.VfxSystems;
using UnityEngine;

// VfxManagerの生成
var vfxManager = new VfxManager();

// VfxContextの作成
var context = new VfxContext {
    prefab = myVfxPrefab, // 再生したいVFXのPrefab
    relativePosition = new Vector3(0, 1, 0),
    constraintPosition = false,
    relativeAngles = Vector3.zero,
    constraintRotation = false,
    localScale = Vector3.one
};

// VFXを再生（自動破棄）
var handle = vfxManager.Play(context);

// 必要に応じて手動停止
handle.Stop();
```

### 2. VFXの手動制御

```csharp
// VFXインスタンスを取得（再生は自分で呼ぶ）
var handle = vfxManager.Get(context);

// 再生開始
handle.Play();

// 位置・回転・スケールの動的変更
handle.ContextPosition = new Vector3(1, 2, 3);
handle.ContextRotation = Quaternion.Euler(0, 90, 0);
handle.ContextLocalScale = new Vector3(2, 2, 2);

// 停止・破棄
handle.Stop(immediate: true, autoDispose: true);
handle.Dispose();
```

### 3. VFX全消去

```csharp
vfxManager.Clear();
```

---

## 各VfxComponentの利用例

### ActiveVfxComponent
- 指定したGameObjectのアクティブ状態を時間・ループで制御。

### ParticleSystemVfxComponent
- ParticleSystemの再生・停止・即時停止を制御。

### PlayableDirectorVfxComponent / AnimationClipVfxComponent
- TimelineやAnimationClipの再生・停止・進行を制御。

### VolumeVfxComponent
- Volumeの重み（weight）をカーブや時間で制御。

### LodVfxComponent
- LODレベルに応じてGameObjectの表示を切り替え。

---

## 備考

- VfxManagerは内部でプーリングを行い、パフォーマンスを最適化します。
- VfxContextの`constraintPosition`や`constraintRotation`を有効にすると、基準Transformに追従します。
- 各VfxComponentは`IVfxComponent`インターフェースを実装しており、独自のVFX制御も追加可能です。 