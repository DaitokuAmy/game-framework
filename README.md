# game-framework
## 概要
Unityでゲーム制作する際のアーキテクチャ設計に関わるフレームワークです

基本的に、以下の要件を重要視した設計になっています
- 実行順の制御を容易にしたい
- 非同期処理を記述する箇所を明確にしたい
- 非同期処理や初期化処理の解放タイミングなどを明確かつシンプルにしたい
- レイヤーを作りすぎず、実装に対してのクラス量を抑えたい
- 仕様の変化になるべく強くしたい

## セットアップ
#### インストール
1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下を入力してインストール
   * https://github.com/DaitokuAmy/game-framework.git?path=/Packages/com.daitokuamy.gameframework
   ![image](https://user-images.githubusercontent.com/6957962/209446846-c9b35922-d8cb-4ba3-961b-52a81515c808.png)

あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記します。

```json
{
    "dependencies": {
        "com.daitokuamy.gameframework": "https://github.com/DaitokuAmy/game-framework.git?path=/Packages/com.daitokuamy.gameframework"
    }
}
```
バージョンを指定したい場合には以下のように記述します。

https://github.com/DaitokuAmy/game-framework.git?path=/Packages/com.daitokuamy.gameframework#1.0.0

## ライフサイクルについて
game-frameworkでは、「Unityにおけるシーン管理」だけでは不足しがちな、ライフサイクルの階層的管理をサポートしています

具体的には
- MainSystem
- Situation
- State

が該当します

### MainSystem
アプリ起動中のライフサイクルを管理するクラスで、Singleton的な動きをします

アプリケーションの以下のサイクルを管理できます
- アプリケーション起動時の初期化処理
- アプリケーションソフトリブート時のリセット処理
- アプリケーション起動中の更新処理
- アプリケーション終了時の処理

具体例で言うと、以下のような記述を書きます
- 初期シーンの決定（タイトル画面など）
- 常駐システムの生成/初期化やリセット処理

### Situation
Unityのシーンでは管理しづらい階層的なシーン管理（ここではシチュエーション管理）を行います

例えば以下のような階層設計が可能です
- TitleSceneSituation // title.unity
  - TitleSetupSituation // タイトルロゴ
  - TitleMainSituation // タイトルメニュー
- HomeSceneSituation // home.unity
  - HomeMainSituation // ホーム画面メイン
  - EquipmentSituation // 装備画面
- GameSceneSituation // game.unity
  - GameStartSituation // ゲーム開始演出
  - GameMainSituation // ゲームメイン
  - GameResultSituation // ゲーム結果

### State
Situationでは大きすぎるような簡易的な状態毎の定義を記述するために使用します

例えば以下のような状態で使用します
- GameMainSituation // ゲーム中
  - State.Playing // プレイ中
  - State.Pause // 一時停止中
  - State.CutIn // カットイン再生中

### まとめ
- MainSystem
  - 必ず一つ
  - 常駐システムの初期化や更新を行う場所
- Situation
  - Unityシーンの切り替わり + その内部での大きな切り替わり
  - ロード、初期化、更新、解放などの非同期的な物を含むライフサイクルを管理する
- State
  - 必要に応じて使う（使わなくても可能）
  - 実質、解放処理付きswitchのような扱い
  - 同期的な初期化とState切り替わり時の終了処理の記述に特化している
  
```mermaid
flowchart LR
subgraph MainSystem
  subgraph TitleSceneSituation
    TitleSetupSituation --> TitleMainSituation
  end

  subgraph HomeSceneSituation
    HomeMainSituation <--> EquipmentSituation
  end
  
  subgraph GameSceneSituation
    subgraph GameMainSituation
      PlayingState <--> PauseState
      PlayingState <--> CutInState
    end
  
    GameStartSituation --> GameMainSituation --> GameResultSituation
  end
  
  TitleSceneSituation --> HomeSceneSituation <--> GameSceneSituation
end
```

## コアになっている機能について
### TaskRunner
UnityのUpdateに依存させずに更新順番の管理を行う場合に使用します

- UnityのUpdateを使わない理由
  - 各MonoBehaviour間の更新順の管理が曖昧になりやすい
  - 更新を呼びたいだけなのに、無駄にGameObjectやMonoBehaviourを作る必要がある
- Taskを使うポイント
  - 基本的に実行順番が重要になる物はこの更新サイクルに乗せるのが理想
    - UI, Camera, Transform, Effect など
- なぜ更新順が重要なのか
  - Action性の高いゲームの場合、Input > SetMotion > UpdateBone > UpdateCamera > ConstraintEffect/ConstarintUI のような1frameにおける処理順が重要になる
    - 上記がうまく出来てないと、3Dキャラに追従させたHPゲージなどがずれてしまったり、エフェクトや当たり判定の位置がずれてしまうなどが起きる

### CoroutineRunner
UnityのCoroutineやC#のasync等とは違い、更新タイミングにずれを無くしたい非同期処理に対して使うためのCoroutine実行用の機能です
シンプルに言えば、Updateを自分でコール出来るCoroutineの実行機構です

- 専用で作る理由
  - 実行タイミングが読みづらい（次のフレームになってしまう、他の連携すべき処理順とずれたタイミングで実行されてしまうなど）
    - これに起因して、1フレーム待つコードなどが増えてしまう事を防止する
  - 本来の非同期処理とは違う意図のコード（WaitForEndOfFrameなど)が使われてしまう

