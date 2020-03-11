using DampEngine.Drawing.Visual.Geometry;
using System;
using Veldrid;
using Veldrid.Utilities;

namespace DampEngine.Drawing.Visual
{
    public abstract class Renderable
    {
        protected Buffers _buffers;
        protected Shader[] _shaders;
        protected Pipeline _pipeline;
        private Lazy<DisposeCollectorResourceFactory> _lazyfactory = new Lazy<DisposeCollectorResourceFactory>(() => new DisposeCollectorResourceFactory(DampCore.GraphicsDevice.ResourceFactory));
        protected DisposeCollectorResourceFactory factory => _lazyfactory.Value;


        public void CreateResources()
        {
            CreateGeometry();
            CreateShaders();
            CreatePipeline();
        }

        protected abstract void CreateGeometry();

        protected abstract void CreateShaders();

        protected abstract void CreatePipeline();

        public abstract void DisposeResources();

        public abstract void Draw();

        public virtual void Update(Update gameTime)
        {}
    }
}
