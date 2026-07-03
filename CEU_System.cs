using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Gadget
{
    [ExecuteAlways]
    public class CEU_System : MonoBehaviour
    {
        [Header("System Control (Port: 8900)")]
        [Tooltip("Check this to start Live Link with Cascadeur.")]
        public bool connectToCascadeur = false;
        
        private bool wasConnected = false;

        void Update()
        {
            // チェックボックスの切り替えを検知してコマンドを送信
            if (connectToCascadeur != wasConnected)
            {
                if (connectToCascadeur) SendStartCommand();
                else SendStopCommand();
                
                wasConnected = connectToCascadeur;
            }
        }

        private void SendStartCommand()
        {
            Application.runInBackground = true;
            CEU_Avatar[] avatars = FindObjectsOfType<CEU_Avatar>();
            List<string> charJsons = new List<string>();

            foreach (var avatar in avatars)
            {
                if (!avatar.isActiveAndEnabled) continue;
                
                // 各キャラの受信ポートを開かせる
                avatar.StartReceiving();
                
                // 動的ハンドシェイク用のJSON文字列を取得
                string requestBones = avatar.GenerateRequestBonesJson();
                string charJson = $"{{\"target_port\": {avatar.targetPort}, \"request_bones\": {requestBones}}}";
                charJsons.Add(charJson);
            }

            // JSON組み立て (UnityのJsonUtilityの辞書シリアライズ制限を回避するため手動構築)
            string fullJson = $"{{\"command\": \"START\", \"characters\": [{string.Join(", ", charJsons)}]}}";
            SendUdpPacket(fullJson);
            
            Debug.Log("<color=#00FF00>[CEU System]</color> Handshake (START) sent to Cascadeur.");
        }

        private void SendStopCommand()
        {
            Application.runInBackground = false;
            CEU_Avatar[] avatars = FindObjectsOfType<CEU_Avatar>();
            foreach (var avatar in avatars) avatar.StopReceiving();

            string json = "{\"command\": \"STOP\"}";
            SendUdpPacket(json);
            
            Debug.Log("<color=#FF4444>[CEU System]</color> STOP command sent.");
        }

        private void SendUdpPacket(string jsonPayload)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(jsonPayload);
                using (UdpClient sender = new UdpClient())
                {
                    // Cascadeur側のマスターポート(8900)へ送信
                    sender.Send(bytes, bytes.Length, "127.0.0.1", 8900);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CEU System] Failed to send command: {e.Message}");
            }
        }
    }
}