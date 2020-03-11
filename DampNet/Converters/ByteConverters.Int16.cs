using System;
using System.Collections.Generic;
using System.Numerics;

namespace DampNet
{
    public static partial class ByteConverters
    {
        public static Func<object, Byte[]> Int16Converter(Func<object, object> getter)
        {
            Byte[] getBytes(Object parent)
            {
                var obj = getter(parent);
                if (obj is Int16 value)
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
        public static Func<object, Byte[], Int32, Int32> Int16Converter(Action<object, object> setter)
        {
            Int32 setBytes(Object parent, Byte[] data, Int32 index)
            {
                setter(parent, BitConverter.ToInt16(data, index));
                return 2;
            }

            return setBytes;
        }
    }
}
