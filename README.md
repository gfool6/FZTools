# FZTools

### VRChatにおけるアバターのセットアップや改変をちょっと楽にするツール群です
### 各種自動生成ファイルは FZTools/AutoCreate以下に作成されます
<br>
<br>

インストールは[こちら](https://gfool6.github.io/vpm-repos)

<br>
<br>

# 実装済み

## AutoExpressionCreator
### 機能
- 現在の顔メッシュに設定されたBlendshapeの値orメニューから編集した値をテンプレートとして表情アニメーションを作成します
  - ハンドジェスチャーおよび表情メニュー（2ページ分）のアニメーターを自動で作成します
- アバターの服・小物を個別にオンオフするアニメーション・アニメーターを自動で作成します
- それらを適用したFX Controller/ExpressionMen/ExpressionParameterを作成します
  - 簡易的な服・小物の全脱着メニューも作成されます
- 作成されたAnimator・Expressionをアバターに自動で適用できます
<br>

## BlendshapeTransfer
### 機能
- 選択したアバターから、選択したメッシュのブレンドシェイプを、選択したAnimationファイルに転写します
- 選択したAnimationファイルから、選択したアバターの選択したメッシュへ、ブレンドシェイプの値を転写します
<br>

## BulkMenuCreator(β)
### 機能
- MAを使用したメニューの雛形を生成できます
  - MA Menu Installer/Menu Group/Menu Items/Object Toggleを一括で設定します
  - MA Menu Itemの各種設定を自動で設定します
- 簡易な表示切り替えだけであればこれだけで生成可能です
- 生成したメニューの設定情報をファイルに保存し、そこから同じメニューを復元可能です
- ※ModularAvatar必須
<br>

## FaceAnimationCreator
### 機能
- 現在の顔メッシュのブレンドシェイプからアニメーションファイルを作成します
- ファイル名は任意の名前にすることができます
- Write Defaults ONの場合/Offの場合など、柔軟に作成可能です
<br>

## MMDMeshCreator
### 機能
- MMD用Blendshapeを追加したメッシュを作成します
<br>

## ScaleTransfer(β)
### 機能
- ArmatureのScaleやTransformなどを読み取り、適用
- ScaleAdjusterをコピー
- ※ModularAvatar必須
<br>

## ヌイデネ(仮)
### 機能
- 改変済みアバターから着せ替え・改変用の素体prefabを生成します
- 素体として扱うメッシュや服・小物を選択できます
- 素体以外のメッシュを非表示+EditorOnly化するか削除するかを選べます
- 簡易的なPhysbone削除も対応
<br>

<br><br><br>

# 開発予定

## キメラかんたんセットアップ
### 機能
- 首と胴体が異なるいわゆる「キメラアバター」の作成支援
- 頭用アバター・胴体用アバターを指定してざっくり合わせる
- 胴体用アバターにMAで頭をマージ
- 胴体用アバター側のDescriptor設定を自動修正
- ※ModularAvatar必須

## BlendShapeの名前変えるやつ
### 機能
- 読んで字の如く以上の機能がなくて何も説明できない
- 複製・削除・追加の機能（開発中）

## AnimationClipを良しなに作ってくれるやつ(アニメーション作成・編集)
### 機能
- メッシュのBlendShapeやオンオフ追加や削除して楽にアニメーションを作成できます
- 既存のアニメーションを編集することができます（開発中）
- マテリアル差し替えアニメーションの作成をすることができます（開発中）

## ~~AnimatorControllerを編集・共有しやすくするやつ(FZAnimatorPresetTools)~~
### ~~機能(全部開発中)~~
- ~~Animator ControllerはLayerの複製やリネーム・パラメータ変更などが若干めんどくさいのでそれを解消したいもの~~
- ~~AnimatorControllerとFZ Presetファイルとの相互変換~~
- ~~FZ Presetファイルの生成~~
