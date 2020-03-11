using DampEngine.Drawing.Vertex;

namespace DampEngine.Drawing.Visual.Geometry
{
    public class PositionColourGeometry: GeometryBase<VertexPositionColor>
    { 
        public PositionColourGeometry()
        {
            VertexLayout = VertexPositionColor.vertexLayout;
        }
    }
}
