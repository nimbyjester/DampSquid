using DampEngine.Drawing.Visual;
using DampEngine.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using Veldrid.Utilities;

namespace DampEngine
{
    public abstract class Scene
    {
        private Lazy<DisposeCollectorResourceFactory> _lazyfactory = new Lazy<DisposeCollectorResourceFactory>(() => new DisposeCollectorResourceFactory(DampCore.GraphicsDevice.ResourceFactory));
        protected DisposeCollectorResourceFactory factory => _lazyfactory.Value;
        protected List<Renderable> renderables = new List<Renderable>();
        public Boolean IsActive { get; set; } = false;

        public abstract void DisposeResources();
        public abstract void CreateResources();
        public void Draw()
        {
            DampCore.FrameCommands
                   .BeginAnd()
                   .SetFrameBufferAnd(DampCore.GraphicsDevice.SwapchainFramebuffer)
                   .ClearColorTargetAnd(0, RgbaFloat.Black);
           
            //maybe order renderables?

            renderables.ForEach(r => r.Draw());

            DampCore.FrameCommands.Submit(DampCore.GraphicsDevice);
        }
        public abstract void Update(Update gameTime);
    }
}
