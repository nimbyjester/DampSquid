using System;
using System.Collections.Generic;
using System.Numerics;

namespace DampNet
{
    public static partial class ByteConverters
    {
        public static Func<object, Byte[]> StringConverter(Func<object, object> getter)
        {
            Byte[] getBytes(Object parent)
            {
                var obj = getter(parent);
                if (obj is String value)
                {
                    var countBytes = BitConverter.GetBytes(value.Length);
                    var elementOutput = new List<Byte>();
                    for (int i = 0; i < value.Length; i++)
                    {
                        var elementBytes = BitConverter.GetBytes(value[i]);
                        elementOutput.AddRange(elementBytes);
                    }
                    var output = new List<Byte>(countBytes);
                    output.AddRange(elementOutput);
                    return output.ToArray();
                }
                else
                {
                    return BitConverter.GetBytes(0);
                }
            }

            return getBytes;
        }
        public static Func<object, Byte[], Int32, Int32> StringConverter(Action<object, object> setter)
        {
            Int32 setBytes(Object parent, Byte[] data, Int32 index)
            {
                Int32 innerIndex = index;
                var count = BitConverter.ToInt32(data, innerIndex);
                if (count == 0) return 4;
                innerIndex += 4;
                String output = String.Empty;
                for (int i = 0; i < count; i++)
                {
                    var character = BitConverter.ToChar(data, innerIndex);
                    innerIndex += 2;
                    output += character;
                }
                setter(parent, output);
                return innerIndex - index;
            }

            return setBytes;
        }
    }
}
