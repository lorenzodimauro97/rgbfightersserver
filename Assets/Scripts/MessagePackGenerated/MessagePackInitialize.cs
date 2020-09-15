using MessagePack;
using MessagePack.Resolvers;
using UnityEditor;
using UnityEngine;

namespace MessagePackGenerated
{
    public static class MessagePackInitialize
    {
        private static bool _serializerRegistered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Debug.Log("Initialized");
            if (_serializerRegistered) return;
            StaticCompositeResolver.Instance.Register(
                GeneratedResolver.Instance,
                StandardResolver.Instance
            );

            var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);

            MessagePackSerializer.DefaultOptions = option;
            _serializerRegistered = true;
        }

#if UNITY_EDITOR


        [InitializeOnLoadMethod]
        private static void EditorInitialize()
        {
            Initialize();
        }

#endif
    }
}