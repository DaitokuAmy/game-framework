# CameraSystems

このフォルダは、ゲーム内のカメラ制御・管理のための各種クラス・コンポーネントを収録しています。

## 主要クラス・インターフェース概要

### CameraManager
- **役割**: 複数の仮想カメラ・カメラグループの生成・切替・制御・ブレンド・ターゲット管理を行う中心的なクラス。
- **主な機能**:
  - `Activate(groupKey, cameraName, blend, priority)`：カメラのアクティブ化・ブレンド切替。
  - `RegisterCameraGroup(CameraGroup)`/`RegisterCameraGroupPrefab(GameObject)`：カメラグループの登録。
  - `GetCameraComponent<T>()`/`SetCameraController()`：カメラコンポーネントやコントローラの取得・設定。
  - `GetTargetPoint()`：ターゲットポイント（注視点等）の取得。
  - `Dispose()`/`Clear()`：全カメラの廃棄・初期化。

### ICameraComponent / DefaultCameraComponent / SerializedCameraComponent
- **役割**: 仮想カメラ（CinemachineVirtualCamera等）をラップし、アクティブ化・優先度・更新・ターゲット指定などを管理。
- **主なメソッド**:
  - `Initialize(CameraManager)`/`Activate()`/`Deactivate()`/`Update(float)`/`SetPriority(int)`/`ResetPriority()`

### ICameraController / CameraController
- **役割**: カメラの動的制御（追従・ズーム・演出等）を行うコントローラのインターフェース・基底クラス。
- **主なメソッド**:
  - `Initialize(ICameraComponent)`/`Activate()`/`Deactivate()`/`Update(float)`

### CameraGroup
- **役割**: 複数の仮想カメラ・ターゲットポイントをまとめて管理するグループ。
- **主な機能**:
  - `Key`：グループ識別キー。
  - `CameraRoot`/`TargetPointRoot`：カメラ・ターゲットのルート。
  - `GetCameraNames()`：グループ内カメラ名一覧取得。

### CameraTarget
- **役割**: 仮想カメラのFollow/LookAtターゲットを文字列指定でCameraManagerから自動取得・設定。

---

## 使い方例

### 1. CameraManagerの生成とカメラ切替

```csharp
using GameFramework.CameraSystems;
using UnityEngine;

// CameraManagerの生成（通常はシーンに配置）
var cameraManager = FindObjectOfType<CameraManager>();

// カメラグループPrefabを登録
cameraManager.RegisterCameraGroupPrefab(myCameraGroupPrefab, "Main");

// カメラをアクティブ化（ブレンド指定も可能）
cameraManager.Activate("Main", "MainCamera", new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 1.0f));

// カメラコンポーネント取得
var camComp = cameraManager.GetCameraComponent<DefaultCameraComponent>("MainCamera");

// カメラコントローラ設定
cameraManager.SetCameraController("MainCamera", myController);
```

### 2. ターゲットポイントの取得

```csharp
// ターゲットポイント（例: プレイヤーのTransform）を取得
var target = cameraManager.GetTargetPoint("PlayerTarget");
```

### 3. カメラグループ・カメラ名一覧取得

```csharp
// カメラグループ内のカメラ名一覧
var names = cameraManager.GetCameraNames("Main");
```

---

## 備考

- CameraManagerはCinemachineを活用し、複数カメラの切替・ブレンド・ターゲット管理を柔軟に行えます。
- CameraComponent/Controllerを拡張することで独自のカメラ演出や制御も可能です。
- CameraTargetを使うと、ターゲット指定が柔軟かつデータ駆動的に行えます。 