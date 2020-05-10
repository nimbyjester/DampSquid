using System;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using Xunit;

namespace DampNet.Test
{
    public class UnitTest1
    {
        [Fact]
        public void PropertiesFlaggedAsTCPShouldSerialize()
        {
            var t = new Test();
            t.num16 = 1;
            t.num32 = 2;
            t.single = 1.2F;
            t.vec = new Vector3(4, 5, 6);
            t.vec2 = new Vector3(7, 8, 9);
            t.uNum16 = 123;
            var bytes = DampConverter.Serialize(t, SerializerMode.TCP);
            var b = DampConverter.DeSerialize<Test>(bytes, SerializerMode.TCP);
            Assert.Equal(t.num16, b.num16);
            Assert.Equal(t.num32, b.num32);
            Assert.Equal(t.single, b.single);
            Assert.Equal(default(Vector3), b.vec);
            Assert.Equal(t.vec2, b.vec2);
            Assert.Equal(t.uNum16, b.uNum16);
        }

        [Fact]
        public void PropertiesFlaggedAsUDPShouldSerialize()
        {
            var t = new Test();
            t.num16 = 1;
            t.num32 = 2;
            t.single = 1.2F;
            t.vec = new Vector3(4, 5, 6);
            t.vec2 = new Vector3(7, 8, 9);
            t.uNum16 = 123;
            t.array = new[] { new Vector3(4, 5, 6), new Vector3(7, 8, 9) };
            t.Hi = "Hello World";

            var bytes = DampConverter.Serialize(t, SerializerMode.UDP);
            var b = DampConverter.DeSerialize<Test>(bytes, SerializerMode.UDP);

            Assert.Equal(default(Int16), b.num16);
            Assert.Equal(default(Int32), b.num32);
            Assert.Equal(default(Single), b.single);
            Assert.Equal(t.vec, b.vec);
            Assert.Equal(t.vec2, b.vec2);
            Assert.Equal(t.uNum16, b.uNum16);
            Assert.Equal(t.array, b.array);
            Assert.Equal(t.Hi, b.Hi);
        }

        [Fact]
        public void CombinedFlagPropertiesShouldSerialize()
        {
            var t = new Test();
            t.num16 = 1;
            t.num32 = 2;
            t.single = 1.2F;
            t.vec = new Vector3(4, 5, 6);
            t.vec2 = new Vector3(7, 8, 9);
            t.uNum16 = 123;
            var bytes = DampConverter.Serialize(t, SerializerMode.TCP | SerializerMode.UDP);
            var b = DampConverter.DeSerialize<Test>(bytes, SerializerMode.TCP | SerializerMode.UDP);
            Assert.Equal(t.num16, b.num16);
            Assert.Equal(t.num32, b.num32);
            Assert.Equal(t.single, b.single);
            Assert.Equal(t.vec, b.vec);
            Assert.Equal(t.vec2, b.vec2);
            Assert.Equal(t.uNum16, b.uNum16);
        }

        [Fact]
        public void AllFlagPropertiesShouldSerialize()
        {
            var t = new Test();
            t.num16 = 1;
            t.num32 = 2;
            t.single = 1.2F;
            t.vec = new Vector3(4, 5, 6);
            t.vec2 = new Vector3(7, 8, 9);
            t.uNum16 = 123;
            var bytes = DampConverter.Serialize(t, SerializerMode.ALL);
            var b = DampConverter.DeSerialize<Test>(bytes, SerializerMode.ALL);

            Assert.Equal(t.num16, b.num16);
            Assert.Equal(t.num32, b.num32);
            Assert.Equal(t.single, b.single);
            Assert.Equal(t.vec, b.vec);
            Assert.Equal(t.vec2, b.vec2);
            Assert.Equal(t.uNum16, b.uNum16);
        }

        [Fact]
        public void IsFasterThanJSON()
        {
            var t = new Test();
            t.num16 = 1;
            t.num32 = 2;
            t.single = 1.2F;
            t.vec = new Vector3(4, 5, 6);
            t.vec2 = new Vector3(7, 8, 9);
            t.uNum16 = 123;
            t.array = new[] { new Vector3(4, 5, 6), new Vector3(7, 8, 9) };
            t.Hi = "Hello World";
            var b = new Test();
            var timer = new System.Diagnostics.Stopwatch();
            var loopCount = 1000;
            timer.Start();
            for (int i = 0; i < loopCount; i++)
            {
                var str = Newtonsoft.Json.JsonConvert.SerializeObject(t);
                b = Newtonsoft.Json.JsonConvert.DeserializeObject<Test>(str);
            }
            timer.Stop();
            var jsonTime = timer.ElapsedMilliseconds;
            timer.Reset();
            timer.Start();
            for (int i = 0; i < loopCount; i++)
            {
               var bytes = DampConverter.Serialize(t, SerializerMode.ALL);
               b = DampConverter.DeSerialize<Test>(bytes, SerializerMode.ALL);
            }
            timer.Stop();
            var dampTime = timer.ElapsedMilliseconds;
            Assert.True(dampTime < jsonTime);
        }

