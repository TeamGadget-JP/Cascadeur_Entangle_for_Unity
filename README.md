# 日本語 #
# Cascadeur Entangle for Unity (CEU) βテスト<br>

# GECBの主要機能
1. CascadeurとBlenderをシームレスに繋ぐリアルタイム同期。
2. BlenderのObject Mode / Pose Modeに関係なく同期可能。
3. Blender側でタイムライン・アニメーションを再生している最中でも同期可能。  
   ただし、対象ボーンにキーフレームが打たれていないことが前提です。
4. 最大4体までのキャラクターを同時同期可能。  
   ただし、PCスペックに依存します。
5. Zero Calibrationにより、CascadeurとBlender間のキャラクター状態を補正。
6. Zero Calibration情報の保存と解除。

---

## 動作環境

- Windows専用  
  GECBはコード内でWindows APIを使用しています。

- 動作確認バージョン  
  - Cascadeur 2026.1.3  
  - Blender 5.1

- 動作確認PC  
  - GPU: NVIDIA RTX 3060 12GB  
  - CPU: AMD Ryzen 7 8-Core  
  - RAM: 32GB

- Cascadeurライセンスについて  
  GECBの利用には、有効なCascadeur有償ライセンスが必要です。  
  Cascadeur無料版では利用できません。

---

## 前提条件

GECBは、CC互換のボーン階層・命名規則を持つキャラクターを対象に設計しています。

想定されるボーン構造およびボーン命名規則については、サンプルキャラクターを参照してください。

### 重要事項

- ボーン階層が想定構造に一致していること。
- ボーンの向きが一致していること。
- ボーン命名規則が対応形式に沿っていること。

GECBは、生ボーンの姿勢を直接同期するツールです。  
汎用リグ変換機能はありませんのでキャラクターのボーンセットアップは
無料ソフトウェアのAccuRIG等を使われる事を推奨します。
https://actorcore.reallusion.com/auto-rig/accurig

### GECBがCC互換のボーン階層・命名規則を採用している理由

GECBでは、以下の理由によりCC互換のボーン階層・命名規則を採用しています。

1. キャラクター制作ワークフローで広く使用されており、高い互換性が期待できるため。
2. Cascadeurのリギング機能がCC互換のキャラクター構造と相性が良く、セットアップ時のトラブルや同期不具合を減らしやすいため。

このため、GECBでは安定したリアルタイム同期を行うための推奨基準として、CC互換スケルトンを採用しています。

---

## 導入手順・使用方法

