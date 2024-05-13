using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace AnimationPipeline.Animation
{
    public class DynamicModelMeshPartContent
    {
        protected internal ModelMeshPartContent Source { get; protected set; }

        // Summary:
        //     Gets the offset, in bytes, from the first index of the of vertex buffer for this mesh part.
        public int VertexOffset { get; set; }
        
        // Summary:
        //     Gets the number of vertices used in this mesh part.
        public int NumVertices { get; set; }
        
        // Summary:
        //     Gets the location in the index buffer at which to start reading vertices.
        public int StartIndex { get; set; }
        
        // Summary:
        //     Gets the number of primitives to render for this mesh part.
        public int PrimitiveCount { get; set; }


        // Summary:
        //     Gets the vertex buffer associated with this mesh part.
        [ContentSerializerIgnore]
        public VertexBufferContent VertexBuffer { get; set; }

        // Summary:
        //     Gets the index buffer associated with this mesh part.
        [ContentSerializerIgnore]
        public Collection<int> IndexBuffer { get; set; }

        // Summary:
        //     Gets the material of this mesh part.
        [ContentSerializer(SharedResource = true)]
        public MaterialContent Material { get; set; }

        // Summary:
        //     Gets a user defined tag object.
        [ContentSerializer(SharedResource = true)]
        public object Tag { get; set; }


        public DynamicModelMeshPartContent(ModelMeshPartContent source)
        {
            this.Source = source;
            this.VertexOffset = source.VertexOffset;
            this.NumVertices = source.NumVertices;
            this.StartIndex = source.StartIndex;
            this.PrimitiveCount = source.PrimitiveCount;
            this.VertexBuffer = source.VertexBuffer;
            this.IndexBuffer = source.IndexBuffer;
            this.Material = source.Material;
            this.Tag = Tag;
        }
    }
}
