using System;
using System.Collections.Generic;
using System.Numerics;

namespace DampNet
{
    public static partial class ByteConverters
    {
        public static Func<object, Byte[]> Vector3Converter(Func<object, object> getter)
        {
            Byte[] getBytes(Object parent)
            {
                var output = new List<Byte>();
                var obj = getter(parent);
                if (obj is Vector3 value)
                {
                    output.AddRange(BitConverter.GetBytes(value.X));
                    output.AddRange(BitConverter.GetBytes(value.Y));
                    output.AddRange(BitConverter.GetBytes(value.Z));
                    return output.ToArray();
                }
                else
                {
                    throw new InvalidCastException("Value is not vector3");
                }
            }

            return getBytes;
        }
        public static Func<object, Byte[], Int32, Int32> Vector3Converter(Action<object, object> setter)
        {
            Int32 setBytes(Object parent, Byte[] data, Int32 index)
            {
                var vec3 = new Vector3(BitConverter.ToSingle(data, index), BitConverter.ToSingle(data, index += 4), BitConverter.ToSingle(data, index += 4));
                setter(parent, vec3);
                return 12;
            }

            return setBytes;
        }
    }
}
