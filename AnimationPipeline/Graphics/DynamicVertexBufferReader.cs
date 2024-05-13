using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AnimationPipeline.Graphics
{
    public class DynamicVertexBufferReader : ContentTypeReader<DynamicVertexBuffer>
    {
        protected override DynamicVertexBuffer Read(ContentReader input, DynamicVertexBuffer buffer)
        {   
            IGraphicsDeviceService graphicsDeviceService = (IGraphicsDeviceService)input.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
            var device = graphicsDeviceService.GraphicsDevice;

            // read standard VertexBuffer
            var declaration = input.ReadRawObject<VertexDeclaration>();
            var vertexCount = (int)input.ReadUInt32();
            int dataSize = vertexCount * declaration.VertexStride;
            byte[] data = new byte[dataSize];
            input.Read(data, 0, dataSize);

            // read extras
            bool IsWriteOnly = input.ReadBoolean();
            
            if (buffer == null)
            {
                BufferUsage usage = (IsWriteOnly) ? BufferUsage.WriteOnly : BufferUsage.None;
                buffer = new DynamicVertexBuffer(device, declaration, vertexCount, usage);
            }
            buffer.SetData(data, 0, dataSize);

            return buffer;
        }
    }
}