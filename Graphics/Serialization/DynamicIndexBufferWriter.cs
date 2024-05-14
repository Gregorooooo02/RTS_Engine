using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Pipeline.Graphics;

namespace Pipeline.Serialization
{
    [ContentTypeWriter]
    class DynamicIndexBufferWriter : ContentTypeWriter<DynamicIndexBufferContent>
    {
        protected override void Write(ContentWriter output, DynamicIndexBufferContent buffer)
        {   
            WriteIndexBuffer(output, buffer);

            output.Write(buffer.IsWriteOnly);

            return;
        }

        private static void WriteIndexBuffer(ContentWriter output, DynamicIndexBufferContent buffer)
        {
            // check if the buffer contains values greater than UInt16.MaxValue
            var is16Bit = true;
            foreach(var index in buffer)
            {
                if(index > UInt16.MaxValue)
                {
                    is16Bit = false;
                    break;
                }
            }
            
           var stride  = (is16Bit) ? 2 : 4;

            output.Write(is16Bit); // Is 16 bit
            output.Write((UInt32)(buffer.Count*stride)); // Data size
            if (is16Bit)
            {
	            foreach (var item in buffer)
	                output.Write((UInt16)item);
            }
            else
            {
	            foreach (var item in buffer)
	                output.Write(item);
            }
        }
        
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Graphics.Content.DynamicIndexBufferReader, Graphics";
        }
        
    }
}