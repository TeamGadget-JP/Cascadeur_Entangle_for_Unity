# 日本語 <br>
# Cascadeur Entangle for Unity (CEU) βテスト<br>
CEUは前作"Gadget Entangle for Cascadeur (GEC)"の後継バージョンになります。<br>
基本的な機能全般はGECを継承しつつ、大幅にブラッシュアップを施しました。<br>

# CEUの概要<br>
1. リアルタイム同期<br>
2. リグ・アグノスティック同期<br>
3. エディターモード同期・プレイモード同期<br>
4. 複数キャラクター同時同期<br>
5. UIを撤廃しインスペクター側に集約<br>
6. キャラクター個別にローカルポートを割り当てることで堅牢化<br>
8. ルートボーン移動を伴うモーションに対応<br>
9. 部位Lerpの実装<br>
10. iCloneフェイシャルとのハイブリッドシステム<br>
11. プロップ転送と同期（現在実装作業中）<br>

# リグ・アグノスティック同期とは?　（検証段階）<br>
Unityから動的にボーン階層・名称をCascadeurとハンドシェイクすることにより<br>
リターゲット作業を徹底的に排除することを目標としました。Unity、Cascadeurにインポートして<br>
リギングできるキャラクターなら理論上何でも同期できると考えています。<br>
それが例えば人間型であろうが四足型であろうがメカ、多足、クリーチャー、その他、Cascadeurで<br>
リギングできるキャラクターならほぼ全て同期できると予想しています。<br>

# ローカルポート割り当て <br>
8900 システム専用<br>
8901 キャラクター1<br>
8902 キャラクター2<br>
・<br>
・<br>
8909 プロップ転送・同期専用<br>

# 導入手順 <br>
1. `CEU_Sender_v1.pyc`をCascadeurのPythonプラグインフォルダに配置します。<br>
   `[Cascadeurインストール先]\resources\scripts\python\commands\`<br>
2. `CEU_System.cs``CEU_Avatar.cs`をUntyのProjectに任意のフォルダーを作り、ドラッグ・アンド・ドロップ<br>

# 使用方法 <br>
step1: 同じキャラクターを双方へインポート<br>
step2: Cascadeur側`Commands -> CEU_Sender_v1`を選択して通信開始。<br>
step3: Unity側、ヒエラルキーに空のゲームオブジェクトを作成して`CEU_System.cs`をアタッチ<br>
step4: Unity側、ヒエラルキー内の同期したいキャラクターに`CEU_Avatar.cs`をアタッチ<br>
step5: `CEU_System.cs`をアタッチしたゲームオブジェクトのインスペクターで`Connect To Cascadeur`トグルをオン<br>
以上です。<br>

# 同期しない場合 <br>
1. Target Port番号は合ってますか？<br>
2. 8901ポートのキャラクターはプリフィックスは付きませんので`Cascadeur Prefix`は何も記入しません<br>
3. 8902ポート以降から`Cascadeur Prefix`に`character1:``character2:`とプリフィックスを記入します<br>
4. `Rig Type`を`Humanoid`や`Generic`に切り替えてみてください<br>

# Cascadeurでの複数キャラクターセットアップ方法<br>
例 : 2体セットアップ<br>
1. シーンを作成、最初の1体目を通常通りインポート -> リギング。<br>
2. 2体目用に更にシーンを作ります。そのまま2体目を通常通りインポート -> リギング。<br>
3. 2体目が居るシーンを保存して閉じます。<br>
4. 1体目が居るシーンに戻って、File -> Import -> Import Scene To Current...で2体目のシーンをインポート。<br>
5. 2体目のボーン名に自動でcharacter1:のプレフィックスが付与されます。<br>
6. 3体目も同じ手順となります。<br>

Team Gadget Youtube<br>
https://www.youtube.com/channel/UCj9OYwzMAIgYAeVkTV4wczw<br>

# 免責事項 <br>
CEUはTeamGadgetによる独立したプロジェクトです。<br>
CascadeurはNekkiの商標または財産です。 <br> 
Unityはunity technologies incの商標または財産です。<br>

本プロジェクトは、Nekkiまたはunity technologies incによる公式製品ではなく、承認、提携、スポンサー提供、または公式サポートを受けたものではありません。<br>
