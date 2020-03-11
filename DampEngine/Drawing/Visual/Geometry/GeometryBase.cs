using Veldrid;
using Veldrid.Utilities;

namespace DampEngine.Drawing.Visual.Geometry
{
    public class GeometryBase<T> where T : struct
    {
        public T[] Vertices;
        public ushort[] Indices;
        public VertexLayoutDescription VertexLayout;
        public Buffers Load(DisposeCollectorResourceFactory factory)
        {
            var output = new Buffers
            {
                VertexBuffer = factory.CreateBuffer(new BufferDescription(4 * VertexLayout.Stride, BufferUsage.VertexBuffer)),
                IndexBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer))
            };
            DampCore.GraphicsDevice.UpdateBuffer(output.VertexBuffer, 0, Vertices);
            DampCore.GraphicsDevice.UpdateBuffer(output.IndexBuffer, 0, Indices);

            return output;
        }
    }
}
