using System;
using System.Collections.Generic;
using Veldrid;

namespace DampEngine.Drawing.Visual
{

    public static class LayoutFactory
    {
        private static readonly Dictionary<ResourceLayoutDescription, ResourceLayout> s_layouts
            = new Dictionary<ResourceLayoutDescription, ResourceLayout>();

        public static ResourceLayout GetTransform(ResourceFactory factory)
        {
            var worldAndInverseDescription = new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WorldAndInverse", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment, ResourceLayoutElementOptions.DynamicBinding));
            return GetResourceLayout(factory, ref worldAndInverseDescription);
        }

        public static ResourceLayout GetProjectionView(ResourceFactory factory)
        {
            ResourceLayoutDescription projViewLayoutDescription = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex));

            return GetResourceLayout(factory, ref projViewLayoutDescription);
        }
        private static ResourceLayout GetResourceLayout(ResourceFactory factory, ref ResourceLayoutDescription desc)
        {
            if (!s_layouts.TryGetValue(desc, out ResourceLayout p))
            {
                p = factory.CreateResourceLayout(ref desc);
                s_layouts.Add(desc, p);
            }

            return p;
        }
    }

    public static class PipelineFactory
    {
        private static readonly Dictionary<GraphicsPipelineDescription, Pipeline> s_pipelines
            = new Dictionary<GraphicsPipelineDescription, Pipeline>();

        public static Pipeline CreateForCameraAndTransform(ResourceFactory factory, (Shader[] shaders, VertexLayoutDescription layout) shader)
        {                                                                                          
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);

            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = new ResourceLayout[] { LayoutFactory.GetProjectionView(factory), LayoutFactory.GetTransform(factory) };

            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { shader.layout },
                shaders: shader.shaders);

            pipelineDescription.Outputs = DampCore.GraphicsDevice.SwapchainFramebuffer.OutputDescription;

            return GetPipeline(factory, ref pipelineDescription);
        }

        private static Pipeline GetPipeline(ResourceFactory factory, ref GraphicsPipelineDescription desc)
        {
            if (!s_pipelines.TryGetValue(desc, out Pipeline p))
            {
                p = factory.CreateGraphicsPipeline(ref desc);
                s_pipelines.Add(desc, p);
            }

            return p;
        }
        
    }
}