### IScope
C#のCancellationTokenやUniRx等のCompositeDisposableなどの非同期処理のキャンセルや購読解除のタイミングを共通化するためのInterfaceです

- 専用で作る理由
  - ライフサイクル用のシステム（Situation, Stateなど)で共通的に初期化と対となる解放処理を定義しやすくする目的
  - 既存の物だと、拡張メソッドでは回避しづらくなるケースもあるが、IScopeは拡張メソッドでUniRxでのTakeUntil, AddToを拡張したり、CancellationTokenへの変換なども対応しやすいため

### ServiceContainer
各ライフサイクル中で共通化したいInstance（わかりやすく言えばSingletonのような物）を管理するためのコンテナです

これ自体はDIコンテナとほぼ同様の役割をしており、DIを使わない想定の設計がgame-frameworkにはのっています

- なぜDIではないのか
  - コードの制約を作る上では、DIの優秀な点は多く挙げられるが、Gameのような複雑な依存度が多いケースでは、引数のリレーが発生したり、どこかでそれを壊すようなSingletonが発生してしまったりする事が多い
  - 上記のような背景から、「ライフサイクル管理とセットのInstance共有システム」と言う定義で作成している
- Singletonを使わない理由
  - 提供クラス側に専用の記述が必要(staticインスタンス保持など)が必要になってしまい、継承関係に混ざってきたりしてコード自体が汚れてしまうため

### LayeredTime
UnityのTime.deltaTimeをラップした物で、これらをネスト管理する事が可能な仕組みです

- なぜネスト管理が必要なのか
  - Gameではスローの表現を多層的に行う事が多く、Unity標準のUnscaled機能などでは対応しきれない事が多いため
    - UIのTimeScale > 3D空間のTimeScale > 3DキャラのTimeScale > 3Dキャラの発射したObjectのTimeScale といったようなイメージ

### SituationFlow
Situation同士の遷移関係を定義するための仕組みです  

具体例でいうと、以下のように遷移関係を指定できるようになっています  
```csharp
// タイトル画面 > ゲーム画面 > メニュー画面
// タイトル画面 > オプション画面
// の遷移関係を定義
var titleNode = _situationFlow.ConnectRoot(_titleSituation);
var gameNode = titleNode.Connect(_gameSituation);
var menuNode = gameNode.Connect(_menuSituation);
var optionNode = titleNode.Connect(_optionSituation);
```

指定した後は、クラスの型を指定して遷移する事で、今の遷移状態の接続先に指定されているSituationに遷移します（接続していない場合はエラー）
```csharp
// タイトル画面に遷移
_situationFlow.Transition<TitleSituation>(situation => /* 遷移時のSituationインスタンスの初期化 */);

// 接続関係の前に戻る
_situationFlow.Back();
```

