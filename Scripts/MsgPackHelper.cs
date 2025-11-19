
namespace DataFabricEntry.Runtime
{
    public static class MsgPackHelper
    {
        private static IProtoAPI _protoApi;

        public static IProtoAPI ProtoApi => _protoApi;

        static MsgPackHelper()
        {
        }

        public static byte[] Serialize<T>(T obj)
        {
            return null;
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            return default(T);
        }

        // 注册反射接口
        public static void RegisterProtoApi(IProtoAPI protoApi)
        {
            _protoApi = protoApi;
        }
    }
}
