# UISystem Overview

`UISystem` は、UI をシーンまたはプレハブ単位で読み込み、`UIService` ごとに更新しながら、`UIScreen` と `UIDialog` の開閉や切り替えを扱うための仕組みです。  
このドキュメントでは、現在の実装での責務分担と、使うときの見取り図を整理します。

## 何を担当するか

- `UIManager`
  - UI シーン / プレハブ / 既存 `GameObject` を登録する
  - 読み込まれた `IUIService` を収集し、`Update` / `LateUpdate` を流す
  - アセットの寿命を `AssetHandle` で管理する
- `UIService`
  - ひとまとまりの UI ルートを表す `MonoBehaviour`
  - 子の `IUIView` を初期化して、独自コルーチンと更新順を管理する
  - `TimeScale` を使って配下 UI の更新速度をまとめて調整する
- `UIView`
  - `UIService` 配下で動く基本ビュー
  - `Initialize` / `Start` / `Dispose` と独自コルーチンの土台を持つ
  - 動的生成時は `InstantiateView` や `ManualInitialize` で service 配下へ編入する
- `UIScreen`
  - 開閉アニメーションとアクティブ状態を持つ画面単位
  - `OpenAsync` / `CloseAsync`、`PreOpen` / `PostOpen` などのフックを提供する
  - `IUIScreenHandler` を差し込んで画面固有ロジックを外付けできる
- `UIScreenContainer`
  - 複数の `UIScreen` をキー付きで保持し、1 画面ずつ遷移させる
  - `Transition` / `Clear` で子画面の切り替えを行う
- `UIDialogContainer`
  - ダイアログをスタック管理する
  - テンプレートから画面をプール生成し、閉じたら再利用する
  - `OpenDialog` と `BackDialog` で結果付きダイアログ操作を行う
- `UIAnimationPlayer`
  - `IUIAnimation` を時間付きで再生する
  - `AnimatableUIScreen` などから開閉アニメーション再生に使う
- `Component`
  - `RecyclableScrollList`、`ScrollSnapper`、`SafeAreaPanel`、`TouchAnimation` など
  - 画面部品として再利用する補助コンポーネント群

## 全体の構造

`UISystem` の中心は `UIManager` です。  
`UIManager` 自体は `MonoBehaviour` ではなく、外側の更新ループから `Update` / `LateUpdate` される前提です。  
その配下に、ロード済み UI ルートごとの `UIService`、さらにその子として `UIView` / `UIScreen` が並びます。

概念上の関係は次のとおりです。

```text
UIManager
  -> UIService
    -> UIView
    -> UIScreen
      -> UIScreenContainer
      -> UIDialogContainer
      -> UIDialog
```

`UIManager` はロード対象ごとに `AssetInfo` を持ち、どの `UIService` がそのアセットに属しているかを記録します。  
`AssetHandle.Dispose()` を呼ぶと、そのアセット由来の service をまとめて `Dispose` し、必要に応じてシーンのアンロードやプレハブの破棄まで行います。

## 読み込みと初期化

`UIManager` に UI を登録する入口は 3 つあります。

- `LoadSceneAsync(string assetKey)`
  - UI 配置済みシーンをロードして有効化する
- `LoadPrefabAsync(string assetKey)`
  - UI プレハブを `UIManager_Root` 配下にインスタンス化する
- `AddGameObject(GameObject gameObject)`
  - 既存オブジェクトをそのまま管理下に追加する

いずれの場合も、登録されたルート配下から `IUIService` を集めて `Initialize()` します。  
`UIService.Initialize()` では、さらに子の `IUIView` を走査して `Initialize(this)` を流します。

この初期化チェーンにより、各ビューは自分が属する `UIService` を知ったうえで動作します。  
動的に `UIView` を増やす場合も、単純に `Instantiate` するだけでは不十分で、`InstantiateView` または `ManualInitialize` によって service 配下へ編入する必要があります。

## 更新モデル

毎フレームの更新責務は次のように分かれています。

- `UIManager`
  - `LayeredTime` があればその `DeltaTime` を使う
  - 全 `IUIService` に `Update` / `LateUpdate` を流す
- `UIService`
  - `TimeScale` を反映して配下の時間を調整する
  - service 自身の `UpdateInternal` / `LateUpdateInternal` を呼ぶ
  - 登録済み `IUIView` を順番に更新する
- `UIView`
  - 独自コルーチンを更新する
  - `IsActive` なときだけ `UpdateInternal` / `LateUpdateInternal` を呼ぶ

つまり時間の入口は `UIManager`、ビュー群の更新の束ね役は `UIService`、個別表示の振る舞いは `UIView` 側にあります。

## `UIScreen` の役割

`UIScreen` は `UIView` に「開閉状態」と「アクティブ状態」を追加したクラスです。

- `OpenAsync(...)`
  - 画面を開く
  - `PreOpen` -> `OpenRoutine` -> `PostOpen` の順に処理する
- `CloseAsync(...)`
  - 画面を閉じる
  - `PreClose` -> `CloseRoutine` -> `PostClose` の順に処理する
