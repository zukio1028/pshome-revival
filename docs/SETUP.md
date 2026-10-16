# 初回セットアップ手順（最小ラウンジ）

このリポジトリには C# スクリプトと設計資料だけが入っています。Unity プロジェクト本体（ProjectSettings、シーン、プレハブ）は以下の手順で手元で作り、コミットしてください。

対象スクリプト：`Assets/_Project/Scripts/`

| ファイル | 役割 |
|---|---|
| `Network/ConnectionManager.cs` | Fusion Shared Mode でラウンジに接続。満員なら lounge-2, lounge-3 … へ |
| `Network/PlayerSpawner.cs` | シーン読み込み完了後に自分のアバターを Spawn |
| `Player/NetworkPlayer.cs` | 名前・アバター番号・速度の同期、チャット RPC |
| `Player/PlayerMovement.cs` | スティック / WASD でカメラ基準移動（自分のアバターのみ） |
| `Player/AvatarCatalog.cs` | 同梱 VRM アバターの一覧（ScriptableObject） |
| `Player/Nameplate.cs` / `Chat/ChatBubble.cs` | 頭上の名札と吹き出し |
| `Chat/ChatManager.cs` | 画面下のチャット欄（ログ・入力・連投制限） |
| `UI/LobbyUI.cs` | 入室画面（ニックネームとアバター選択） |

---

## 1. Unity プロジェクトを作る
1. Unity Hub で **Unity 6 の最新 LTS** を入れる（モジュール：Android Build Support、iOS Build Support）。
2. `git lfs install` を一度実行してから、このリポジトリを clone する。
3. Unity Hub で **Universal 3D（URP）** テンプレートの新規プロジェクトを別の場所に作り、作られた中身（`Assets/` `Packages/` `ProjectSettings/`）を clone したフォルダにコピーして、そのフォルダを Unity Hub で開く。
4. File > Build Profiles で Android（または iOS）に切り替える。

## 2. パッケージを入れる
- **Input System**（Package Manager）。Player Settings の Active Input Handling は「Input System Package (New)」または「Both」。
- **Cinemachine**（3.x）。
- **TextMeshPro**（Unity 6 では uGUI に同梱。初回に TMP Essentials を Import）。日本語は Noto Sans JP から TMP Font Asset を作る（Dynamic でよい）。
- **Photon Fusion 2 SDK**：Photon の公式サイトからダウンロードして Import。
- **UniVRM（VRM 1.0）**：GitHub の vrm-c/UniVRM の Releases から最新の `VRM-*.unitypackage`（VRM 1.0 用）を Import。

## 3. Photon の設定
1. Photon ダッシュボードで **Fusion** 用のアプリを作り、App ID を取得。
2. Unity で Tools > Fusion > Fusion Hub を開き、App ID を貼り付ける。
3. このリポジトリは公開設定です。App ID が載る `PhotonAppSettings.asset` をコミットする前に、リポジトリを非公開にすることをおすすめします（他人に CCU を使われるのを防ぐため）。

## 4. VRM アバターを用意する
1. `.vrm` ファイルを `Assets/_Project/Art/Avatars/` に置く。UniVRM が自動でプレハブを生成する。
   - 配布物に同梱するので、**再配布・改変・商用利用の条件を必ず確認**すること（VRoid Studio で自作したものが一番安全）。
2. Project ウィンドウで Create > PsHome > Avatar Catalog を作り、各アバターの表示名・プレハブ・サムネイルを登録。

## 5. 移動アニメーション
1. Asset Store の「Starter Assets - Third Person」（無料）など Humanoid の Idle / Walk / Run を用意。
2. Animator Controller `Locomotion` を作り、Float パラメータ `Speed` で Blend Tree（0 = Idle、2 = Walk、4.5 = Run）。

## 6. プレハブを作る
### Player（`Assets/_Project/Prefabs/Player.prefab`）
- ルート：`NetworkObject`、`NetworkTransform`、`CharacterController`（Center Y 0.8 / Radius 0.3 / Height 1.6）、`NetworkPlayer`、`PlayerMovement`
- 子 `AvatarRoot`（空）→ NetworkPlayer の Avatar Root に設定
- 子 `CameraTarget`（Y 1.4）→ Camera Target に設定
- 子 `Nameplate`（World Space Canvas + TMP Text、Y 2.0、Scale 0.01）→ `Nameplate` を付けて Label を設定
- 子 `ChatBubble`（World Space Canvas + 背景 Image + TMP Text、Y 2.3）→ `ChatBubble` を付けて Root と Label を設定
- NetworkPlayer に Avatar Catalog と Locomotion Controller を設定

### NetworkRunner（`Assets/_Project/Prefabs/NetworkRunner.prefab`）
- `NetworkRunner`、`NetworkSceneManagerDefault`、`PlayerSpawner`（Player Prefab に上の Player を設定）

## 7. シーンを作る
Build Settings のシーン一覧は **0 = Boot、1 = Lounge** の順にする（ConnectionManager がこの番号を使う）。

### Boot
- 空の GameObject `ConnectionManager` → `ConnectionManager`（Runner Prefab に上の NetworkRunner）
- Canvas：ニックネーム入力（TMP_InputField）、アバター選択（TMP_Dropdown）、入室ボタン、状態表示テキスト → `LobbyUI`
- EventSystem（Input System UI Input Module）

### Lounge
- 床（Plane など、後で Asset Store のラウンジ素材に差し替え）
- スポーン位置：空の GameObject を数個置き、Tag を **Respawn** にする
- Main Camera に `CinemachineBrain`、別に `CinemachineCamera`（Body: Third Person Follow または Orbital Follow）
- Canvas（Screen Space）：
  - 画面下にチャット欄（ScrollRect + ログ用 TMP Text、TMP_InputField、送信ボタン）→ `ChatManager`
  - 画面左下に On-Screen Stick（Control Path：`<Gamepad>/leftStick`）
- EventSystem（Input System UI Input Module）

## 8. 動作確認
- エディタで Boot から再生 → 入室できて、自分のアバターが動き、チャットが表示されること。
- 2 人目は実機ビルド、または Unity 6 の Multiplayer Play Mode（Package Manager の `Multiplayer Play Mode`）で仮想プレイヤーを立ち上げて確認。
- 確認項目：互いの移動が滑らかに見える / 名札が出る / チャットと吹き出しが全員に届く / 片方を終了するともう片方からアバターが消える。

うまく動かない場合は、Console のエラー全文をスレッドに貼ってください。
