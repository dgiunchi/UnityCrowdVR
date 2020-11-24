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

    public OvrAvatar LocalAvatar;
    public OvrAvatar LoopbackAvatar;

    private int PacketSequence = 0;

    LinkedList<Packet> packetQueue = new LinkedList<Packet>();

    public bool record = false;
    bool _lastRecord = false;
    public bool playback = false;
    string path;
    private string fileName = "avatar.avs";
    LinkedList<Packet> _recordedQueue = new LinkedList<Packet>();


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
        }
    }

    void OnLocalAvatarPacketRecorded(object sender, OvrAvatar.PacketEventArgs args)
    {
        if (playback)
        {
            LinkedListNode<Packet> packet = _recordedQueue.First;
            if (packet == null)
            {
                ReadFile();
                packet = _recordedQueue.First;
            }
            SendPacketData(packet.Value.PacketData);
            _recordedQueue.RemoveFirst();

        }

        else using (MemoryStream outputStream = new MemoryStream())
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
            _lastRecord = record;
        }

        if (packetQueue.Count > 0)
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
    }

    void SendPacketData(byte[] data)
    {
        Packet packet = new Packet();
        packet.PacketData = data;

        packetQueue.AddLast(packet);
        if (record && !playback) _recordedQueue.AddLast(packet);
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

    void WriteToFile()
    {
        using (Stream stream = File.Open(Path.Combine(path, fileName), FileMode.Create))
        {
            new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, new PacketsFile { packetList = _recordedQueue });
        }
        Debug.Log("File written");
    }

    void ReadFile()
    {
        using (Stream stream = File.Open(Path.Combine(path, fileName), FileMode.Open))
        {
            _recordedQueue = (new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream) as PacketsFile).packetList;
        }
        Debug.Log("File read");
    }
}