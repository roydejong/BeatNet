using System.Diagnostics.CodeAnalysis;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.MultiplayerCore;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using Serilog;
using Serilog.Core;

namespace BeatNet.Lib.Net;

public class NetPayload : INetSerializable
{
    public byte SenderId { get; set; }
    public byte ReceiverId { get; set; }
    public PacketOption PacketOptions { get; set; }

    public List<INetSerializable>? Messages;

    public NetPayload(byte senderId = 0, byte receiverId = 0, PacketOption packetOptions = PacketOption.None)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        PacketOptions = packetOptions;
        Messages = new();
    }

    public NetPayload(INetSerializable message, byte senderId = 0, byte receiverId = 0,
        PacketOption packetOptions = PacketOption.None) : this(senderId, receiverId, packetOptions)
    {
        Messages = new() { message };
    }

    public void WriteTo(ref NetWriter writer)
    {
        writer.WriteByte(SenderId);
        writer.WriteByte(ReceiverId);
        writer.WriteByte((byte)PacketOptions);

        if (Messages == null)
            throw new InvalidOperationException("Messages list is null, cannot serialize payload");
        
        foreach (var message in Messages)
            WriteNextMessage(ref writer, message);
    }

    public void WriteNextMessage(ref NetWriter writer, INetSerializable message)
    {
        var subBuffer = AcquireSubBuffer();
        var subWriter = new NetWriter(subBuffer);

        try
        {
            // Prefix the message depending on the type of serializer
            if (message is BaseCpmPacket cpmPacket)
                subWriter.WriteByte((byte)cpmPacket.InternalMessageType);
            else
                throw new InvalidOperationException("Base level messages must inherit from BaseCpmPacket");

            if (message is BaseSessionPacket sessionPacket)
            {
                subWriter.WriteByte((byte)sessionPacket.SessionMessageType);

                if (message is BaseRpc rpc)
                    subWriter.WriteByte(rpc.RpcTypeValue);
                else if (message is BaseMpcPacket mpcPacket)
                    subWriter.WriteString(mpcPacket.PacketName);
            }

            // Write message contents
            subWriter.WriteSerializable(message);

            // Determine length; write prefixed contents to main buffer
            var subLength = subWriter.Position;
            writer.WriteVarUInt((uint)subLength);
            writer.WriteBytes(subWriter.Content);
        }
        finally
        {
            ReturnSubBuffer(subBuffer);
        }
    }

    public void ReadFrom(ref NetReader reader)
    {
        SenderId = reader.ReadByte();
        ReceiverId = reader.ReadByte();
        PacketOptions = (PacketOption)reader.ReadByte();

        if (Messages == null)
            Messages = new();
        else
            Messages.Clear();

        while (!reader.EndOfData && TryReadNextMessage(ref reader, out var message))
            Messages.Add(message);
    }

    private bool TryReadNextMessage(ref NetReader reader, [NotNullWhen(true)] out INetSerializable? message)
    {
        var nextLength = (int)reader.ReadVarUInt();

        if (nextLength <= 0 || reader.RemainingLength < nextLength)
        {
            // Read would exceed buffer size   
            message = null;
            return false;
        }

        var nextSlice = reader.ReadBytes(nextLength);
        var nextReader = new NetReader(nextSlice);

        // Read prefix(es) to determine serializable layer and type
        var packetLayer = PacketLayer.ConnectedPlayerMessage;
        var packetType = nextReader.ReadByte();

        var internalMessageType = (InternalMessageType)packetType;
        string? mpcPacketName = null;

        if (internalMessageType == InternalMessageType.MultiplayerSession)
        {
            packetLayer = PacketLayer.MultiplayerSession;
            packetType = nextReader.ReadByte();

            var sessionMessageType = (SessionMessageType)packetType;

            if (sessionMessageType == SessionMessageType.MenuRpc)
            {
                packetLayer = PacketLayer.MenuRpc;
                packetType = nextReader.ReadByte();
            }
            else if (sessionMessageType == SessionMessageType.GameplayRpc)
            {
                packetLayer = PacketLayer.GameplayRpc;
                packetType = nextReader.ReadByte();
            }
            else if (sessionMessageType == SessionMessageType.MultiplayerCore)
            {
                mpcPacketName = nextReader.ReadString();
                var mpcMessageType = MpcMessageTypeExtensions.GetMpCoreMessageType(mpcPacketName);

                packetLayer = PacketLayer.MultiplayerCore;
                packetType = (byte)mpcMessageType;
            }
        }

        // Try to instantiate the message
        message = SerializableRegistry.TryInstantiate(packetLayer, packetType);
        if (message == null)
        {
            Log.Logger?.Warning("Failed to read message, not implemented ({PacketLayer}, {PacketType})",
                packetLayer, packetType);
            return false;
        }

        // Restore generic packet name for MPC
        if (mpcPacketName != null && message is GenericMpcPacket mpcPacket)
            mpcPacket.PacketNameValue = mpcPacketName;

        // Read message contents
        message.ReadFrom(ref nextReader);
        return true;
    }

    #region Static sub buffer pool

    private static RingBuffer<byte[]>? _subBufferPool;

    public static void EnsureSubBufferPoolInit()
    {
        if (_subBufferPool != null)
            return;

        _subBufferPool = new RingBuffer<byte[]>(NetConsts.SubBufferCount);

        for (var i = 0; i < NetConsts.SubBufferCount; i++)
            _subBufferPool.Enqueue(GC.AllocateArray<byte>(length: NetConsts.MaximumPacketSize, pinned: true));
    }

    public static byte[] AcquireSubBuffer()
    {
        if (_subBufferPool == null)
            throw new InvalidOperationException("Sub-buffer pool not initialized");

        return _subBufferPool.Dequeue();
    }

    public static void ReturnSubBuffer(byte[] buffer)
    {
        if (_subBufferPool == null)
            throw new InvalidOperationException("Sub-buffer pool not initialized");

        _subBufferPool.Enqueue(buffer);
    }

    #endregion
}