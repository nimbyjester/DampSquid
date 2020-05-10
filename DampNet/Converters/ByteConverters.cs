using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

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
            converters.Add(typeof(String), (StringConverter, StringConverter));
            converters.Add(typeof(Object), (StringConverter, StringConverter));
            converters.Add(typeof(Vector3), (Vector3Converter, Vector3Converter));
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


            var list = DampConverter.CreateConverterList(type);

            Func<object, Byte[]> GetFrom(Func<object, object> getter)
            {
                Byte[] getBytes(Object parent)
                {
                    var obj = getter(parent);
                    if (obj == null) return Array.Empty<Byte>();
                    var bytes = list.SelectMany(v => v.getter(obj));
                    return bytes.ToArray();
                }
                return getBytes;
            }
            Func<object, Byte[], Int32, Int32> SetFrom(Action<object, object> setter)
            {
                Int32 setBytes(Object parent, Byte[] data, Int32 index)
                {
                    if (data.Length == 0) return index;
                    var cons = type.GetConstructors().First();
                   
                    var consturctedObj = cons.Invoke(cons.GetParameters().Select(s => GetDefault(s.ParameterType)).ToArray());


                   // var consturctedObj = type.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<Object>());
                    var usedBytes = 0;
                    foreach (var converter in list)
                    {
                        usedBytes++;
                        index += converter.setter(consturctedObj, data, index);
                    }
                    setter(parent, consturctedObj);
                    return usedBytes;
                }

                return setBytes;
            }
            converters.Add(type, (GetFrom, SetFrom));
            return converters[type];


           // throw new InvalidCastException();
        }
        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
