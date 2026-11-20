# CUBE ORBIT（Unity版）

キューブを転がして宇宙のゴールを目指すパズルアクションです。
ギミックごとにスクリプトを分け、数値はすべてインスペクターで調整できます。
クリックでブロックを置けるステージエディタと、クリアできるかを自動チェックする検証機能も付いています。

---

## 動作環境

- Unity 2022.3 LTS / Unity 6（**3D (Built-in)** でも **Universal 3D (URP)** でもOK）
- HDRP は非対応（シェーダーが動かないため）
- 旧Input Manager・新Input System のどちらの設定でも動きます

---

## はじめ方（5分）

1. この `CubeOrbit` フォルダを、Unityプロジェクトの `Assets` フォルダの中にドラッグして入れる
2. メニュー **Tools > Cube Orbit > 1. 初期セットアップ** を押す
   → `CubeOrbit/Materials` と `CubeOrbit/Prefabs` が自動で作られます
3. メニュー **Tools > Cube Orbit > ステージエディタ** を開く
4. **「サンプル15ステージを生成」** を押す
   → Web版の15ステージが `CubeOrbit/Stages/Samples` にシーンとして作られ、Build Settings にも登録されます
5. メニュー **Tools > Cube Orbit > 3. タイトル画面を作る** を押す
   → タイトル画面（スタート・ステージ選択）が Build Settings の先頭に入ります
6. `CubeOrbit/Stages/Title` を開いて ▶（再生）で遊べます。クリアすると次のステージへ進みます

### 操作

| 操作 | キーボード | スマホ/マウス |
|---|---|---|
| 転がる | 矢印キー / WASD（↑ = 画面の右上） | スワイプ、または画面の四隅をタップ |
| やり直し | R | 右下の ↺ |
| ポーズ | Esc / P | 右下の Ⅱ |
| サウンドON/OFF | M | ポーズ画面 |

---

## ステージの作り方

### 1. 新しいステージを作る
ステージエディタの **「新しいステージ」** を押して、保存場所を選びます。
カメラ・GameManager・プレイヤー・スタート床・ゴールが入ったシーンができ、Build Settings に追加されます。

### 2. クリックで置く
1. ステージエディタの **「置く物」** から選ぶ（ブロック、ジェム、動く床…）
2. シーンビューをクリックすると置かれます
   - **ブロックの面をクリック** → その面の隣のマスに置かれます（マイクラと同じ感覚）
   - **何もない所をクリック** → 「何もない所の高さ」の高さに置かれます
   - **Shift+クリック** → 削除
   - **Ctrl+Z** → 元に戻す
3. 置いた物を選ぶと、インスペクターで細かい数値を変えられます

### 3. ブロックは「大きさ」で作る
ブロックは1個でいくつものマスをまとめて作れます。インスペクターの `size` を変えるだけで、床・通路・壁・柱になります。
エディタの **「下に伸ばす」** を使うと、下に向かって伸びた柱になり、Web版のように「闇から生えている」見た目になります（3〜4がおすすめ）。

### 4. アイテムは「ブロックの上のマス」に置く
ジェム・スイッチ・チェックポイント・ゴールは、床ブロックの **1つ上のマス** に置きます（床の面をクリックすれば自動でそうなります）。

### 5. 検証する
**「ステージを検証」** を押すと、プレイヤーの開始位置から総当たりで調べて、

- ゴールにたどり着けるか
- 取れないジェムがないか

を教えてくれます。取れないジェムがあれば、ボタン1つで選択されます。
（押し出す壁や動く床の **タイミング** までは見ないので、最後は実際に遊んで確認してください）

### 6. ステージ情報
シーン内の `GameManager` のインスペクターで変えられます。

| 項目 | 内容 |
|---|---|
| Stage Name | 画面左上に出る名前 |
| Stage Number | 画面に出す番号（0なら Build Settings の順番） |
| Hint | 開始時に画面下に出る説明 |
| Par Time | ランク計算の目安タイム |
| Auto Kill Y | 一番低いブロックの少し下を自動で落下ラインにする |
| Next Scene Name | 空なら Build Settings の次のシーンへ |

