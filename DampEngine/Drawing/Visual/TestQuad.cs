using DampEngine.Drawing.Projection;
using DampEngine.Drawing.Vertex;
using DampEngine.Drawing.Visual.Geometry;
using DampEngine.Extensions;
using System;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace DampEngine.Drawing.Visual
{
    public class TestQuad: Renderable
    {
        private ResourceSet _mainPerObjectRS;
        public Transform transform = new Transform();
        private const string VertexCode = @"
#version 450

struct Damp_WorldAndInverse
{
    mat4 World;
    mat4 InverseWorld;
};

layout(set = 0, binding = 0) uniform Projection
{
    mat4 _Projection;
};

layout(set = 0, binding = 1) uniform View
{
    mat4 _View;
};

layout(set = 1, binding = 0) uniform WorldAndInverse
{
    Damp_WorldAndInverse _WorldAndInverse;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    vec4 worldPosition = _WorldAndInverse.World * vec4(Position, 1);
    vec4 viewPosition = _View * worldPosition;
    gl_Position = _Projection * viewPosition;    
    fsin_Color = Color;
}";

        private const string FragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

        protected override void CreateGeometry()
        {
            var geo = new TestQuadGeometry();
            _buffers = geo.Load(factory);
        }

        protected override void CreateShaders()
        {
            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(VertexCode),
                "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(FragmentCode),
                "main");
            _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        }

        protected override void CreatePipeline()
        {
            CreateTransformResources();
            _pipeline = PipelineFactory.CreateForCameraAndTransform(factory, (_shaders, VertexPositionColor.vertexLayout));
        }

        public override void DisposeResources()
        {
            factory.DisposeCollector.DisposeAll();
        }

        public override void Draw()
        {
            DampCore.FrameCommands
                   .SetPipelineAnd(_pipeline)
                   .SetVertexBufferAnd(0, _buffers.VertexBuffer)
                   .SetIndexBufferAnd(_buffers.IndexBuffer, IndexFormat.UInt16)
                   .SetGraphicsResourceSetAnd(0, DampCore.FrameCamera.ResourceSet)
                   .SetGraphicsResourceSetAnd(1, _mainPerObjectRS, 1)
                   .DrawIndexed(
                       indexCount: 4,
                       instanceCount: 1,
                       indexStart: 0,
                       vertexOffset: 0,
                       instanceStart: 0);
        }

        public override void Update(Update gameTime)
        {
            if (transform.IsDirty) transform.Update(gameTime);
        }

        private void CreateTransformResources()
        {
            transform.Load(factory);
            _mainPerObjectRS = factory.CreateResourceSet(new ResourceSetDescription(LayoutFactory.GetTransform(factory),
                new DeviceBufferRange(transform.Buffer, 0, WorldAndInverse.SizeInBytes)));
        }
    }

    public class Transform
    {
        private DeviceBuffer _worldAndInverseBuffer;
        private Vector3 _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;
        private bool isDirty;

        public DeviceBuffer Buffer => _worldAndInverseBuffer;
        public Vector3 Position { get => _position; set { _position = value; IsDirty = true; } }
        public Vector3 Velocity { get; set; }
        public Quaternion Rotation { get => _rotation; set { _rotation = value; IsDirty = true; } }
        public Vector3 RotationVelocity { get; set; }
        public Vector3 Scale { get => _scale; set { _scale = value; IsDirty = true; } }

        public Boolean IsDirty {
            get => isDirty || Velocity.Length() == 0 || RotationVelocity.Length() == 0;
            set => isDirty = value; }

        public Matrix4x4 GetTransformMatrix()
        {
            return Matrix4x4.CreateScale(_scale)
                * Matrix4x4.CreateFromQuaternion(_rotation)
                * Matrix4x4.CreateTranslation(Position);
        }

        public void Load(DisposeCollectorResourceFactory factory)
        {
            _worldAndInverseBuffer = factory.CreateBuffer(new BufferDescription(WorldAndInverse.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            Update();
        }

        private void Update()
        {
            WorldAndInverse wai;
            wai.World = GetTransformMatrix();
            wai.InverseWorld = VdUtilities.CalculateInverseTranspose(ref wai.World);
            DampCore.GraphicsDevice.UpdateBuffer(_worldAndInverseBuffer, 0, ref wai);
        }
        public void Update(Update update)
        {
            Position += Velocity * (Single)update.ElapsedDeltaSeconds;
            var rot = RotationVelocity * (Single)update.ElapsedDeltaSeconds;
            Rotation = Quaternion.Concatenate(Rotation, Quaternion.CreateFromYawPitchRoll(rot.X, rot.Y, rot.Z));
          //  Rotation = Quaternion.Lerp(Rotation, Quaternion.Concatenate(Rotation, RotationVelocity), (Single)update.ElapsedDeltaSeconds);
            //Rotation = Quaternion.Concatenate(Rotation, RotationVelocity);

            Update();
        }
    }

    //public class SceneContext
    //{
    //    public DeviceBuffer ProjectionMatrixBuffer { get; private set; }
    //    public DeviceBuffer ViewMatrixBuffer { get; private set; }
    //    // public DeviceBuffer CameraInfoBuffer { get; private set; }

    //    public Camera Camera { get; private set; } = new Camera();
    //    public virtual void CreateDeviceObjects(DisposeCollectorResourceFactory factory)
    //    {
    //        ProjectionMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
    //        ViewMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
    //        //CameraInfoBuffer = factory.CreateBuffer(new BufferDescription(CameraInfo.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
    //        if (Camera != null)
    //        {
    //            UpdateCameraBuffers();
    //        }
    //    }

    //    public void Update()
    //    {
    //        UpdateCameraBuffers();
    //    }
    //    private void UpdateCameraBuffers()
    //    {
    //        if (Camera != null) Camera.UpdateBackend();
    //        if (Camera != null) Camera.Update(0);
    //        if (ProjectionMatrixBuffer != null) DampCore.FrameCommands.UpdateBuffer(ProjectionMatrixBuffer, 0, Camera.ProjectionMatrix);
    //        if (ViewMatrixBuffer != null) DampCore.FrameCommands.UpdateBuffer(ViewMatrixBuffer, 0, Camera.ViewMatrix);
    //        //DampCore.FrameCommands.UpdateBuffer(CameraInfoBuffer, 0, Camera.GetCameraInfo());
    //    }
    //}

    public struct CameraInfo
    {
        public Vector3 CameraPosition_WorldSpace;
        public Vector3 CameraLookDirection;

        public const uint SizeInBytes = 24;
    }

    public struct WorldAndInverse
    {
        public Matrix4x4 World;
        public Matrix4x4 InverseWorld;
        public const uint SizeInBytes = 128;
    }


}