- `ActivateInternal(IScope scope)`
  - 開き終わって操作可能になったタイミングの処理
- `DeactivateInternal()`
  - 閉じ始めるときの解除処理

`CurrentOpenStatus` は `Opening` / `Opened` / `Closing` / `Closed` を持ち、重複した open / close 要求を吸収します。  
`immediate` を指定すると、コルーチン部分をスキップして状態だけ即時反映できます。

また、`IUIScreenHandler` を登録すると、画面の open / close / activate / deactivate に追従する外付けロジックを持たせられます。  
画面の見た目を prefab 側、画面固有の制御を handler 側に分離したいときの拡張ポイントです。

## `UIScreenContainer` の役割

`UIScreenContainer` は、複数の子 `UIScreen` をキーで管理するコンテナです。  
自分自身も `UIScreen` なので、コンテナごと開閉できます。

主な役割は次のとおりです。

- `Transition(key, ...)`
  - 現在の子画面から別の子画面へ遷移する
- `Clear(...)`
  - 現在画面を閉じて空状態にする
- `Add(key, screen)`
  - 実行時に子画面を追加する
- `Remove(key)`
  - 子画面の `GameObject` ごと破棄する

遷移中は `TransitionInfo` で `Prev` / `Next` / `Direction` / `Effects` を保持し、`ITransition` 実装に処理を委ねます。  
デフォルトは `CrossTransition`、同一画面の再初期化には `OutInTransition` を使います。

## `UIDialogContainer` の役割

`UIDialogContainer` は、`UIScreenContainer` のような画面切り替えではなく、ダイアログの積み上げに特化したコンテナです。

挙動の要点は次のとおりです。

- ダイアログは `key` ごとのテンプレートから生成する
- 生成には `UIViewPool<UIScreen>` を使い、閉じたダイアログは破棄せず再利用する
- 新しいダイアログを開くと、直前のダイアログはいったん閉じる
- 結果が返ると、そのダイアログを閉じ、1 つ下のダイアログを再度開く
- `BackDialog()` は一番上のダイアログに `Cancel()` を送る

`OpenDialog<TScreen>()` は選択 index を返し、`OpenDialog<TScreen, TResult>()` は `IDialog<TResult>` 経由で型付き結果を返します。  
戻り値は `DialogHandle` なので、コルーチンの `yield return` と `await` の両方で扱えます。

## アニメーションと派生画面

画面の開閉をそのまま使うだけでなく、アニメーション付きの派生も用意されています。

- `AnimatableUIScreen`
  - `UIAnimationPlayer` を内包する
  - `OpenRoutine` / `CloseRoutine` で `UIAnimationComponent` を再生する
- `AnimatableUIDialog`
  - `AnimatableUIScreen` ベースのダイアログ版
- `FaderUIScreen`
  - 複数の `FaderUIView` をラベル付きで管理し、フェードイン / フェードアウトを切り替える

`UIAnimationPlayer` 自体は `IUIAnimation` の `Duration` と `SetTime` を基準に進みます。  
1 つのアニメーションに対して重複再生が来た場合は既存再生を止め、`Handle.Skip()` で即時完了にもできます。

## サンプルでの使い分け

`Assets/SampleGame` では、`UIService` を用途ごとに分けて使っています。

- `ResidentUIService`
  - `UIScreenContainer` を使ってローディング画面を切り替える
  - ブロック画面、通知、フェーダーなど常駐 UI を束ねる
- `DialogUIService`
  - `UIDialogContainer` を使って共通ダイアログを開く
- `UITestDialogUIService`
  - ダイアログごとの handler を設定して、外部ロジックを差し込む

この構成にすると、呼び出し側は `UIManager.GetService<TUIService>()` で service を取得し、そこから「画面を切り替える」「ダイアログを開く」といった用途別 API を呼ぶだけで済みます。

## ざっくりした使い分け

- まず UI 一式を読み込みたい
  - `UIManager`
- UI ルート単位で状態や依存をまとめたい
  - `UIService`
- 単純な表示部品を作りたい
  - `UIView`
- 開閉を持つ画面を作りたい
  - `UIScreen`
- 1 画面ずつ切り替える画面群を作りたい
  - `UIScreenContainer`
- モーダルを積み重ねて結果を受け取りたい
  - `UIDialogContainer`
- 画面ロジックを view から分離したい
  - `IUIScreenHandler`
- UI 部品を再利用したい
  - `UIViewPool<TView>`

## 実装上の注意

- `UIManager` は `MonoBehaviour` ではないので、外側の更新ループに登録しないと動かない
- `AssetHandle` を破棄すると、そのアセット由来の `UIService` もまとめて解放される
- 動的生成した `UIView` は service 配下へ初期化登録しないと更新されない
- `UIDialogContainer` はダイアログを積む仕組みで、複数ダイアログを同時表示し続ける用途には向かない
- `UIScreenContainer.Remove` は対象 `GameObject` を破棄する

コードを読むときは、まず `UIManager` を入口にして、次に `UIService`、`UIView`、`UIScreen`、最後に `UIScreenContainer` / `UIDialogContainer` の順で追うと把握しやすいです。
