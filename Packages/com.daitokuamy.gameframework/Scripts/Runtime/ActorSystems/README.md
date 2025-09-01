# ActorSystems

このフォルダは、ゲーム内のアクター（キャラクター・オブジェクト等）の構造・管理・拡張のための各種クラス・コンポーネントを収録しています。

## 主要クラス・インターフェース概要

### Actor
- **役割**: ゲーム内のアクター（キャラクター・オブジェクト等）の基底クラス。Body（見た目・物理）を制御し、各種ActorComponentを管理。
- **主な機能**:
  - `Body`/`Transform`/`Position`/`Rotation` などのプロパティ
  - ActorComponentの追加・取得・更新
  - TaskRunnerやGizmo描画等の拡張

### Body
- **役割**: GameObjectの見た目・物理・ロケーター・メッシュ・マテリアル等を一括管理するクラス。
- **主な機能**:
  - `GameObject`/`Transform`/`IsActive`/`IsVisible`/`LayeredTime` などのプロパティ
  - BodyComponentの追加・取得・更新
  - 各種物理・描画・アタッチメント・ギミック等の拡張

### ActorEntity
- **役割**: アクターの論理的なまとまり（Entity）を管理し、複数のIActorEntityComponentで拡張可能。
- **主な機能**:
  - `SetActive()`/`AddComponent<T>()`/`GetComponent<T>()` など
  - Body, Actor, Scene, Logic, Model等の管理用Componentを追加可能

### ActorEntityManager
- **役割**: 複数のActorEntityを一括生成・管理・破棄するマネージャ。
- **主な機能**:
  - `CreateEntity(id)`/`DestroyEntity(id)`/`FindEntity(id)` など

### BodyComponent / IBodyComponent
- **役割**: Bodyの機能拡張用の基底クラス・インターフェース。
- **主な機能**:
  - `Initialize(Body)`/`Update(float)`/`LateUpdate(float)`/`Dispose()` など

### ActorEntityComponent / IActorEntityComponent
- **役割**: ActorEntityの機能拡張用の基底クラス・インターフェース。
- **主な機能**:
  - `Attached(ActorEntity)`/`Activate()`/`Deactivate()`/`Dispose()` など

### ActorComponent
- **役割**: Actorの機能を拡張するための基底クラス。移動・アニメーション・AI・エフェクトなど、アクター固有の振る舞いをモジュール化して追加可能。
- **主な機能**:
  - `ExecutionOrder`：実行順序指定
  - `InitializeInternal(IScope)`/`DisposeInternal()`/`UpdateInternal(float)`/`LateUpdateInternal(float)` などのオーバーライドで独自処理を実装
  - ギズモ描画の拡張も可能
- **使い方**:
  - Actorに `AddComponent(new MyActorComponent())` で追加し、`GetComponent<MyActorComponent>()` で取得
- **拡張例**:
```csharp
public class MyActorComponent : ActorComponent {
    protected override void UpdateInternal(float deltaTime) {
        // 例: 移動やAI処理など
    }
}
// Actor生成時に追加
actor.AddComponent(new MyActorComponent());
```

---

## 使い方例

### 1. Actor/Body/Entityの実践的な生成例（Factoryパターン利用）

```csharp
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using SampleGame.Domain.Battle;
using SampleGame.Infrastructure;
using UnityEngine;

// 依存リポジトリやマネージャの取得
var bodyPrefabRepository = ServiceResolver.Resolve<BodyPrefabRepository>();
var characterAssetRepository = ServiceResolver.Resolve<BattleCharacterAssetRepository>();
var actorEntityManager = ServiceResolver.Resolve<ActorEntityManager>();

// プレイヤー用モデル（IReadOnlyPlayerModel）とLayeredTimeが与えられている前提
// CancellationToken ct も必要

// Body生成
var prefab = await bodyPrefabRepository.LoadCharacterPrefabAsync(model.Master.AssetKey, ct);
var body = new Body(Object.Instantiate(prefab, actorEntityManager.RootTransform));
body.LayeredTime.SetParent(layeredTime);
body.RegisterTask(TaskOrder.Body);

// Actor生成
var actorData = await characterAssetRepository.LoadActorDataAsync(model.Master.ActorAssetKey, ct);
var actor = new BattleCharacterActor(body, actorData);
actor.RegisterTask(TaskOrder.Actor);

// Adapter生成
var adapter = new CharacterActorAdapter(actor, model);
adapter.RegisterTask(TaskOrder.Logic);

// Controller生成
var controller = new PlayerInputController(model);
controller.RegisterTask(TaskOrder.Input);

// Entity構築
var entity = actorEntityManager.CreateEntity(model.Id);
entity.SetBody(body);
entity.AddActor(actor);
entity.AddLogic(adapter);
entity.AddLogic(controller);

// ---

### 2. Entityの破棄

```csharp
// プレイヤー削除時などにEntityを破棄
actorEntityManager.DestroyEntity(model.Id);
```

### 3. BodyComponent/ActorEntityComponentの拡張

```csharp
// BodyComponentを継承して独自機能を追加
public class MyBodyComponent : BodyComponent {
    protected override void UpdateInternal(float deltaTime) {
        // 独自の更新処理
    }
}

// ActorEntityComponentを継承してEntity拡張
public class MyEntityComponent : ActorEntityComponent {
    protected override void ActivateInternal(IScope scope) {
        // アクティブ時の処理
    }
}
```

---

## 備考

- Actor/Body/Entity/Componentの分離により、柔軟なキャラクター・オブジェクト設計が可能です。
- Entity-Componentパターンで、BodyやActor、Scene、Logic、Model等を自由に組み合わせ・拡張できます。
- BodyComponent/ActorEntityComponentを継承することで、独自の機能追加やゲーム固有の拡張も容易です。 