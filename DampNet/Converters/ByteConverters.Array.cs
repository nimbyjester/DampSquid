using System;
using System.Collections.Generic;
using System.Numerics;

namespace DampNet
{
    public static partial class ByteConverters
    {
        public static Func<object, Byte[]> ArrayConverter(Func<object, object> getter, Func<Func<object, object>, Func<object, Byte[]>> elementGetter)
        {
            
            Byte[] getBytes(Object parent)
            {
                Array obj = (Array) getter(parent);
                var elementOutput = new List<Byte>();
                for (int i = 0; i < obj.Length; i++)
                {
                    Object get(Object arry)
                    {
                        return ((Array)arry).GetValue(i);
                    }
                    var elementBytes = elementGetter(get)(obj);
                    elementOutput.AddRange(elementBytes);
                }
                var countBytes = BitConverter.GetBytes(obj.Length);
                var output = new List<Byte>(countBytes);
                output.AddRange(elementOutput);
                return output.ToArray();
            }

            return getBytes;
        }
        public static Func<object, Byte[], Int32, Int32> ArrayConverter(Action<object, object> setter, Func<Action<object, object>, Func<object, Byte[], Int32, Int32>> elementSetter)
        {
            Int32 setBytes(Object parent, Byte[] data, Int32 index)
            {
                Int32 innerIndex = index;
                var count = BitConverter.ToInt32(data, innerIndex);
                if (count == 0) return 4;
                innerIndex += 4;
                var arr = new Object[count];
                var counter = 0;
                for (int i = 0; i < count; i++)
                {
                    void set(Object p, Object v)
                    {
                        ((Object[])p)[i] = v;
                    }
                    var count2 = elementSetter(set)(arr, data, innerIndex);
                    counter += count2;
                    innerIndex += count2;
                }
                var outArray = Array.CreateInstance(arr[0].GetType(), arr.Length);
                for (int i = 0; i < count; i++)
                {
                    outArray.SetValue(arr[i], i);
                }
                setter(parent, outArray);
                return innerIndex - index;
            }

            return setBytes;
        }
    }
}
