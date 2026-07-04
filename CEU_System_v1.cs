using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Gadget
{
    [ExecuteAlways]
    public class CEU_System_v1 : MonoBehaviour
    {
        [Header("System Control (Port: 8900)")]
        [Tooltip("Check this to start Live Link with Cascadeur.")]
        public bool connectToCascadeur = false;
        
        private bool wasConnected = false;

        void Update()
        {
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
            
            CEU_Avatar_v1[] avatars = FindObjectsOfType<CEU_Avatar_v1>();
            List<string> charJsons = new List<string>();
            foreach (var avatar in avatars)
            {
                if (!avatar.isActiveAndEnabled) continue;
                avatar.StartReceiving();
                string requestBones = avatar.GenerateRequestBonesJson();
                charJsons.Add($"{{\"target_port\": {avatar.targetPort}, \"request_bones\": {requestBones}}}");
            }

            CEU_Prop_v1[] props = FindObjectsOfType<CEU_Prop_v1>();
            List<string> propJsons = new List<string>();
            foreach (var prop in props)
            {
                if (!prop.isActiveAndEnabled) continue;
                prop.StartReceiving();
                propJsons.Add($"{{\"prop_id\": \"{prop.propID}\", \"target_port\": {prop.targetPort}}}");
            }

            string fullJson = $"{{\"command\": \"START\", \"characters\": [{string.Join(", ", charJsons)}], \"props\": [{string.Join(", ", propJsons)}]}}";
            SendUdpPacket(fullJson);
            
            Debug.Log("<color=#00FF00>[CEU System]</color> Handshake (START) sent to Cascadeur.");
        }

        private void SendStopCommand()
        {
            Application.runInBackground = false;
            
            CEU_Avatar_v1[] avatars = FindObjectsOfType<CEU_Avatar_v1>();
            foreach (var avatar in avatars) avatar.StopReceiving();

            CEU_Prop_v1[] props = FindObjectsOfType<CEU_Prop_v1>();
            foreach (var prop in props) prop.StopReceiving();

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