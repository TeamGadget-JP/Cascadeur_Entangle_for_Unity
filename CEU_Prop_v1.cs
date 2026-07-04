using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;
using System;
using System.Net;
#if UNITY_EDITOR
using UnityEditor;
using System.Diagnostics;
#endif

namespace Gadget
{
    [ExecuteAlways]
    public class CEU_Prop_v1 : MonoBehaviour
    {
        [Header("Network Settings")]
        public int targetPort = 8909;

        [Header("Prop Identity")]
        public string propID = "Weapon_01";

        [Header("Interpolation Settings")]
        public bool useInterpolation = true;
        [Range(1f, 60f)] public float lerpSpeed = 60f;

        private void Reset()
        {
            propID = gameObject.name;

            CEU_Prop_v1[] allProps = FindObjectsOfType<CEU_Prop_v1>();
            int maxPort = 8908;
            
            foreach (var p in allProps)
            {
                if (p != this && p.targetPort > maxPort)
                {
                    maxPort = p.targetPort;
                }
            }
            targetPort = maxPort + 1;
        }

        private UdpClient udpClient;
        private Thread receiveThread;
        private byte[] pendingBytes = null;
        private bool hasNewData = false;
        private readonly object lockObj = new object();

        public void StartReceiving()
        {
            StopReceiving();
            try
            {
                udpClient = new UdpClient(targetPort);
                receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                receiveThread.Start();
            }
            catch (Exception e) { UnityEngine.Debug.LogError($"[CEU Prop] Port {targetPort} Error: {e.Message}"); }
        }

        public void StopReceiving()
        {
            if (udpClient != null) { udpClient.Close(); udpClient = null; }
            if (receiveThread != null) receiveThread = null;
        }

        private void ReceiveLoop()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            while (udpClient != null)
            {
                try
                {
                    byte[] buffer = udpClient.Receive(ref ep);
                    lock (lockObj) { pendingBytes = buffer; hasNewData = true; }
                }
                catch { }
            }
        }

        private void ProcessLatestData()
        {
            byte[] dataToProcess = null;
            lock (lockObj)
            {
                if (hasNewData) { dataToProcess = pendingBytes; hasNewData = false; }
            }

            if (dataToProcess == null) return;

            try
            {
                using (MemoryStream ms = new MemoryStream(dataToProcess))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    if (br.ReadByte() != 'C' || br.ReadByte() != 'E' || br.ReadByte() != 'P') return;

                    byte flags = br.ReadByte();
                    bool hasPos = (flags & 2) != 0;
                    bool hasRot = (flags & 1) != 0;

                    if (hasPos)
                    {
                        float px = br.ReadSingle();
                        float py = br.ReadSingle();
                        float pz = br.ReadSingle();
                        
                        Vector3 targetPos = new Vector3(px * -1f, py, pz);
                        if (useInterpolation) transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed / 60f);
                        else transform.position = targetPos;
                    }

                    if (hasRot)
                    {
                        float rx = br.ReadSingle();
                        float ry = br.ReadSingle();
                        float rz = br.ReadSingle();
                        float rw = br.ReadSingle();

                        Quaternion targetRot = new Quaternion(rx, ry * -1f, rz * -1f, rw);
                        if (useInterpolation) transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lerpSpeed / 60f);
                        else transform.rotation = targetRot;
                    }
                }
                
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
                    foreach (var mr in mrs) { mr.enabled = false; mr.enabled = true; }
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
#endif
            }
            catch { }
        }

#if UNITY_EDITOR
        private void OnEnable() { EditorApplication.update += EditorUpdate; }
        private void OnDisable() { EditorApplication.update -= EditorUpdate; StopReceiving(); }
        private void EditorUpdate() 
        { 
            if (!Application.isPlaying) 
            {
                ProcessLatestData(); 
                if (udpClient != null) EditorApplication.QueuePlayerLoopUpdate();
            }
        }
#else
        private void OnDisable() { StopReceiving(); }
