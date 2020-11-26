using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Oculus.Avatar;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class SerializeAvatarManager : MonoBehaviour
{
    [Serializable]
    class PacketsFile
    {
        public LinkedList<Packet> packetList;
    };

    [Serializable]
    class Packet
    {
        public byte[] PacketData;
    };

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    class TimedPosition
    {
        public float time;
        public float x, y, z;
        public float qx,qy,qz,qw;
    };

    public OvrAvatar LocalAvatar;
    public OvrAvatar LoopbackAvatar;
    public GameObject ovrPlayer;

    private int PacketSequence = 0;

    LinkedList<Packet> packetQueue = new LinkedList<Packet>();
    LinkedList<Packet> timedPositionQueue = new LinkedList<Packet>();

    public bool record = false;
    bool _lastRecord = false;
    public bool playback = false;
    string path;
    private string fileName = "avatar.avs";
    private string fileNameTP= "avatartp.avs";
    LinkedList<Packet> _recordedQueue = new LinkedList<Packet>();
    LinkedList<Packet> _recordedTimedPositionQueue = new LinkedList<Packet>();


    void Start()
    {
        //path = Application.streamingAssetsPath;
        path = Application.persistentDataPath;

        LocalAvatar.RecordPackets = true;
        LocalAvatar.PacketRecorded += OnLocalAvatarPacketRecorded;
        _lastRecord = record;
        if (playback)
        {
            ReadFile();
            ReadFileTimedPosition();
        }
    }

    byte[] getBytes(TimedPosition str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    TimedPosition fromBytes(byte[] arr)
    {
        TimedPosition str = new TimedPosition();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (TimedPosition)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }

    void OnLocalAvatarPacketRecorded(object sender, OvrAvatar.PacketEventArgs args)
    {
        if (playback)
        {
            LinkedListNode<Packet> packet = _recordedQueue.First;
            LinkedListNode<Packet> timedPositionPacket = _recordedTimedPositionQueue.First;
            if (packet == null)
            {
                ReadFile();
                packet = _recordedQueue.First;
            }
            SendPacketData(packet.Value.PacketData);
            _recordedQueue.RemoveFirst();

            if (timedPositionPacket == null)
            {
                ReadFileTimedPosition();
                timedPositionPacket = _recordedTimedPositionQueue.First;
            }
            SendTimedPositionData(timedPositionPacket.Value.PacketData);
            _recordedTimedPositionQueue.RemoveFirst();

        } else {
            using (MemoryStream outputStream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(outputStream);

                var size = CAPI.ovrAvatarPacket_GetSize(args.Packet.ovrNativePacket);
                byte[] data = new byte[size];
                CAPI.ovrAvatarPacket_Write(args.Packet.ovrNativePacket, size, data);

                writer.Write(PacketSequence++);
                writer.Write(size);
                writer.Write(data);

                SendPacketData(outputStream.ToArray());
            }
            using (MemoryStream outputStream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(outputStream);

                TimedPosition value = new TimedPosition();
                var size = System.Runtime.InteropServices.Marshal.SizeOf(value);
                byte[] data = new byte[size];
                
                value.time = Time.unscaledTime;  // need to check fo the 0
                value.x = ovrPlayer.transform.localPosition.x;
                value.y = ovrPlayer.transform.localPosition.y;
                value.z = ovrPlayer.transform.localPosition.z;
                value.qx = ovrPlayer.transform.localRotation.x;
                value.qy = ovrPlayer.transform.localRotation.y;
                value.qz = ovrPlayer.transform.localRotation.z;
                value.qw = ovrPlayer.transform.localRotation.w;
                data = getBytes(value);

                writer.Write(data);

                SendTimedPositionData(outputStream.ToArray());
            }
        }
    }

    void Update()
    {
        if(OVRInput.Get(OVRInput.Button.Four))
        {
            record = false;
        }
        if (!record && _lastRecord)
        {
            WriteToFile();
            WriteToFileTimedPosition();
            _lastRecord = record;
        }

        if (packetQueue.Count > 0) //queues synced
        {
            List<Packet> deadList = new List<Packet>();
            foreach (Packet packet in packetQueue)
            {
                ReceivePacketData(packet.PacketData);
                deadList.Add(packet); //was Add
            }

            foreach (var packet in deadList)
            {
                packetQueue.Remove(packet);
            }

        }

        if (timedPositionQueue.Count > 0) //queues synced
        {
            List<Packet> tpdeadList = new List<Packet>();
            foreach (Packet packet in timedPositionQueue)
            {
                ReceiveTimedPositionData(packet.PacketData);
                tpdeadList.Add(packet); //was Add
            }

            foreach (var packet in tpdeadList)
            {
                timedPositionQueue.Remove(packet);
            }

        }
    }

    void SendPacketData(byte[] data)
    {
        Packet packet = new Packet();
        packet.PacketData = data;

        packetQueue.AddLast(packet);
        if (record && !playback) _recordedQueue.AddLast(packet);
    }

    void SendTimedPositionData(byte[] data)
    {
        Packet packet = new Packet();
        packet.PacketData = data;

        timedPositionQueue.AddLast(packet);
        if (record && !playback) _recordedTimedPositionQueue.AddLast(packet);
    }

    void ReceivePacketData(byte[] data)
    {
        using (MemoryStream inputStream = new MemoryStream(data))
        {
            BinaryReader reader = new BinaryReader(inputStream);
            int sequence = reader.ReadInt32();

            int size = reader.ReadInt32();
            byte[] sdkData = reader.ReadBytes(size);

            IntPtr packet = CAPI.ovrAvatarPacket_Read((UInt32)data.Length, sdkData);
            LoopbackAvatar.GetComponent<OvrAvatarRemoteDriver>().QueuePacket(sequence, new OvrAvatarPacket { ovrNativePacket = packet });
        }
    }

    void ReceiveTimedPositionData(byte[] data)
    {
        using (MemoryStream inputStream = new MemoryStream(data))
        {
            BinaryReader reader = new BinaryReader(inputStream);
            var size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(TimedPosition));
            byte[] sdkData = reader.ReadBytes(size);


            TimedPosition value = fromBytes(data);
            LoopbackAvatar.transform.parent.position = new Vector3(value.x, value.y, value.z);
            LoopbackAvatar.transform.parent.rotation = new Quaternion(value.qx, value.qy, value.qz, value.qw);
        }
    }

    void WriteToFile()
    {
        using (Stream stream = File.Open(Path.Combine(path, fileName), FileMode.Create))
        {
            new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, new PacketsFile { packetList = _recordedQueue });
        }
        Debug.Log("File written");
    }

    void WriteToFileTimedPosition()
    {
        using (Stream stream = File.Open(Path.Combine(path, fileNameTP), FileMode.Create))
        {
            new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, new PacketsFile { packetList = _recordedTimedPositionQueue });
        }
        Debug.Log("File written tp");
    }

    void ReadFile()
    {
        using (Stream stream = File.Open(Path.Combine(path, fileName), FileMode.Open))
        {
            _recordedQueue = (new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream) as PacketsFile).packetList;
        }
        Debug.Log("File read");
    }

    void ReadFileTimedPosition()
    {
        using (Stream stream = File.Open(Path.Combine(path, fileNameTP), FileMode.Open))
        {
            _recordedTimedPositionQueue = (new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream) as PacketsFile).packetList;
        }
        Debug.Log("File read tp");
    }
    
}