特徴として、この設計は「Situationをスタック管理していない」点があります  
事前にツリー構造を定義しているため、現在位置(Node)の戻り先が特定可能なので、スタック管理が不要になっています  

また、以下のようなツリーでは表現しづらいショートカットのような遷移指定にも対応しています  
```csharp
var gameNode = titleNode.Connect(_gameSituation); // この時点ではタイトル画面の次にゲーム画面を指した状態
_situation.SetFallbackNode(gameNode); // こうする事で、タイトル画面以外から GameSituation を指定された場合、ここにダイレクトジャンプする指定が可能
```
※どのSituationからでもGameSituationに遷移する事が可能で、Fallback指定経由で遷移した場合はGameSituationの戻り先がTitleSituationになる  

SituationのSetParentと併用する事で、ライフサイクルの管理（リソースやクラスの初期化/解放）と遷移の管理を別定義できるため、主に複雑な遷移のアプリケーションの際に利用します
```csharp
//---- リソーススコープを指定(以下の指定をする事で、常駐リソースの管理やアウトゲーム中のみに必要なクラスの初期化などを階層的に管理が可能になっています)
// |Resident                                                          |
// |Introduction          |OutGame                             |InGame|
// |TitleTop|Option|Credit|HomeTop|PartyTop|UnitList|UnitDetail|
var residentSituation = new ResidentSituation();
regidentSituation.SetParent(_situationRunner);
var introductionSituation = new IntroductionSceneSituation();
introductionSceneSituation.SetParent(regidentSituation);
var titleTopSituation = new TitleTopSituation();
titleTopSituation.SetParent(introductionSceneSituation);
var optionSituation = new OptionSituation();
optionSituation.SetParent(introductionSceneSituation);
var creditSituation = new CreditSituation();
creditSituation.SetParent(introductionSceneSituation);
var outGameSceneSituation = new OutGameSceneSituation();
outGameSceneSituation.SetParent(residentSituation);
var homeTopSituation = new HomeTopSituation();
homeTopSituation.SetParent(outGameSceneSituation);
var partyTopSituation = new PartyTopSituation();
partyTopSituation.SetParent(outGameSceneSituation);
var unitListSituation = new UnitListSituation();
unitListSituation.SetParent(outGameSceneSituation);
var unitDetailSituation = new UnitDetailSituation();
unitDetailSituation.SetParent(outGameSceneSituation);
var inGameSceneSituation = new InGameSceneSituation();
inGameSceneSituation.SetParent(residentSituation);

//---- 遷移関係を指定(以下のように遷移関係を指定する事で、遷移に合わせて上記で指定したライフサイクルスコープが自動的に初期化、解放されるようになります)
// タイトル > オプション
// タイトル > クレジット
// タイトル > ホームトップ
var titleTopNode = _situationFlow.ConnectRoot(titleTopSituation);
titleTopNode.Connect(optionSituation);
titleTopNode.Connect(creditSituation);
var homeTopNode = titleTopNode.Connect(homeSituation);

// ホームトップ > パーティトップ
// パーティトップ > ユニットリスト
// パーティトップ > ユニット詳細
// ユニットリスト > ユニット詳細]
var partyTopNode = homeTopNode.Connect(partyTopSituation);
var unitListNode = partyTopNode.Connect(unitListSituation);
partyTopNode.Connect(unitDetailSituation);
unitListNode.Connect(unitDetailSituation);

// パーティトップのFallback指定(フッター遷移のように、アウトゲームの各種画面からパーティトップへの遷移を許す)
_situationFlow.SetFallback(partyTopNode, homeTopNode);

// ホームトップ > インゲーム
var inGameNode = homeTopSituation.Connect(inGameSceneSituation);
```

使用方法は[サンプルコード](https://github.com/DaitokuAmy/game-framework/blob/main/Assets/SituationFlowSample/SituationFlowSample.cs)を参考にしてください  


