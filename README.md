# English <br>
# Cascadeur Entangle for Unity (CEU) Beta Test<br>
CEU is the successor to the previous version, "Gadget Entangle for Cascadeur (GEC)".<br>
While inheriting the core functionality of GEC, CEU has been significantly refined and improved.<br>

# Development Environment / System Requirements<br>
Windows only (uses Windows API internally)<br>
Unity 6.5<br>
Editor Version: 6000.5.1f1<br>
Render Pipeline: URP<br>
Cascadeur Pro 2026.1.3<br>

# CEU Overview<br>
1. Real-time synchronization<br>
2. Rig-agnostic synchronization<br>
3. Editor Mode synchronization / Play Mode synchronization<br>
4. Simultaneous synchronization of multiple characters<br>
5. UI removed and integrated into the Inspector<br>
6. Improved robustness by assigning a dedicated local port to each character<br>
8. Supports motions with root bone movement<br>
9. Body-part Lerp implementation<br>
10. Hybrid system with iClone facial animation<br>
11. Prop transfer and synchronization (currently under development)<br>

# What is Rig-Agnostic Synchronization? (Verification Stage)<br>
By dynamically handshaking the bone hierarchy and bone names from Unity to Cascadeur,<br>
the goal is to eliminate retargeting work as much as possible. In theory, any character that can be imported into Unity and Cascadeur and successfully rigged should be able to synchronize.<br>
Whether it is a humanoid, quadruped, mechanical character, multi-legged creature, monster, or any other character that can be rigged in Cascadeur,<br>
it is expected that almost all of them can be synchronized.<br>

# Local Port Assignment <br>
8900 Reserved for the system<br>
8901 Character 1<br>
8902 Character 2<br>
・<br>
・<br>
8909 Reserved for prop transfer and synchronization<br>

# Installation <br>
1. Place `CEU_Sender_v1.pyc` into the Cascadeur Python plugin folder.<br>
   `[Cascadeur Installation Folder]\resources\scripts\python\commands\`<br>
2. Create any folder in your Unity project, then drag and drop `CEU_System.cs` and `CEU_Avatar.cs` into it.<br>

# Usage <br>
Step 1: Import the same character into both Unity and Cascadeur.<br>
Step 2: In Cascadeur, select `Commands -> CEU_Sender_v1` to start communication.<br>
Step 3: In Unity, create an empty GameObject in the Hierarchy and attach `CEU_System.cs`.<br>
Step 4: Attach `CEU_Avatar.cs` to the character you want to synchronize.<br>
Step 5: In the Inspector of the GameObject with `CEU_System.cs`, enable the `Connect To Cascadeur` toggle.<br>
That's all.<br>

# If Synchronization Does Not Work <br>
1. Is the Target Port number correct?<br>
2. Characters using port 8901 do not use a prefix, so leave `Cascadeur Prefix` empty.<br>
3. Starting from port 8902, enter `character1:`, `character2:`, etc. into `Cascadeur Prefix`.<br>
4. Try switching `Rig Type` between `Humanoid` and `Generic`.<br>
5. Always start synchronization from Cascadeur first, then Unity.<br>

# Setting Up Multiple Characters in Cascadeur<br>
Example: Setting up two characters<br>
1. Create a scene, then import and rig the first character as usual.<br>
2. Create another scene for the second character. Import and rig the second character as usual.<br>
3. Save and close the scene containing the second character.<br>
4. Return to the first character's scene, then select `File -> Import -> Import Scene To Current...` and import the second scene.<br>
5. The second character's bone names will automatically receive the `character1:` prefix.<br>
6. The third character can be added in the same way.<br>

Team Gadget YouTube<br>
https://www.youtube.com/channel/UCj9OYwzMAIgYAeVkTV4wczw<br>

# Disclaimer <br>
CEU is an independent project developed by Team Gadget.<br>
Cascadeur is a trademark and/or property of Nekki.<br>
Unity is a trademark and/or property of Unity Technologies Inc.<br>
This project is not an official product of Nekki or Unity Technologies Inc., and is not endorsed by, affiliated with, sponsored by, or officially supported by either company.<br>

# 日本語 <br>
# Cascadeur Entangle for Unity (CEU) βテスト<br>
CEUは前作"Gadget Entangle for Cascadeur (GEC)"の後継バージョンになります。<br>
基本的な機能全般はGECを継承しつつ、大幅にブラッシュアップを施しました。<br>

# 開発・動作環境<br>
Windows専用 (コード内でWindowsAPIを使用)<br>
Unity6.5<br>
エディターバージョン : 6000.5.1f1<br>
レンダーパイプライン : URP<br>
CascadeurPro 2026.1.3<br>

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
5. 同期順はCascadeurが最初でUnity側が後です<br>

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
