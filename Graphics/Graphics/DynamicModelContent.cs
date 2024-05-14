using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Pipeline.Graphics 
{    
    public class DynamicModelContent
    {
        [Flags]
        public enum BufferType : int
        {
            /// <summary>
            /// Use the default BufferReader
            /// </summary>
            Default = int.MinValue,

            /// <summary>
            /// Deserialize a Dynamic Buffer
            /// </summary> 
            Dynamic = 0,
            
            /// <summary>
            /// Deserialize a Dynamic Buffer with BufferUsage.WriteOnly
            /// </summary>
            DynamicWriteOnly = 0x01,
        }

        protected internal ModelContent Source { get; protected set; }
        public BufferType VertexBufferType = BufferType.Dynamic;
        public BufferType IndexBufferType = BufferType.Dynamic;
        
        // Summary:
        //     Gets the collection of bones that are referenced by this model.
        public ModelBoneContentCollection Bones { get { return Source.Bones; } }
        
        // Summary:
        //     Gets the collection of meshes that are associated with this model.
        [ContentSerializerIgnore]
        public List<DynamicModelMeshContent> Meshes { get; private set; }

        // Summary:
        //     Gets the root bone of this model
        [ContentSerializerIgnore]
        public ModelBoneContent Root { get { return Source.Root; } }
        
        // Summary:
        //     Gets a user defined tag object.
        [ContentSerializer(SharedResource = true)]
        public object Tag { get { return Source.Tag; } set { Source.Tag = value; } }

        public DynamicModelContent(ModelContent source)
        {
            this.Source = source;

            //deep clone Meshes
            Meshes = new List<DynamicModelMeshContent>(source.Meshes.Count);
            foreach(var mesh in source.Meshes)
                Meshes.Add(new DynamicModelMeshContent(mesh));
        }

    }
}