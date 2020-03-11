using System;
using System.Linq;
using System.Numerics;

namespace DampNet
{
    [Flags]
    public enum SerializerMode: Byte
    {
        None = 0b_0000,
        TCP = 0b_0001,
        UDP = 0b_0010,
        ALL = 0b_1111
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class DampData : Attribute
    {
        public SerializerMode SerializeMode { get; set; }
        public DampData(SerializerMode serializeMode = SerializerMode.None)
        {
            SerializeMode = serializeMode;
        }
    }
}
