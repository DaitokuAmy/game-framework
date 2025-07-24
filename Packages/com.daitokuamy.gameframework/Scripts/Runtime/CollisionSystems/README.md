# CollisionSystems

このフォルダは、ゲーム内のコリジョン（当たり判定）管理・判定・通知・可視化のための各種クラス・コンポーネントを収録しています。

## 主要クラス・インターフェース概要

### CollisionManager
- **役割**: コリジョン（ICollision/IRaycastCollision）の登録・更新・判定・通知・自動廃棄を一括管理する中心的なクラス。
- **主な機能**:
  - `Register()`：コリジョンやリスナーの登録（複数オーバーロードあり）
  - `Unregister()`/`Dispose()`：登録解除・全廃棄
  - Update/LateUpdateで自動判定・通知

### ICollision / Collision
- **役割**: コリジョン（当たり判定）の共通インターフェース・基底クラス。
- **主な実装例**:
  - `SphereCollision`/`BoxCollision` など
- **主な機能**:
  - `Tick()`/`ClearHistory()`/`Center` など

### IRaycastCollision / RaycastCollision
- **役割**: レイキャストベースのコリジョン判定用インターフェース・基底クラス。
- **主な実装例**:
  - `SphereRaycastCollision`/`LineRaycastCollision` など
- **主な機能**:
  - `Tick()`/`March()`/`ClearHistory()`/`Start`/`End` など

### ICollisionListener / IRaycastCollisionListener
- **役割**: コリジョン判定結果の通知を受け取るリスナーインターフェース。
- **主な実装例**:
  - `CollisionListener`/`RaycastCollisionListener`（イベントでコールバック可能）

### CollisionHandle
- **役割**: コリジョン登録の有効期間管理用ハンドル。Disposeで自動解除。

### CollisionVisualizer
- **役割**: 登録されたコリジョンのギズモ描画（エディタ用デバッグ支援）。

---

## 使い方例

### 1. コリジョンの登録と判定通知

```csharp
using GameFramework.CollisionSystems;

// CollisionManagerの生成
var collisionManager = new CollisionManager();

// SphereCollisionを作成
var collision = new SphereCollision(center: new Vector3(0, 1, 0), radius: 1.0f);

// コリジョンリスナーを作成
var listener = new CollisionListener();
listener.HitCollisionEvent += result => {
    Debug.Log($"Hit! {result.collider}");
};

// コリジョンを登録
var handle = collisionManager.Register(listener, collision, layerMask: ~0, customData: null);

// 不要になったら解除
handle.Dispose();
```

### 2. レイキャストコリジョンの登録

```csharp
var raycastCollision = new SphereRaycastCollision(start, end, radius: 0.5f);
var rayListener = new RaycastCollisionListener();
rayListener.HitRaycastCollisionEvent += result => {
    Debug.Log($"Raycast hit! {result.RaycastHit.collider}");
};
var rayHandle = collisionManager.Register(rayListener, raycastCollision, layerMask: ~0, customData: null);
```

### 3. コリジョンの可視化（エディタ）

- 登録したコリジョンは `CollisionVisualizer` によりギズモ描画され、エディタ上で当たり判定範囲を確認できます。

---

## 備考

- コリジョン/リスナー/ハンドル/可視化の分離により、柔軟な当たり判定・通知・デバッグが可能です。
- Sphere/Box/Line/Raycast等の多様なコリジョン形状・判定方式に対応。
- コリジョンの履歴管理や自動廃棄、カスタムデータ付与もサポートしています。 