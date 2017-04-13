using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters;

namespace OgameBot.Utilities
{
    public static class SerializerHelper
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
        };
        private static readonly Encoding Encoding = Encoding.UTF8;

        public static void SerializeToStream<T>(T obj, Stream target, bool compress = false)
        {
            Stream pseudoTarget = target;

            if (compress)
                pseudoTarget = new GZipStream(pseudoTarget, CompressionLevel.Optimal);

            using (StreamWriter sw = new StreamWriter(pseudoTarget, Encoding))
                Serializer.Serialize(sw, obj);
        }

        public static byte[] SerializeToBytes<T>(T obj, bool compress = false)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                SerializeToStream(obj, ms, compress);
                return ms.ToArray();
            }
        }

        public static string SerializeToString<T>(T obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                Serializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }

        public static T DeserializeFromString<T>(string obj)
        {
            return (T)DeserializeFromString(typeof(T), obj);
        }

        public static object DeserializeFromString(Type type, string obj)
        {
            using (StringReader sr = new StringReader(obj))
                return Serializer.Deserialize(sr, type);
        }

        public static T DeserializeFromStream<T>(Stream source, bool decompress = false)
        {
            return (T)DeserializeFromStream(typeof(T), source, decompress);
        }

        private static object DeserializeFromStream(Type type, Stream source, bool decompress = false)
        {
            Stream pseudoSource = source;
            if (decompress)
                pseudoSource = new GZipStream(pseudoSource, CompressionMode.Decompress);

            using (StreamReader sr = new StreamReader(pseudoSource, Encoding))
            using (JsonTextReader jr = new JsonTextReader(sr))
                return Serializer.Deserialize(jr, type);
        }

        public static object DeserializeFromBytes(Type type, byte[] data, bool? decompress = false)
        {
            using (MemoryStream ms = new MemoryStream(data))
                return DeserializeFromStream(type, ms, decompress == null ? IsCompressed(data) : decompress.Value);
        }

        public static T DeserializeFromBytes<T>(byte[] data, bool decompress = false)
        {
            using (MemoryStream ms = new MemoryStream(data))
                return (T)DeserializeFromStream(typeof(T), ms, decompress);
        }

        // gzip header 0x1F8B
        public static bool IsCompressed(byte[] data)
        {
            return data.Length >= 2 && data[0] == 31 && data[1] == 139;
        }
    }
}