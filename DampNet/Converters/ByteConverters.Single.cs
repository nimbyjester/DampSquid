using System;
using System.Collections.Generic;
using System.Numerics;

namespace DampNet
{
    public static partial class ByteConverters
    {
        public static Func<object, Byte[]> SingleConverter(Func<object, object> getter)
        {
            Byte[] getBytes(Object parent)
            {
                var obj = getter(parent);
                if (obj is Single value)
                {
                    return BitConverter.GetBytes(value);
                }
                else
                {
                    return BitConverter.GetBytes(0);
                }
            }

            return getBytes;
        }
        public static Func<object, Byte[], Int32, Int32> SingleConverter(Action<object, object> setter)
        {
            Int32 setBytes(Object parent, Byte[] data, Int32 index)
            {
                setter(parent, BitConverter.ToSingle(data, index));
                return 4;
            }

            return setBytes;
        }
    }
}
