using System;
using Graphics.ContentReaders;
using GraphicsImporters.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace GraphicsImporters.Serialization
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
            var type = typeof(DynamicVertexBufferReader);
            var readerType = type.Namespace + ".DynamicVertexBufferReader, " + type.Assembly.FullName;
            return readerType;
        }
        
    }
}