        [Fact]
        public void IsSmallerThanJSON()
        {
            var t = new Test();
            t.num16 = 1;
            t.num32 = 2;
            t.single = 1.2F;
            t.vec = new Vector3(4, 5, 6);
            t.vec2 = new Vector3(7, 8, 9);
            t.uNum16 = 123;
            t.array = new[] { new Vector3(4, 5, 6), new Vector3(7, 8, 9) };
            t.Hi = "Hello World";
            var jsonBytes = Newtonsoft.Json.JsonConvert.SerializeObject(t).Length * 2;
            
            var dampBytes = DampConverter.Serialize(t, SerializerMode.ALL).Length;
            
           Assert.True(dampBytes < jsonBytes);
        }
        public static byte[] Compress(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(buffer, 0, buffer.Length);
            zip.Close();
            ms.Position = 0;

            MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return gzBuffer;
        }

        public static byte[] Decompress(byte[] gzBuffer)
        {
            MemoryStream ms = new MemoryStream();
            int msgLength = BitConverter.ToInt32(gzBuffer, 0);
            ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

            byte[] buffer = new byte[msgLength];

            ms.Position = 0;
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            zip.Read(buffer, 0, buffer.Length);

            return buffer;
        }
        [Fact]
        public void CanDoArrayOfArrays()
        {
            var t = new ComplexTest();
            t.array = new[]
            {
                new[] { new Vector3(4, 5, 6), new Vector3(7, 8, 9) },
                new[] { new Vector3(14, 15, 16), new Vector3(17, 18, 19) },
                new[] { new Vector3(24, 25, 26), new Vector3(27, 28, 29) }
            };
            t.words = new[]
            {
                new[] {
                    new[] {
                    "Word1",
                    "Word2",
                    }
                },
                new[] {
                    new[] {
                    "Word3"
                    },
                    new[] {
                    "Word4",
                    "Word5"
                    }
                }
            };
            var bytes = DampConverter.Serialize(t, SerializerMode.ALL);
            var b = DampConverter.DeSerialize<ComplexTest>(bytes, SerializerMode.ALL);

            Assert.Equal(t.array, b.array);
            Assert.Equal(t.words, b.words);
        }

        [Fact]
        public void SubObjectsCanBeSerialized()
        {
            var t = new Test();
            t.num16 = 1;
            t.num32 = 2;
            t.single = 1.2F;
            t.vec = new Vector3(4, 5, 6);
            t.vec2 = new Vector3(7, 8, 9);
            t.uNum16 = 123;
            var a = new ObjectTest();
            a.test = t;
            var bytes = DampConverter.Serialize(a, SerializerMode.ALL);
            
            var b = DampConverter.DeSerialize<ObjectTest>(bytes, SerializerMode.ALL);
            Assert.Equal(t.num16, b.test.num16);
            Assert.Equal(t.num32, b.test.num32);
            Assert.Equal(t.single, b.test.single);
            Assert.Equal(t.vec, b.test.vec);
            Assert.Equal(t.vec2, b.test.vec2);
            Assert.Equal(t.uNum16, b.test.uNum16);
        }

    }
    public class ObjectTest
    {
        [DampData(SerializerMode.ALL)]
        public Test test { get; set; }
    }
    public class ComplexTest
    {
        [DampData(SerializerMode.ALL)]
        public Vector3[][] array { get; set; } = { };
        [DampData(SerializerMode.ALL)]
        public String[][][] words { get; set; } = { };
    }
    public class Test
    {
        [DampData(SerializerMode.TCP)]
        public Int32 num32 { get; set; }

        [DampData(SerializerMode.TCP)]
        public Single single { get; set; }

        [DampData(SerializerMode.TCP)]
        public Int16 num16 { get; set; }

        [DampData(SerializerMode.UDP)]
        public Vector3 vec { get; set; }

        [DampData(SerializerMode.UDP | SerializerMode.TCP)]
        public Vector3 vec2 { get; set; }

        [DampData(SerializerMode.ALL)]
        public UInt16 uNum16 { get; set; }

        [DampData(SerializerMode.UDP)]
        public Vector3[] array { get; set; } = { };

        [DampData(SerializerMode.UDP)]
        public String Hi { get; set; }
    }
}