#endif
        private void LateUpdate() { if (Application.isPlaying) ProcessLatestData(); }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CEU_Prop_v1))]
    public class CEU_PropEditor_v1 : Editor
    {
        public enum PropExportMode { EquipLocal, EnvironmentWorld }
        private PropExportMode exportMode = PropExportMode.EquipLocal;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            CEU_Prop_v1 script = (CEU_Prop_v1)target;

            GUILayout.Space(15);
            GUIStyle propTitleStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = new Color(1.0f, 0.7f, 0.0f) } };
            EditorGUILayout.LabelField("Prop Smuggler (Direct Export)", propTitleStyle);
            
            exportMode = (PropExportMode)EditorGUILayout.EnumPopup("Export Mode", exportMode);

            string tempPath = $"C:/Temp/{script.propID}.obj";
            EditorGUILayout.LabelField("Export Path", tempPath, EditorStyles.wordWrappedMiniLabel);

            GUILayout.Space(5);
            GUI.backgroundColor = new Color(1.0f, 0.8f, 0.0f);
            if (GUILayout.Button("Export OBJ & Command Cascadeur", GUILayout.Height(35)))
            {
                ExecutePropTransfer(script, tempPath);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);
            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f);
            if (GUILayout.Button("Open Temp Folder", GUILayout.Height(20)))
            {
                if (!Directory.Exists("C:/Temp/")) Directory.CreateDirectory("C:/Temp/");
                Process.Start("explorer.exe", "C:\\Temp\\");
            }
            GUI.backgroundColor = Color.white;
        }

        private void ExecutePropTransfer(CEU_Prop_v1 prop, string path)
        {
            if (string.IsNullOrEmpty(prop.propID))
            {
                EditorUtility.DisplayDialog("Error", "Prop ID is empty!", "OK");
                return;
            }

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (ExportPropToOBJ(prop.gameObject, path, exportMode))
            {
                Vector3 sendPos = (exportMode == PropExportMode.EnvironmentWorld) ? prop.transform.position : Vector3.zero;
                Quaternion sendRot = (exportMode == PropExportMode.EnvironmentWorld) ? prop.transform.rotation : Quaternion.identity;

                string json = $@"{{
                    ""command"": ""IMPORT_PROP"",
                    ""prop_id"": ""{prop.propID}"",
                    ""target_port"": {prop.targetPort},
                    ""filepath"": ""{path.Replace("\\", "/")}"",
                    ""gp"": [{-sendPos.x}, {sendPos.y}, {sendPos.z}],
                    ""gr"": [{sendRot.x}, {-sendRot.y}, {-sendRot.z}, {sendRot.w}]
                }}";

                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    using (UdpClient sender = new UdpClient()) sender.Send(bytes, bytes.Length, "127.0.0.1", 8900);
                    
                    prop.StartReceiving();
                    UnityEngine.Debug.Log($"<color=#00FF00>[CEU Prop Smuggler]</color> Successfully smuggled [{prop.propID}] to Cascadeur!");
                }
                catch (Exception e) { UnityEngine.Debug.LogError($"[CEU Prop] UDP Error: {e.Message}"); }
            }
        }

        private bool ExportPropToOBJ(GameObject prop, string path, PropExportMode mode)
        {
            try
            {
                MeshFilter[] meshFilters = prop.GetComponentsInChildren<MeshFilter>();
                if (meshFilters.Length == 0) return false;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# Gadget Entangle Prop Smuggler (CEU Sync Export)");
                sb.AppendLine($"o {prop.name}");

                int vertexOffset = 0;
                foreach (MeshFilter mf in meshFilters)
                {
                    Mesh m = mf.sharedMesh;
                    if (m == null) continue;

                    foreach (Vector3 v in m.vertices)
                    {
                        Vector3 wPos = mf.transform.TransformPoint(v);
                        Vector3 finalPos = (mode == PropExportMode.EquipLocal) ? prop.transform.InverseTransformPoint(wPos) : wPos;
                        finalPos *= 100f; 
                        sb.AppendLine($"v {-finalPos.x} {finalPos.y} {finalPos.z}"); 
                    }
                    
                    foreach (Vector3 vn in m.normals)
                    {
                        Vector3 wNorm = mf.transform.TransformDirection(vn);
                        Vector3 finalNorm = (mode == PropExportMode.EquipLocal) ? prop.transform.InverseTransformDirection(wNorm) : wNorm;
                        sb.AppendLine($"vn {-finalNorm.x} {finalNorm.y} {finalNorm.z}");
                    }
                    
                    foreach (Vector2 uv in m.uv) sb.AppendLine($"vt {uv.x} {uv.y}");

                    for (int material = 0; material < m.subMeshCount; material++)
                    {
                        int[] triangles = m.GetTriangles(material);
                        for (int i = 0; i < triangles.Length; i += 3)
                        {
                            int i1 = triangles[i] + 1 + vertexOffset;
                            int i2 = triangles[i + 2] + 1 + vertexOffset;
                            int i3 = triangles[i + 1] + 1 + vertexOffset;
                            sb.AppendLine($"f {i1}/{i1}/{i1} {i2}/{i2}/{i2} {i3}/{i3}/{i3}");
                        }
                    }
                    vertexOffset += m.vertices.Length;
                }
                File.WriteAllText(path, sb.ToString());
                return true;
            }
            catch (Exception e) { UnityEngine.Debug.LogError($"[CEU Prop] OBJ Export Failed: {e.Message}"); return false; }
        }
    }
#endif
}