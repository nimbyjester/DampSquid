using System;
using System.Collections.Generic;
using System.Numerics;

namespace DampNet
{
    public static partial class ByteConverters
    {
        private static Dictionary<Type, (
            Func<Func<object, object>, Func<object, Byte[]>>,
            Func<Action<object, object>, Func<object, Byte[], Int32, Int32>>
            )> converters = new Dictionary<Type,
                (Func<Func<object, object>, Func<object, Byte[]>>,
                Func<Action<object, object>, Func<object, Byte[], Int32, Int32>>)>();
        static ByteConverters()
        {
            converters.Add(typeof(Int32), (Int32Converter, Int32Converter));
            converters.Add(typeof(UInt32), (UInt32Converter, UInt32Converter));
            converters.Add(typeof(Int16), (Int16Converter, Int16Converter));
            converters.Add(typeof(UInt16), (UInt16Converter, UInt16Converter));
            converters.Add(typeof(Single), (SingleConverter, SingleConverter));
            converters.Add(typeof(Double), (DoubleConverter, DoubleConverter));
            converters.Add(typeof(Char), (CharConverter, CharConverter));
            converters.Add(typeof(Vector3), (Vector3Converter, Vector3Converter));
            converters.Add(typeof(String), (StringConverter, StringConverter));
            converters.Add(typeof(Object), (StringConverter, StringConverter));
        }
        public static (Func<Func<object, object>, Func<object, Byte[]>> getter, Func<Action<object, object>, Func<object, Byte[], Int32, Int32>> setter) GetConvertersFor(Type type)
        {
            if(type.IsArray)
            {
                var elementConverters = GetConvertersFor(type.GetElementType());
                Func<object, Byte[]> get(Func<object, object> getter)
                {
                    return ArrayConverter(getter, elementConverters.getter);
                }
                Func<object, Byte[], Int32, Int32> set(Action<object, object> setter)
                {
                    return ArrayConverter(setter, elementConverters.setter);
                }
                return (get, set);
            }

            if (converters.TryGetValue(type, out (
            Func<Func<object, object>, Func<object, Byte[]>>,
            Func<Action<object, object>, Func<object, Byte[], Int32, Int32>>
            )  output)) return output;

            throw new InvalidCastException();
        }
    }
}