ステージの順番は **File > Build Settings** のシーンの並び順です（0番がタイトル、1番からがステージ）。
タイトルのステージ選択には、Build Settings に入っているステージが自動で全部並びます。

---

## ギミックと調整できる数値

| ギミック | スクリプト | 主な数値 |
|---|---|---|
| ブロック | `BlockVolume` | `size`（大きさ） |
| 透明ブロック | `GhostBlockVolume` | `group`（スイッチ番号）、出現アニメの速さ |
| スイッチ | `FloorSwitch` | `group`（同じ番号の透明ブロックが出る） |
| 動く床 / エレベーター | `MovingPlatform` | `moveOffset`（移動量）、`speed`、`pauseTime`（止まる時間）、`timeOffset`（タイミングずらし）、乗り降りの猶予 |
| 押し出す壁 | `Pusher` | `pushDirection`、`period`（周期）、`timeOffset`、飛び出し/停止/戻りの時間、予告の点滅時間 |
| 崩れる床 | `CrumbleTile` | `crumbleDelay`（崩れるまでの時間）、震え、落ちる速さ |
| ジェム | `Gem` | 色、回転・揺れ |
| チェックポイント | `Checkpoint` | 色 |
| ゴール | `Goal` | 色の変化、脈打ち |

### タイミングのコツ
動く床と押し出す壁は、すべて **ステージ開始からの時間**（`GameManager.StageTime`）で動きます。
同じ設定なら必ず同じタイミングになるので、`timeOffset` をずらすだけで「交互に動く」「順番に動く」が作れます。

- 押し出す壁を3つ並べて `timeOffset` を 0 / 0.7 / 1.4 → 波のように順番に飛び出す
- 押し出す壁の間には **安全なマスを1つ空ける** と遊びやすい
- 動く床を待つ場所は **崩れる床にしない**（待っている間に落ちてしまう）

---

## プレイヤー・カメラの数値

| 対象 | スクリプト | 主な数値 |
|---|---|---|
| キューブの動き | `PlayerCube` | 転がる時間、よじ登る時間、重力、押し出され時間、出現・ゴール演出 |
| 入力 | `CubeInput` | ↑キーの画面上の向き、スワイプの距離、タップ移動、先行入力の時間 |
| キューブの色 | `CubeColorCycle` | 色が変わる速さ、彩度、明るさ |
| 影 | `BlobShadow` | 濃さ、高さによる薄まり方 |
| カメラ | `CameraFollow` | 見下ろす角度、ズーム、追従の速さ、揺れ |
| 星 | `StarField` | 数、大きさ、瞬き |
| 効果音 | `SoundPlayer` | 音量。自分の AudioClip を入れれば差し替え（入れない音は電子音を自動生成） |
| 画面表示 | `StageHUD` | 文字サイズ、色、ヒントが消えるまでの手数 |

---

## スクリプト構成