## 導入手順<br>
1. `GECB_Sender_v1_0.pyc`をCascadeurのPythonプラグインフォルダに配置します。<br>
   `[Cascadeurインストール先]\resources\scripts\python\commands\`<br>
2. `GECB_Receiver_v1_0.zip`をBlenderのアドオン登録します。<br>
3. `SampleCharacter.blend`と`SampleCharacter.casc`を開きます。<br>
4. Cascadeurで`Commands -> GECB Sender(v1_0)`を選択する `Event log に[GECB] Sender Started (v1.0)`と表示されば準備完了です。<br>
5. Blender側は`Nパネル -> GECB -> START SYNC`で接続開始します。<br>
6. Cascadeur側を停止するにはもう一度`Commands -> GECB Sender(v1_0)`を選択してください。<br>
   注意　シーンを閉じる時や終了する時は必ず接続を切ってください。<br>

## 使用方法<br>

キャラクターセットアップのチュートリアル動画<br>
https://youtu.be/-p8h0pQ99co

step1: ご自身で同期させたいキャラクターを用意してください。(自作、CC4&5、フリーモデル等)<br>
step2: まずBlenderへキャラクターをインポートします、キャラサイズを実サイズにします。<br>
step3: ボーン構成、向き、命名をCC互換にしてください、またはAccuRIG等を使用してCC互換にしてください。<br>
step4: CascadeurへFBXでエクスポート。FBX設定は下記のURLを参照<br>
　　　　https://cascadeur.com/help/getting_started/import_fbxdae/import_from_blender<br>
step5: Cascadeurでインポート、リギングまで完了した後、`Box Controller Mode -> 全選択 -> Commands -> Go to T-Pose`を実施して基準姿勢にします。<br>
　　　　(この姿勢から絶対に動かさないでください)<br>
step6: Cascadeurで`Commands -> GECB Sender(v1_0)`を選択して通信開始。<br>
step7: BlenderでGECBパネルを開きSlot0へキャラクターをセットします。<br>
step8: START SYNCボタンを押します、キャラクターが変形しますが問題ありません。<br>
step9: Zero Calib (Live Stream)ボタン -> OK でキャラクターがCascadeur側と同じポーズになったら成功です。<br>
　　　　(この時、同じポーズにならない場合はボーン構成、向き、命名を再確認してください)<br>
step10: 同期完了です！リアルタイム・プレビズの世界へようこそ！<br>

## 更に詳細な説明<br>
自作キャラクターやAccuRIGを通したキャラクター等で同期する上で最も重要な事はHipボーンに余計な角度がついて無い事です。<br>
<img width="300" height="391" alt="image" src="https://github.com/user-attachments/assets/865c8cc6-78d1-44ed-b8d9-1e71e8c946eb" /><br>
HipボーンのHead : X=0 Y=0 Tail : X=0 Y=0 Roll = 0 は絶対に守って下さい。<br>
AccuRIGを通してインポートしたキャラクターは高確率でHipボーンに傾きが入ります。<br>
それをボタン一つで修正するスクリプトも用意しました。<br>
`accurig_hip_fix_addon.py`<br>
必要に応じてお使いください。アドオン登録しますとGECBのUIに統合されます。<br>

複数キャラクターの同期について<br>
<img width="300" height="457" alt="image" src="https://github.com/user-attachments/assets/c4485158-1033-415a-86c2-b270fbd6729c" /><br>
GECBでは最大4体までのキャラクターを同時接続できます。<br>
GECBではボーン名の前にプレフィックスを付ける事でキャラクター識別を行っています。<br>
例 : CC_Base_Hip -> character1:CC_Base_Hip<br>
Blender側ではSlot 0:がプレフィックス無し、Slot 1:から順にcharacter1: Character2:...の様に自動的に付与されるようになっています。<br>

Cascadeurでの複数キャラクターセットアップ方法<br>
例 : 2体セットアップ<br>
1. シーンを作成、最初の1体目を通常通りインポート -> リギング。<br>
2. 2体目用に更にシーンを作ります。そのまま2体目を通常通りインポート -> リギング。<br>
3. 2体目が居るシーンを保存して閉じます。<br>
4. 1体目が居るシーンに戻って、File -> Import -> Import Scene To Current...で2体目のシーンをインポート。<br>
5. 2体目のボーン名に自動でcharacter1:のプレフィックスが付与されます。<br>
6. 3体目も同じ手順となります。<br>

## Cascadeurで作ったモーションの最終版をFBXを通じてBlenderのキャラクターに適用する場合<br>
まず説明して置かなければならないのが、リアルタイム同期で使われているボーンアニメーションのデータと<br>
FBXにエクスポートされたボーンアニメーションデータは全く別物です。<br>
リアルタイム同期をする時の条件はTポーズもしくはAポーズであること、同期直後にゼロキャリブレーションされたキャラクターであることです。<br>
逆にFBXを適用できる状態のキャラクターはTポーズもしくはAポーズであること、ゼロキャリブレーションを解除されたキャラクターであることです。<br>
したがって下記の手順を踏むことになります。<br>
1. Tポーズへ戻す（原点復帰）<br>
   CascadeurとBlenderの両方で、キャラクターをTポーズ（初期姿勢/レストポーズ）に戻します。<br>
2. 同期の停止（通信切断）<br>
   Blender側のGECBパネルで `STOP SYNC` をクリックし、リアルタイム通信を完全に遮断します。<br>
3. キャリブレーションの消去（メモリ初期化）<br>
   GECBパネルで `Clear Calibrate` をクリックします。<br>
4. FBXの適用（本番データの流し込み）<br>
   CascadeurからアニメーションをFBXとしてエクスポートし、Blenderのキャラクターにインポート（またはリターゲティング）します。<br>

GECBを使用されてもし良かったらチャネル登録、高評価お願いします。<br>
YouTube:[https://www.youtube.com/@TeamGadget](https://www.youtube.com/channel/UCj9OYwzMAIgYAeVkTV4wczw)

## フィードバックのお願い
SampleCharacterは自分のメッシュモデルをAccuRIGを通してCC互換スケルトン&バインドを行って作成したものです。<br>
いまのところCC4&5を使わずにCC互換スケルトンを最短で構築する方法はこれしか思いつきませんでした。<br>

CC4&5を所持されている方は公式ブリッジアドオンを使用してキャラクターをインポートしますとかなり高精度な<br>
同期キャラクターを実現できます。(狂いは全く無いと言って良いレベルです)<br>

ボーン構成で特に重要なのはHipです。HipのTransformでHeadとTail:X=0 Y=0が精度を確保する上で絶対の仕様となりますので、<br>
汎用を組まれる方は特にここに注意してください。<br>

SampleCharacterはHipが傾いた状態でAccuRIGから出力されてたことから、仕方なくBlenderのエディットモードで修正を施しました。<br>
その影響で指先等の位置精度が若干悪くなっています。<br>
この辺は最適なフローがありましたら是非フィードバックを頂けるとありがたいです。

今後のバージョンで皆様から頂いた意見を反映して、より一層良いツールになって行ければと思います。<br>

---

## サポートについて

GECBは完全無料・現状渡しで提供されます。

開発者は普段、別の本業を抱えるFA系個人エンジニアです。  
そのため、個別の環境に合わせた技術サポートを提供することは事実上不可能です。

本ツールは以下の条件で提供されます。

- 完全無料
- サポートなし
- 無保証
- 自己責任での利用

バグ不具合報告へはできる限り対応しますが不定期になります。ご理解ください

---

## 免責事項

GECBはTeamGadgetによる独立したプロジェクトです。

CascadeurはNekkiの商標または財産です。  
BlenderはBlender Foundationの商標または財産です。

本プロジェクトは、NekkiまたはBlender Foundationによる公式製品ではなく、承認、提携、スポンサー提供、または公式サポートを受けたものではありません。
