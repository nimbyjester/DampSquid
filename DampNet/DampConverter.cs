using System;
using System.Collections.Generic;
using System.Reflection;

namespace DampNet
{
    public static class DampConverter
    {
        
        private static Dictionary<Type, List<(
                            SerializerMode mode,
                            Func<object, Byte[]> getter,
                            Func<object, Byte[], Int32, Int32> setter)>> converters = new Dictionary<Type, List<(
                                                                                        SerializerMode mode,
                                                                                        Func<object, Byte[]> getter,
                                                                                        Func<object, Byte[], Int32, Int32> setter)>>();
        private static List<(
                            SerializerMode mode,
                            Func<object, Byte[]> getter,
                            Func<object, Byte[], Int32, Int32> setter)> GetConverters(Type type)
        {
            if (converters.TryGetValue(type, out var output)) return output;
            return CreateConverterList(type);

        }
        internal static List<(
                            SerializerMode mode,
                            Func<object, Byte[]> getter,
                            Func<object, Byte[], Int32, Int32> setter)> CreateConverterList(Type type)
        {
            lock (converters)
            {
                if (converters.TryGetValue(type, out var output)) return output;

                output = new List < (
                            SerializerMode mode,
                            Func<object, Byte[]> getter,
                            Func<object, Byte[], Int32, Int32> setter)>();
                var properties = type.GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo property = properties[i];
                    DampData att = (DampData)Attribute.GetCustomAttribute(property, typeof(DampData));
                    SerializerMode mode = att?.SerializeMode ?? SerializerMode.None;
                    if (mode == SerializerMode.None) continue;
                    var propertyType = property.PropertyType;
                    Func<object, object> getter = property.GetValue;
                    Action<object, object> setter = property.SetValue;
                    var byteConverters = ByteConverters.GetConvertersFor(propertyType);
                    output.Add((mode,
                           byteConverters.getter(getter),
                           byteConverters.setter(setter)));
                }
                converters.Add(type, output);
                return output;
            }
        }

        public static Byte[] Serialize(Object obj, SerializerMode mode)
        {
            var converterList = GetConverters(obj.GetType());            
            var output = new List<Byte>();
            for (int i = 0; i < converterList.Count; i++)
            {
                var converter = converterList[i];
                if ((converter.mode & mode) == 0) continue;
                output.AddRange(converter.getter(obj));
            }
            return output.ToArray();
        }

        public static T DeSerialize<T>(Byte[] data, SerializerMode mode)
        {
            Type type = typeof(T);
            var converterList = GetConverters(type);
            Int32 index = 0;
            T output = (T)Activator.CreateInstance(type);
            for (int i = 0; i < converterList.Count; i++)
            {
                var converter = converterList[i];
                if ((converter.mode & mode) == 0) continue;
                index += converter.setter(output, data, index);
            }

            return output;
        }

    }
}