```
CubeOrbit/
├─ Scripts/
│  ├─ Core/                 … ゲームの土台
│  │  ├─ GridUtil.cs        … 方向・色・共通ヘルパー
│  │  ├─ GridElement.cs     … グリッドに置く物すべての親クラス（自動スナップ付き）
│  │  ├─ GridWorld.cs       … 「このマスは固い？」の問い合わせをまとめる
│  │  ├─ GridInterfaces.cs  … 乗った/入った/リセット時に呼ばれる仕組み
│  │  ├─ MoveRules.cs       … 「この方向に動いたらどうなるか」のルール（1か所だけ）
│  │  ├─ SwitchSystem.cs    … スイッチ番号のON/OFF
│  │  └─ GridSolver.cs      … ステージ検証（総当たり探索）
│  ├─ Player/
│  │  ├─ PlayerCube.cs      … 転がる・登る・落ちる・押される・乗るのアニメと状態
│  │  ├─ CubeInput.cs       … キー/スワイプ/タップ → 方向
│  │  ├─ InputCompat.cs     … 新旧Input Systemの両対応
│  │  ├─ CubeColorCycle.cs  … 虹色に変わる
│  │  └─ BlobShadow.cs      … 足元の影
│  ├─ Gimmicks/
│  │  ├─ BlockVolume.cs     … ブロック
│  │  ├─ GhostBlockVolume.cs… 透明ブロック
│  │  ├─ FloorSwitch.cs     … スイッチ
│  │  ├─ MovingPlatform.cs  … 動く床 / エレベーター
│  │  ├─ Pusher.cs          … 押し出す壁
│  │  └─ CrumbleTile.cs     … 崩れる床
│  ├─ Items/
│  │  ├─ Gem.cs / Checkpoint.cs / Goal.cs
│  ├─ Game/
│  │  ├─ GameManager.cs     … 進行（タイム・ジェム・復活・クリア・次へ）
│  │  ├─ TitleScreen.cs     … タイトル画面とステージ選択
│  │  ├─ CameraFollow.cs / StageHUD.cs / SoundPlayer.cs / BurstEffect.cs / StarField.cs
│  └─ Editor/
│     ├─ CubeOrbitSetup.cs  … マテリアルとプレハブを自動生成
│     ├─ StageEditorWindow.cs … ステージエディタ
│     ├─ StageFactory.cs    … シーン作成・配置・検証の処理
│     └─ SampleStages.cs    … Web版15ステージのデータ
└─ Shaders/
   ├─ CubeOrbitBlock.shader … ブロック用（マスの境目・下に行くほど闇に溶ける）
   └─ CubeOrbitGhost.shader … 透明ブロック・影用
```

### 動きの流れ（1フレーム）
1. `GameManager` が `StageTime` を進める
2. 各ギミック（動く床・押し出す壁・崩れる床）が `StageTime` から自分の状態を計算する
3. `PlayerCube` が入力を受け取り、`MoveRules.Resolve()` で行き先を決めてアニメーションする
4. マスに着いたら `GridWorld.NotifyEnter / NotifyLand` で、そのマスの物（ジェム・スイッチ・崩れる床…）に知らせる

---

## 新しいギミックを作るには

`GridElement` を継承して、必要なところだけ上書きします。

```csharp
using UnityEngine;
namespace CubeOrbit
{
    [ExecuteAlways]
    public class MyGimmick : GridElement, IPlayerEnterHandler
    {
        public float myValue = 1f;   // インスペクターで調整できる

        // そのマスが固いか（進めない・上に乗れる）
        public override bool Blocks(Vector3Int c) { return c == Cell; }

        // 検証ツールから見た固さ（時間を無視）
        public override bool SolverSolid(Vector3Int c, int mask) { return c == Cell; }

        // プレイヤーがこのマスに入った時
        public void OnPlayerEnter(PlayerCube player, bool passingThrough) { }
    }
}
```

| 上書きできるもの | 意味 |
|---|---|
| `Blocks(c)` | 固い（進めない） |
| `Climbable(c)` | よじ登れる（既定は Blocks と同じ） |
| `IsFloor(c)` | 上に立てる（既定は Blocks と同じ） |
| `BodyWait(c)` / `FloorWait(c)` | 今は入らずに待つ（動く床の通り道など） |
| `SolverSolid(c, mask)` | 検証ツール用の固さ |
| `IPlayerEnterHandler` | プレイヤーがこのマスに入った |
| `IPlayerLandHandler` | プレイヤーがこのブロックの上に乗った |
| `IStageResettable` | 落下してやり直す時に元に戻す |

---

## よくある質問

**Q. ピンク色になる**
シェーダーが読み込めていません。`Shaders` フォルダが入っているか、HDRP ではないかを確認して、もう一度「初期セットアップ」を押してください。

**Q. 操作の向きを変えたい**
プレイヤーの `CubeInput > Up Key Screen Angle` を変えます（0 = 真上、45 = 右上）。カメラの角度を変えても、操作は画面の見た目に自動で合わせます。

**Q. カメラをもっと寄せたい**
`Main Camera > CameraFollow > Orthographic Size` を小さくします。

**Q. 効果音を差し替えたい**
`GameManager > SoundPlayer > Sounds` に種類と AudioClip を追加します。
