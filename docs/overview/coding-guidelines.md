# Coding Guidelines

## Source of Truth
- コードスタイルはリポジトリ直下の `.editorconfig` を最優先とする。
- 本ファイルと `.editorconfig` が矛盾する場合は `.editorconfig` を優先する。

## 目的
- 本ファイルは実装判断のガイドラインを定義する。
- 具体例は `docs/overview/coding-examples.md` を参照する。

## 適用範囲
- インデント、改行、空白、`using` 順、括弧配置などの機械的スタイルは `.editorconfig` に従う。
- 既存ファイルの局所スタイルに合わせ、不要な整形差分を作らない。

## 命名
### 基本
- 型名、メソッド名、プロパティ名、イベント名は PascalCase を使用する。
- ローカル変数、引数、private インスタンスフィールドは camelCase を使用する。
- private インスタンスフィールドは `_fieldName` 形式を使用する。
- interface は `I` 接頭辞付きの PascalCase を使用する。

### フィールド
- `const` は PascalCase を使用する。
- `static readonly` は PascalCase を使用する。
- `private static` フィールドは `s_fieldName` 形式を使用する。

## コメント
- コメント方針（`summary` / `param` / `inheritdoc`）は `docs/overview/coding-examples.md` に準拠する。
- XML コメントは言い切りで記述し、`です` / `ます` を使用しない。
- XML コメントの末尾に `。` を付けない。
- `summary` は、`class` / `struct` / `interface` / `enum` / `delegate` などの型定義、コンストラクタ、プロパティ、`event`、メソッド、定数、公開フィールドに原則として付与する。
- interface 実装などで重複説明になる場合は `inheritdoc` を優先する。
- メンバーフィールドに行コメントを書く場合は、フィールドの直前に `// コメント` の形式で記述する。
- `private` メンバーフィールドは、基本的にコメントを付けない。

### 省略語
- 2文字の略語は全大文字を使用する（例: `BG`, `SE`, `IP`）。
- 3文字以上の略語は通常の PascalCase / camelCase として扱う（例: `Bgm`, `Html`）。
- `Id` / `Ok` のように単語の一部を切り出した語は、略語ルールの対象外とする。

## フィールド公開範囲
- `public` / `internal` / `protected` フィールドは原則禁止とする。
- 外部から参照・更新が必要な場合は、フィールドを直接公開せずプロパティ経由で公開する。
- Unity のシリアライズ対象は `public` フィールドではなく `[SerializeField] private` を基本とする。

## クラス構成
- クラス内の定義順、アクセス修飾子の順序は `docs/overview/coding-examples.md` に準拠する。
- private インスタンスフィールドは連続で記述し、フィールド同士の間に空行を入れない。
- 同じブロックに属する宣言同士の間には空行を入れない。
- ここでいうブロックとは、同じ配置ルールが適用される宣言群を指す。
- 例: `const` と `static readonly`、`public` フィールドと `protected` フィールド、プロパティと `event`、`private readonly` フィールドと `private` フィールド、`[SerializeField] private` フィールドと通常の `private` フィールドは別ブロックとして扱う。

### 定義順の補足（簡易ルール）
- 迷った場合は次の順で配置する: `const` / `static readonly` / ネスト型 / フィールド / プロパティ・event / コンストラクタ / メソッド。
- メソッドは公開範囲の広いものを先に置く（`public` → `protected` → `internal` → `private`）。
- `static` メソッドは用途でまとまりを優先してよい。クラス方針として static セクションを先に置く場合、`private static` が `public` より前でもよい。
- `private static` メソッドは不要に増やさない。基本的に、`private` メソッドは static 経由でアクセスする必要がある場合を除き、インスタンスメソッドとして定義する。
- Unity のライフサイクルメソッド（例: `Awake`, `OnEnable`, `OnGUI`, `OnDisable`, `OnDestroy`）は、同一クラス内で順序を固定して運用する。
- `MenuItem` 付きメソッドは、アクセス修飾子に応じたメソッド群に配置する（属性の有無で別セクション化しない）。
- Unity クラスで迷った場合は次の順を基準とする: `const` / `static readonly` / ネスト型 / `private static` / `[SerializeField] private` / その他 private フィールド / プロパティ・event / コンストラクタ / Unity ライフサイクル / public メソッド / protected メソッド / internal メソッド / private メソッド。
- interface の明示的実装は、関連する public / protected メソッド群の近く、または private メソッドの直前にまとめる。

## Attribute 記法
- `[SerializeField, Tooltip("...")]` の横並び記法を使用する。
- attribute は短く収まる場合は横並びで記述する。
- attribute 引数が長い場合や可読性が落ちる場合は、無理に横並びにせず改行してよい。

## 差分方針
- 既存ファイルを修正する場合は、周辺スタイルとの整合を優先する。
- 新規ファイルを作成する場合は、本ガイドラインと `.editorconfig` を基準にする。
- 本質と無関係な全面整形は避け、変更理由がある箇所に差分を寄せる。
