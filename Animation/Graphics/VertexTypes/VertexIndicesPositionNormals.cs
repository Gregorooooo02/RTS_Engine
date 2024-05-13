﻿using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Animation.Graphics
{
    [StructLayout(LayoutKind.Explicit)]
    public struct VertexIndicesWeightsPositionNormal : IVertexType
    {        
        [FieldOffset( 0)] public byte BlendIndex0;
        [FieldOffset( 1)] public byte BlendIndex1;        
        [FieldOffset( 2)] public byte BlendIndex2;                
        [FieldOffset( 3)] public byte BlendIndex3;
        [FieldOffset( 4)] public Vector4 BlendWeights;
        [FieldOffset(20)] public Vector3 Position;
        [FieldOffset(32)] public Vector3 Normal;

        
        #region IVertexType Members
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement[] 
                {                  
                    new VertexElement( 0, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0),
                    new VertexElement( 4, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                    new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                });

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        #endregion
        

        public VertexIndicesWeightsPositionNormal(Vector3 position, Vector3 normal, Vector4 blendWeights, byte blendIndex0, byte blendIndex1, byte blendIndex2, byte blendIndex3)
        {
            this.BlendIndex0 = blendIndex0;
            this.BlendIndex1 = blendIndex1;
            this.BlendIndex2 = blendIndex2;
            this.BlendIndex3 = blendIndex3;
            this.BlendWeights = blendWeights;
            this.Position = position;
            this.Normal = normal;
        }

        public override string ToString()
        {
            return string.Format("{{Position:{0} Normal:{1} BlendWeights:{2} BlendIndices:{3},{4},{5},{6} }}",
                new object[] { Position, Normal, BlendWeights, BlendIndex0, BlendIndex1, BlendIndex2, BlendIndex3 });
        }
    }
}