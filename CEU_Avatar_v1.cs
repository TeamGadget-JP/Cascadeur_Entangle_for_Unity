using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Net;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gadget
{
    [ExecuteAlways]
    public class CEU_Avatar_v1 : MonoBehaviour
    {
        public enum RigType { Humanoid, Generic }
        
        public enum AxisMap { X, Y, Z, InvertX, InvertY, InvertZ }

        [Header("Network Settings")]
        public int targetPort = 8901;

        [Header("Avatar Settings")]
        public string cascadeurPrefix = "character1:";
        public RigType rigType = RigType.Humanoid;

        [Header("Root Motion Settings")]
        public bool syncRootMotion = false;
        public string rootBoneName = "CC_Base_BoneRoot";
        public Vector3 rootRotationOffset = new Vector3(90f, 0f, 0f); 
        
        [Header("Root Swizzle (Advanced)")]
        [Tooltip("Map Cascadeur axes to Unity Root Position axes.")]
        public AxisMap rootPosX = AxisMap.InvertX;
        public AxisMap rootPosY = AxisMap.Y;
        public AxisMap rootPosZ = AxisMap.Z;

        [Tooltip("Map Cascadeur axes to Unity Root Rotation axes.")]
        public AxisMap rootRotX = AxisMap.X;
        public AxisMap rootRotY = AxisMap.InvertZ; 
        public AxisMap rootRotZ = AxisMap.Y;       

        [Header("Interpolation")]
        public bool useInterpolation = true;
        [Range(1f, 60f)] public float lerpSpeed = 60f;
        [Range(1f, 60f)] public float footLerpSpeed = 60f;

        [Header("Hybrid Settings")]
        public bool enableICloneHybrid = false;

        private Dictionary<byte, Transform> boneMap = new Dictionary<byte, Transform>();
        
        private SkinnedMeshRenderer[] cachedSMRs; 

        private UdpClient udpClient;
        private Thread receiveThread;
        private byte[] pendingBytes = null;
        private bool hasNewData = false;
        private readonly object lockObj = new object();

        public string GenerateRequestBonesJson()
        {
            boneMap.Clear();
            
            cachedSMRs = GetComponentsInChildren<SkinnedMeshRenderer>(true);

            List<string> entries = new List<string>();
            string pfx = string.IsNullOrEmpty(cascadeurPrefix) ? "" : cascadeurPrefix.Trim();

            if (syncRootMotion)
            {
                byte rootId = 255;
                boneMap[rootId] = this.transform; 
                entries.Add($"\"{rootId}\": \"{pfx}{rootBoneName}\"");
            }

            if (rigType == RigType.Humanoid)
            {
                Animator anim = GetComponent<Animator>();
                if (anim != null)
                {
                    foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
                    {
                        if (bone == HumanBodyBones.LastBone) continue;
                        Transform t = anim.GetBoneTransform(bone);
                        if (t != null)
                        {
                            byte id = (byte)bone; 
                            boneMap[id] = t;
                            entries.Add($"\"{id}\": \"{pfx}{t.name}\"");
                        }
                    }
                }
            }
            else 
            {
                Transform[] allTransforms = GetComponentsInChildren<Transform>();
                byte id = 0;
                foreach (Transform t in allTransforms)
                {
                    if (id >= 254) break; 
                    boneMap[id] = t;
                    entries.Add($"\"{id}\": \"{pfx}{t.name}\"");
                    id++;
                }
            }

            return "{" + string.Join(", ", entries) + "}";
        }

        public void StartReceiving()
        {
            StopReceiving();
            try
            {
                udpClient = new UdpClient(targetPort);
                receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                receiveThread.Start();
            }
            catch (System.Exception e) { Debug.LogError($"[CEU Avatar] Port {targetPort} Error: {e.Message}"); }
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

        private float GetMappedAxis(AxisMap map, float x, float y, float z)
        {
            switch (map)
            {
                case AxisMap.X: return x;
                case AxisMap.Y: return y;
                case AxisMap.Z: return z;
                case AxisMap.InvertX: return -x;
                case AxisMap.InvertY: return -y;
                case AxisMap.InvertZ: return -z;
                default: return 0f;
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
                    if (br.ReadByte() != 'C' || br.ReadByte() != 'E' || br.ReadByte() != 'U') return;

                    int numBones = br.ReadByte();
                    for (int i = 0; i < numBones; i++)
                    {
                        byte b_id = br.ReadByte();
                        byte flags = br.ReadByte();
                        bool hasPos = (flags & 2) != 0;
                        bool hasRot = (flags & 1) != 0;

                        if (!boneMap.TryGetValue(b_id, out Transform t) || t == null)
                        {
                            if (hasPos) br.ReadBytes(12);
                            if (hasRot) br.ReadBytes(16);
                            continue;
                        }

                        bool isFoot = false;
                        bool isHybridSkip = false;

                        if (rigType == RigType.Humanoid)
                        {
                            isFoot = (b_id >= 17 && b_id <= 20);
                            isHybridSkip = enableICloneHybrid && (b_id == 9 || b_id == 10);
                        }
                        else
                        {
                            string n = t.name.ToLower();
                            isFoot = n.Contains("foot") || n.Contains("toe");
                            isHybridSkip = enableICloneHybrid && (n.Contains("neck") || n.Contains("head"));
                        }

                        float currentLerp = isFoot ? footLerpSpeed : lerpSpeed;

                        if (hasPos)
                        {
                            float px = br.ReadSingle();
                            float py = br.ReadSingle();
                            float pz = br.ReadSingle();

                            Vector3 targetPos;
                            
                            if (b_id == 255) 
                            {
                                targetPos = new Vector3(
                                    GetMappedAxis(rootPosX, px, py, pz),
                                    GetMappedAxis(rootPosY, px, py, pz),
                                    GetMappedAxis(rootPosZ, px, py, pz)
                                );
                            }
                            else 
                            {
                                targetPos = new Vector3(px * -1f, py, pz);
                            }

                            if (!isHybridSkip && (b_id == 0 || b_id == 255 || rigType == RigType.Generic))
                            {    
                                if (useInterpolation) t.localPosition = Vector3.Lerp(t.localPosition, targetPos, currentLerp / 60f);
                                else t.localPosition = targetPos;
                            }
                        }

                        if (hasRot)
                        {
                            float rx = br.ReadSingle();
                            float ry = br.ReadSingle();
                            float rz = br.ReadSingle();
                            float rw = br.ReadSingle();

                            Quaternion targetRot;
                            
                            if (b_id == 255) 
                            {
                                targetRot = new Quaternion(
                                    GetMappedAxis(rootRotX, rx, ry, rz),
                                    GetMappedAxis(rootRotY, rx, ry, rz),
                                    GetMappedAxis(rootRotZ, rx, ry, rz),
                                    rw
                                );
                            }
                            else 
                            {
                                targetRot = new Quaternion(rx, ry * -1f, rz * -1f, rw);
                            }
                            
                            if (b_id == 255)
                            {
                                targetRot = Quaternion.Euler(rootRotationOffset) * targetRot;
                            }

                            if (!isHybridSkip)
                            {
                                if (useInterpolation) t.localRotation = Quaternion.Slerp(t.localRotation, targetRot, currentLerp / 60f);
                                else t.localRotation = targetRot;
                            }
                        }
                    }
                }
                
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (cachedSMRs != null)
                    {
                        foreach (var smr in cachedSMRs) 
                        { 
                            if (smr != null)
                            {
                                smr.enabled = false; 
                                smr.enabled = true; 
                            }
                        }
                    }
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
}