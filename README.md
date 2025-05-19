# FZTools

### VRChatにおけるアバターのセットアップや改変をちょっと楽にするツール群です
### 各種自動生成ファイルは FZTools/AutoCreate以下に作成されます
<br>

<br>

# 実装済み

## Expression自動作成機(なんかいろいろ自動で作るやつ)
### 機能
- 現在の顔メッシュに設定されたBlendshapeの値orメニューから編集した値をテンプレートとして表情アニメーションを作成します
- アバターの服・小物を個別にオンオフするアニメーションを自動で作成します
- それらを適用したFX Controller/ExpressionMen/ExpressionParameterを作成します
  - 簡易的な服・小物の全脱着メニューも作成されます
- 作成されたAnimator・Expressionをアバターに自動で適用できます
<br>

## ヌイデネ(仮)
### 機能
- 改変済みアバターから着せ替え・改変用の素体prefabを生成します
- 素体として扱うメッシュや服・小物を選択できます
- 素体以外のメッシュを非表示+EditorOnly化するか削除するかを選べます
- 簡易的なPhysbone削除も対応
<br>

## MMD用メッシュ作成
### 機能
- MMD用Blendshapeを追加したメッシュを作成します
<br>

## BlendshapeTransfer
### 機能
- 選択したアバターから選択したメッシュのブレンドシェイプをAnimationファイルに転写します
- ↑の逆の機能（Animation→Blendshape）
<br>

## 今の顔から表情Animation作るやつ
### 機能
- 現在の顔メッシュのブレンドシェイプからアニメーションファイルを作成します
- ファイル名は任意の名前にすることができます

<br><br><br>

# 開発中

## BlendShapeの名前変えるやつ
### 機能
- 読んで字の如く以上の機能がなくて何も説明できない
- 複製・削除・追加の機能（開発中）

## AnimationClipを良しなに作ってくれるやつ(アニメーション作成・編集)
### 機能
- メッシュのBlendShapeやオンオフ追加や削除して楽にアニメーションを作成できます
- 既存のアニメーションを編集することができます（開発中）
- マテリアル差し替えアニメーションの作成をすることができます（開発中）

## AnimatorControllerを編集・共有しやすくするやつ(FZAnimatorPresetTools)
### 機能(全部開発中)
- Animator ControllerはLayerの複製やリネーム・パラメータ変更などが若干めんどくさいのでそれを解消したいもの
- AnimatorControllerとFZ Presetファイルとの相互変換
- FZ Presetファイルの生成