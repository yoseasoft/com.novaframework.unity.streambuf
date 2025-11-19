using System;

namespace DataFabricEntry.Runtime
{
    public interface ISerializable
    {
        void Serialize(DFByteArray writer);
        void DeSerialize(DFByteArray reader);
    }

    public interface IClientAPI : ISerializable
    {
    }

    public interface IServerAPI : ISerializable
    {
    }

    // 反射接口
    public interface IProtoAPI
    {
        int GetRequest(Type type);
        Type GetRequestType(int hashCode);
        Type GetResponse(int hashCode);
    }
}