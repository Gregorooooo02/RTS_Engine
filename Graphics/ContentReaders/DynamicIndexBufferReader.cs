﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Graphics.Content
{
    public class DynamicIndexBufferReader : ContentTypeReader<DynamicIndexBuffer>
    {
        protected override DynamicIndexBuffer Read(ContentReader input, DynamicIndexBuffer buffer)
        {   
            IGraphicsDeviceService graphicsDeviceService = (IGraphicsDeviceService)input.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
            var device = graphicsDeviceService.GraphicsDevice;

            // read IndexBuffer
            var is16Bit = input.ReadBoolean();
            var dataSize = (int)input.ReadUInt32();
            byte[] data = new byte[dataSize];
            input.Read(data, 0, dataSize);

            // read IsWriteOnly
            bool IsWriteOnly = input.ReadBoolean();


            if (buffer == null)
            {
                var elementSize = (is16Bit) ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits;
                var stride = (is16Bit) ? 2 : 4;
                var indexCount = dataSize / stride;
                BufferUsage usage = (IsWriteOnly) ? BufferUsage.WriteOnly : BufferUsage.None;
                buffer = new DynamicIndexBuffer(device, elementSize, indexCount, usage);
            }

            buffer.SetData(data, 0, dataSize);

            return buffer;
        }
    }
}