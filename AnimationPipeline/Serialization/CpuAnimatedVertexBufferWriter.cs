﻿using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using AnimationPipeline.Animation;

namespace AnimationPipeline.Animation
{
    [ContentTypeWriter]
    public class CpuAnimatedVertexBufferWriter : ContentTypeWriter<CpuAnimatedVertexBufferContent>
    {
        protected override void Write(ContentWriter output, CpuAnimatedVertexBufferContent buffer)
        {
            WriteVertexBuffer(output, buffer);

            output.Write(buffer.IsWriteOnly);
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
            return "tainicom.Aether.Animation.Content.CpuAnimatedVertexBufferReader, Aether.Animation";
        }
    }
}