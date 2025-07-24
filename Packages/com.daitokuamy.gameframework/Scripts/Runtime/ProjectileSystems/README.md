# ProjectileSystems

このフォルダは、ゲーム内の弾・ビームなどの飛翔体（Projectile）制御・管理のための各種クラス・コンポーネントを収録しています。

## 主要クラス・インターフェース概要

### ProjectileManager
- **役割**: 弾・ビームなどの飛翔体の生成・再生・停止・プーリング管理を行う中心的なクラス。
- **主な機能**:
  - `Play(GameObject prefab, IBulletProjectileController, Vector3 scale, ...)`：弾の再生。
  - `Play(GameObject prefab, IBeamProjectileController, Vector3 scale, ...)`：ビームの再生。
  - `StopAll()`：全飛翔体の停止。
  - `Clear()`：全飛翔体とプールをクリア。
- **プーリング**: 弾・ビームごとにプールを持ち、パフォーマンスを最適化。

### ProjectilePlayer
- **役割**: 飛翔体の再生・進行・停止・当たり判定などの実行管理。
- **Handle**: 個々の飛翔体再生を制御するハンドル。

### BulletProjectile / BeamProjectile
- **役割**: 弾（Bullet）やビーム（Beam）の実体制御用MonoBehaviour。
- **主な機能**:
  - `Start(controller)`：飛翔開始。
  - `Update(deltaTime)`：進行・挙動更新。
  - `Exit()`：終了処理。
  - `SetSpeed(float)`：再生速度変更。
  - `SetActive(bool)`：アクティブ切り替え。
  - `SetLocalScale(Vector3)`：スケール設定。
  - `OnHitCollision(RaycastHit)`：衝突時の通知。

### IBulletProjectile / IBeamProjectile
- **役割**: 弾・ビームの共通インターフェース。独自のProjectile実装も可能。

### IBulletProjectileComponent / IBeamProjectileComponent
- **役割**: 弾・ビームの挙動拡張用インターフェース。MonoBehaviourで拡張可能。

### 代表的なコンポーネント実装
- **ActivateBulletProjectileComponent**: GameObjectのアクティブ制御
- **ParticleBulletProjectileComponent**: ParticleSystemによるエフェクト制御
- **BulletProjectileComponent/BeamProjectileComponent**: 拡張用の基底クラス

---

## 使い方例

### 1. ProjectileManagerの生成と弾の再生

```csharp
using GameFramework.ProjectileSystems;
using UnityEngine;

// ProjectileManagerの生成
var projectileManager = new ProjectileManager();

// 弾のコントローラー（例: ShotBulletProjectileController）を作成
var controller = new ShotBulletProjectileController(/* ...設定... */);

// 弾を再生
var handle = projectileManager.Play(myBulletPrefab, controller, Vector3.one);

// 必要に応じて停止
handle.Stop();
```

### 2. ビームの再生

```csharp
// ビームのコントローラー（例: ParticleBeamProjectileController）を作成
var controller = new ParticleBeamProjectileController(/* ...設定... */);

// ビームを再生
var handle = projectileManager.Play(myBeamPrefab, controller, Vector3.one);

// 必要に応じて停止
handle.Stop();
```

### 3. 全飛翔体の停止・クリア

```csharp
projectileManager.StopAll(); // 全ての弾・ビームを停止
projectileManager.Clear();   // プールも含めて全てクリア
```

---

## 各ProjectileComponentの利用例

### ActivateBulletProjectileComponent
- 飛翔中・ヒット時・終了時にGameObjectのアクティブ状態を制御。

### ParticleBulletProjectileComponent
- 飛翔中・ヒット時・終了時にParticleSystemを再生。

### BulletProjectileComponent/BeamProjectileComponent
- 独自の挙動拡張用に継承して利用。

---

## 備考

- ProjectileManagerは内部でプーリングを行い、パフォーマンスを最適化します。
- 弾・ビームの挙動はController（例: ShotBulletProjectileController, ParticleBeamProjectileController）で制御します。
- 各ProjectileComponentは拡張用インターフェースを実装しており、独自の挙動追加も可能です。 