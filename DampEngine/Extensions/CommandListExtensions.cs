using Veldrid;
namespace DampEngine.Extensions
{
    public static class CommandListExtensions
    {
        public static CommandList BeginAnd(this CommandList list)
        {
            list.Begin();
            return list;
        }

        public static CommandList SetFrameBufferAnd(this CommandList list, Framebuffer buffer)
        {
            list.SetFramebuffer(buffer);
            return list;
        }
        public static CommandList ClearColorTargetAnd(this CommandList list, uint index, RgbaFloat clearcolour)
        {
            list.ClearColorTarget(index, clearcolour);
            return list;
        }
        public static CommandList SetVertexBufferAnd(this CommandList list, uint index, DeviceBuffer buffer)
        {
            list.SetVertexBuffer(index, buffer);
            return list;
        }
        public static CommandList SetIndexBufferAnd(this CommandList list, DeviceBuffer buffer, IndexFormat format)
        {
            list.SetIndexBuffer(buffer, format);
            return list;
        }
        public static CommandList SetPipelineAnd(this CommandList list, Pipeline pipeline)
        {
            list.SetPipeline(pipeline);
            return list;
        }

        public static CommandList SetGraphicsResourceSetAnd(this CommandList list, uint slot, ResourceSet rs)
        {
            list.SetGraphicsResourceSet(slot, rs);
            return list;
        }
        public static CommandList SetGraphicsResourceSetAnd(this CommandList list, uint slot, ResourceSet rs, uint dynamicOffsetCount)
        {
            uint off = 0;
            list.SetGraphicsResourceSet(slot, rs, dynamicOffsetCount, ref off);
            return list;
        }

        public static CommandList DrawIndexedAnd(this CommandList list, 
                uint indexCount,
                uint instanceCount,
                uint indexStart,
                int vertexOffset,
                uint instanceStart)
        {
            list.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
            return list;
        }

        /// <summary>
        /// Ends the command list and submits the commands to the graphics device
        /// </summary>
        /// <param name="list"></param>
        /// <param name="gd"></param>
        public static void Submit(this CommandList list, GraphicsDevice gd)
        {
            list.End();
            gd.SubmitCommands(list);
        }
    }
}
