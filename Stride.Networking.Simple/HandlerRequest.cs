using System;
using System.IO;
using Stride.Core;
using Stride.Core.Serialization;

namespace Stride.Networking.Simple
{
    /// <summary>
    /// This class manages serialization of handler requests - it's independent of custom serializers.
    /// A HandlerRequest is basically a serialization of a typed enum.
    /// </summary>
    [DataContract]
    internal struct HandlerRequest
    {
        public int Value;
        public string TypeName;

        public byte[] Serialize()
        {
            using var stream = new MemoryStream(this.TypeName.Length + 2 * sizeof(int));
            BinarySerialization.Write(stream, Value);
            BinarySerialization.Write(stream, TypeName);
            return stream.ToArray();
        }

        public object Instantiate()
        {
            var type = Type.GetType(TypeName);
            if (type == null || !type.IsEnum)
            {
                throw new ArgumentException("Provided type couldn't be found or is not suitable for deserialization as an enum.");
            }

            return Enum.ToObject(type, Value);
        }

        public static HandlerRequest Deserialize(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            var value = BinarySerialization.Read<int>(stream);
            var typeName = BinarySerialization.Read<string>(stream);
            return new HandlerRequest
            {
                Value = value,
                TypeName = typeName,
            };
        }

        public static HandlerRequest Create<TEnum>(TEnum value) where TEnum : Enum
        {
            return new HandlerRequest
            {
                Value = Convert.ToInt32(value),
                TypeName = typeof(TEnum).AssemblyQualifiedName,
            };
        }

    }
}
