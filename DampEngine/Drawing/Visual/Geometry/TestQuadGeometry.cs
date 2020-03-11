using DampEngine.Drawing.Vertex;
using System.Numerics;
using Veldrid;

namespace DampEngine.Drawing.Visual.Geometry
{
    public class TestQuadGeometry : PositionColourGeometry
    {
        public TestQuadGeometry()
        {
            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector3(-.75f, .75f, 0), RgbaFloat.Red),
                new VertexPositionColor(new Vector3(.75f, .75f, 0), RgbaFloat.Green),
                new VertexPositionColor(new Vector3(-.75f, -.75f, 0), RgbaFloat.Blue),
                new VertexPositionColor(new Vector3(.75f, -.75f, 0), RgbaFloat.Yellow)
            };
            Vertices = quadVertices;
            ushort[] quadIndices = { 0, 1, 2, 3 };
            Indices = quadIndices;
        }
    }
}
