using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Pipeline.Graphics;

namespace Pipeline.Serialization
{
    [ContentTypeWriter]
    public class DynamicVertexBufferWriter : ContentTypeWriter<DynamicVertexBufferContent>
    {    
        protected override void Write(ContentWriter output, DynamicVertexBufferContent buffer)
        {            
            WriteVertexBuffer(output, buffer);

            output.Write(buffer.IsWriteOnly);

            return;
        }

        private void WriteVertexBuffer(ContentWriter output, DynamicVertexBufferContent buffer)
        {
            var vertexCount = buffer.VertexData.Length / buffer.VertexDeclaration.VertexStride;
            output.WriteRawObject(buffer.VertexDeclaration);
            output.Write((UInt32)vertexCount);
            output.Write(buffer.VertexData);
        }
                
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Graphics.Content.DynamicVertexBufferReader, Graphics";
        }
        
    }
}