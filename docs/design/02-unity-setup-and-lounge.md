# Step 2：Unity 初期構成と最小ラウンジ（接続 → 移動 → チャット）

**決定（2026-10-16）**：Unity 6 LTS + URP / アバターは VRM 読み込み（UniVRM） / GitHub リポジトリに PR で納品

前提（決定済み）：Photon Fusion 2 Shared Mode / ボーリングはハイブリッド同期 / ラウンジ内レーン・観戦可・1レーン最大4人

---

## 1. 初期構成（案）

| 項目 | 提案 | 理由 |
|---|---|---|
| Unity バージョン | **Unity 6 系の最新 LTS** | Fusion 2 対応、長期サポート。途中でメジャー更新しない |
| レンダリング | **URP**（Mobile 向け設定） | モバイルの標準。Asset Store 素材も URP 対応が主流 |
| ネットコード | Photon Fusion 2（Shared Mode）SDK | 決定済み |
| 入力 | Input System ＋ 画面左バーチャルスティック（On-Screen Stick） | 標準パッケージで追加費用なし |
| カメラ | Cinemachine（三人称追従） | ノーコードで追従・衝突回避 |
| UI | uGUI ＋ TextMeshPro（日本語フォントは Noto Sans JP をサブセット化） | チャット入力・吹き出しに十分 |
| アバター | 質問2を参照 | |
| バージョン管理 | GitHub ＋ Unity 用 .gitignore ＋ Git LFS（モデル・テクスチャ・音） | 個人でも履歴とバックアップは必須 |

### フォルダ構成
```
Assets/
  _Project/
    Scripts/
      Network/    … 接続・セッション管理（ConnectionManager）
      Player/     … アバター移動・アニメ同期（PlayerController / NetworkPlayer）
      Chat/       … チャット送受信・吹き出し（ChatManager / ChatBubble）
      Bowling/    … （Step 3 で追加）
      UI/
    Prefabs/  Scenes/  Art/  Settings/
  ThirdParty/   … Asset Store 素材はここへ隔離（更新時の差分を見やすく）
```

---

## 2. 最小ラウンジの実装手順

1. **接続**：起動 → ニックネーム入力 → `NetworkRunner.StartGame(GameMode.Shared, SessionName="lounge-1", PlayerCount=24)`。満員なら `lounge-2` に自動フォールバック。
2. **スポーン**：`PlayerJoined` で自分のアバター Prefab を `Runner.Spawn`（Shared Mode なので自分が State Authority）。
3. **移動**：自分のアバターだけ入力で `CharacterController` を動かす。`NetworkTransform` が位置を同期し、他人は補間表示。歩き/走りは `[Networked] float Speed` を Animator に流す。
4. **チャット**：`[Rpc(RpcSources.All, RpcTargets.All)] RPC_SendChat(string msg)`。受信側でログ欄に追加し、発言者の頭上に吹き出しを数秒表示。文字数上限（例：60字）と送信間隔制限（例：1秒）をクライアント側で入れる。
5. **名札**：`[Networked] NetworkString<_16> Nickname` を頭上の TextMeshPro に表示。
6. **実機確認**：iOS/Android 実機2台＋エディタ1つで同時接続し、移動・チャット・切断/再接続を確認。

完了の目安：3端末で互いの移動が滑らかに見え、チャットと吹き出しが全員に届くこと。

### 後回しにするもの（MVP外）
- NGワードフィルタ、通報・ブロック（ストア審査前には必要。公開前の Step で対応）
- 音声チャット、フレンド、アバターの着せ替え保存

---

## 3. 判断してほしいこと（推奨付き）

1. **Unity / 描画**：Unity 6 LTS ＋ URP（推奨）／ Unity 2022 LTS ＋ URP（既存資産がある場合）
2. **アバター素材**：Asset Store のローポリ人型パック（数体のプリセットから選ぶだけ）（推奨）／ VRM 読み込み（UniVRM、自作アバター対応だが工数増）／ 自作モデル
3. **コードの置き場所**：GitHub にリポジトリを作り、こちらから C# スクリプトとセットアップ手順を PR で出す（推奨）／ このプロジェクトのファイル置き場にスクリプトを置き、手でコピーする

※ この環境では Unity エディタを動かせないため、シーン作成・Prefab 組み立て・実機ビルドはかずきさんの手元で行い、スクリプトと手順はこちらで用意する分担を想定しています。
