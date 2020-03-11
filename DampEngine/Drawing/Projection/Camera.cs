using DampEngine.Drawing.Visual;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace DampEngine.Drawing.Projection
{
    public class Camera
    {
        public DeviceBuffer ProjectionMatrixBuffer { get; private set; }
        public DeviceBuffer ViewMatrixBuffer { get; private set; }
        public ResourceSet ResourceSet { get; private set; }

        private Boolean _viewMatrixBufferDirty = true;
        private Boolean _projectionBufferDirty = true;
        private float _fov = 1f;
        private float _near = 1f;
        private float _far = 1000f;

        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;

        private Vector3 _position = new Vector3(0, 0, 10);
        private Vector3 _lookDirection = new Vector3(0, 0, -1);

        private float _yaw;
        private float _pitch;
        private float _roll;

        private bool _useReverseDepth => DampCore.GraphicsDevice.IsDepthRangeZeroToOne;
        private float windowWidth => DampCore.Window.Width;
        private float windowHeight => DampCore.Window.Height;

        public event Action<Matrix4x4> ProjectionChanged;
        public event Action<Matrix4x4> ViewChanged;

        public Camera()
        {
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public Vector3 LookDirection => _lookDirection;

        public float FarDistance => _far;

        public float FieldOfView => _fov;
        public float NearDistance => _near;

        public float AspectRatio => windowWidth / windowHeight;

        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
        public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }

        public float Roll { get => _roll; set { _roll = value; UpdateViewMatrix(); } }

        public void Update()
        {
            UpdateBuffers();
        }

        public void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = CreatePerspective(
                DampCore.GraphicsDevice,
                _useReverseDepth,
                _fov,
                windowWidth / windowHeight,
                _near,
                _far);
            ProjectionChanged?.Invoke(_projectionMatrix);
            _projectionBufferDirty = true;
        }

        private void UpdateViewMatrix()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            _lookDirection = lookDir;
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY) * Matrix4x4.CreateFromYawPitchRoll(0f, 0f, Roll);
            ViewChanged?.Invoke(_viewMatrix);
            _viewMatrixBufferDirty = true;
        }

        public CameraInfo GetCameraInfo() => new CameraInfo
        {
            CameraPosition_WorldSpace = _position,
            CameraLookDirection = _lookDirection
        };

        private static Matrix4x4 CreatePerspective(
                GraphicsDevice gd,
                bool useReverseDepth,
                float fov,
                float aspectRatio,
                float near, float far)
        {
            Matrix4x4 persp;
            if (useReverseDepth)
            {
                persp = CreatePerspective(fov, aspectRatio, far, near);
            }
            else
            {
                persp = CreatePerspective(fov, aspectRatio, near, far);
            }
            if (gd.IsClipSpaceYInverted)
            {
                persp *= new Matrix4x4(
                    1, 0, 0, 0,
                    0, -1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);
            }

            return persp;
        }

        private static Matrix4x4 CreatePerspective(float fov, float aspectRatio, float near, float far)
        {            
            if (fov <= 0.0f || fov >= Math.PI)
                throw new ArgumentOutOfRangeException(nameof(fov));

            if (near <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(near));

            if (far <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(far));

            float yScale = 1.0f / (float)Math.Tan(fov * 0.5f);
            float xScale = yScale / aspectRatio;

            Matrix4x4 result;

            result.M11 = xScale;
            result.M12 = result.M13 = result.M14 = 0.0f;

            result.M22 = yScale;
            result.M21 = result.M23 = result.M24 = 0.0f;

            result.M31 = result.M32 = 0.0f;
            var negFarRange = float.IsPositiveInfinity(far) ? -1.0f : far / (near - far);
            result.M33 = negFarRange;
            result.M34 = -1.0f;

            result.M41 = result.M42 = result.M44 = 0.0f;
            result.M43 = near * negFarRange;

            return result;
        }

        private static Matrix4x4 CreateOrtho(
            GraphicsDevice gd,
            bool useReverseDepth,
            float left, float right,
            float bottom, float top,
            float near, float far)
        {
            Matrix4x4 ortho;
            if (useReverseDepth)
            {
                ortho = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, far, near);
            }
            else
            {
                ortho = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, near, far);
            }
            if (gd.IsClipSpaceYInverted)
            {
                ortho *= new Matrix4x4(
                    1, 0, 0, 0,
                    0, -1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);
            }

            return ortho;
        }

        public void CreateDeviceObjects(DisposeCollectorResourceFactory factory)
        {
            ProjectionMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            ViewMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(LayoutFactory.GetProjectionView(factory),
                            ProjectionMatrixBuffer,
                            ViewMatrixBuffer));
        }
        private void UpdateBuffers()
        {
            UpdateProjectionMatrixBuffer();
            UpdateViewMatrixBuffer();
        }

        private void UpdateProjectionMatrixBuffer()
        {
            if (ProjectionMatrixBuffer == null || !_projectionBufferDirty) return;
            DampCore.FrameCommands.UpdateBuffer(ProjectionMatrixBuffer, 0, ProjectionMatrix);
            _projectionBufferDirty = false;
        }
        private void UpdateViewMatrixBuffer()
        {
            if (ViewMatrixBuffer == null || !_viewMatrixBufferDirty) return;
            DampCore.FrameCommands.UpdateBuffer(ViewMatrixBuffer, 0, ViewMatrix);
            _viewMatrixBufferDirty = false;
        }
    }

    public struct CameraInfo
    {
        public Vector3 CameraPosition_WorldSpace;
        public Vector3 CameraLookDirection;

        public const uint SizeInBytes = 24;
    }
}
