using System.Reflection;
using System.Runtime.CompilerServices;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.MultiplayerCore;
using BeatNet.Lib.Net.Interfaces;

namespace BeatNet.Lib;

public static class SerializableRegistry
{
    private static readonly Dictionary<PacketLayer, Dictionary<byte, INetSerializable>> TypeRegistry;

    static SerializableRegistry()
    {
        TypeRegistry = new();

        RegisterAllFromAssembly(Assembly.GetExecutingAssembly());
    }

    public static void NoopCallForStaticInit()
    {
    }

    public static void Register(INetSerializable serializable)
    {
        switch (serializable)
        {
            case BaseMenuRpc rpc:
                RegisterInternal(PacketLayer.MenuRpc, rpc.RpcTypeValue, serializable);
                break;
            case BaseGameplayRpc rpc:
                RegisterInternal(PacketLayer.GameplayRpc, rpc.RpcTypeValue, serializable);
                break;
            case BaseMpcPacket msg:
                RegisterInternal(PacketLayer.MultiplayerCore, (byte)msg.MpcMessageType, serializable);
                break;
            case BaseSessionPacket msg:
                RegisterInternal(PacketLayer.MultiplayerSession, (byte)msg.SessionMessageType, serializable);
                break;
            case BaseCpmPacket msg:
                RegisterInternal(PacketLayer.ConnectedPlayerMessage, (byte)msg.InternalMessageType, serializable);
                break;
            default:
                // We don't know what layer/opcode to assign to this serializable
                throw new NotImplementedException($"Registry does not know how to auto-register serializable " +
                                                  $"of type {serializable.GetType().Name}");
        }
    }

    private static void RegisterInternal(PacketLayer packetLayer, byte opcode, INetSerializable serializable)
    {
        if (!TypeRegistry.ContainsKey(packetLayer))
            TypeRegistry[packetLayer] = new();

        if (TypeRegistry[packetLayer].ContainsKey(opcode))
            throw new ArgumentException($"Serializable is already registered (Layer={packetLayer}, " +
                                        $"Opcode={opcode}, Type={serializable.GetType().Name})");

        TypeRegistry[packetLayer].Add(opcode, serializable);
    }

    public static void RegisterAllFromAssembly(Assembly assembly) =>
        RegisterSubclassTypesFromAssembly(typeof(BaseCpmPacket), assembly);

    private static void RegisterSubclassTypesFromAssembly(Type baseType, Assembly assembly)
    {
        var exportedPacketTypes = assembly
            .GetExportedTypes()
            .Where(type => type.IsAssignableTo(baseType) && !type.IsAbstract)
            .ToList();

        foreach (var serializableType in exportedPacketTypes)
        {
            var instance = RuntimeHelpers.GetUninitializedObject(serializableType) as INetSerializable;
            if (instance is null)
                return;

            Register(instance);
        }
    }

    public static INetSerializable? TryInstantiate(PacketLayer packetLayer, byte opcode)
    {
        if (TypeRegistry.ContainsKey(packetLayer) && TypeRegistry[packetLayer].ContainsKey(opcode))
            return RuntimeHelpers.GetUninitializedObject(TypeRegistry[packetLayer][opcode].GetType()) as INetSerializable;

        return null;
    }
}

public enum PacketLayer : byte
{
    ConnectedPlayerMessage,
    MultiplayerSession,
    MenuRpc,
    GameplayRpc,
    MultiplayerCore
}