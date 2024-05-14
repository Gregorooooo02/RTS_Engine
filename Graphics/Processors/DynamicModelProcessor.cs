using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Pipeline.Graphics;

namespace Pipeline.Serialization
{
    [ContentProcessor(DisplayName = "DynamicModel - Custom")]
    public class DynamicModelProcessor : ModelProcessor, IContentProcessor
    {
        DynamicModelContent.BufferType _vertexBufferType = DynamicModelContent.BufferType.Dynamic;
        DynamicModelContent.BufferType _indexBufferType = DynamicModelContent.BufferType.Dynamic;

        // used to avoid creating clones/duplicates of the same VertexBufferContent
        Dictionary<VertexBufferContent, DynamicVertexBufferContent> _vertexBufferCache = new Dictionary<VertexBufferContent, DynamicVertexBufferContent>();
        // used to avoid creating clones/duplicates of the same Index Buffer
        Dictionary<Collection<int>, DynamicIndexBufferContent> _indexBufferCache = new Dictionary<Collection<int>, DynamicIndexBufferContent>();

#if WINDOWS
        // override OutputType
        [Browsable(false)]
#endif
        Type IContentProcessor.OutputType { get { return typeof(DynamicModelContent); } }
        
        [DefaultValue(DynamicModelContent.BufferType.Dynamic)]
        public virtual DynamicModelContent.BufferType VertexBufferType
        {
            get { return  _vertexBufferType; }
            set { _vertexBufferType = value; }
        }
        
        [DefaultValue(DynamicModelContent.BufferType.Dynamic)]
        public virtual DynamicModelContent.BufferType IndexBufferType
        {
            get { return  _indexBufferType; }
            set { _indexBufferType = value; }
        }

        public DynamicModelProcessor()
        {
        }

        object IContentProcessor.Process(object input, ContentProcessorContext context)
        {
            var model = Process((NodeContent)input, context);
            var dynamicModel = new DynamicModelContent(model);
            
            foreach(var mesh in dynamicModel.Meshes)
            {
                foreach(var part in mesh.MeshParts)
                {
                    ProcessVertexBuffer(dynamicModel, context, part);
                    ProcessIndexBuffer(dynamicModel, context, part);
                }
            }

            return dynamicModel;
        }

        protected virtual void ProcessVertexBuffer(DynamicModelContent dynamicModel, ContentProcessorContext context, DynamicModelMeshPartContent part)
        {
            if(VertexBufferType != DynamicModelContent.BufferType.Default)
            {
                // Replace the default VertexBufferContent with CpuAnimatedVertexBufferContent.
                DynamicVertexBufferContent vb;
                if (!_vertexBufferCache.TryGetValue(part.VertexBuffer, out vb))
                {
                    vb = new DynamicVertexBufferContent(part.VertexBuffer);
                    vb.IsWriteOnly = (VertexBufferType == DynamicModelContent.BufferType.DynamicWriteOnly);
                    _vertexBufferCache[part.VertexBuffer] = vb;
                }
                part.VertexBuffer = vb;
            }
        }

        protected virtual void ProcessIndexBuffer(DynamicModelContent dynamicModel, ContentProcessorContext context, DynamicModelMeshPartContent part)
        {
            if(IndexBufferType != DynamicModelContent.BufferType.Default)
            {
                DynamicIndexBufferContent ib;
                if (!_indexBufferCache.TryGetValue(part.IndexBuffer, out ib))
                {
                    ib = new DynamicIndexBufferContent(part.IndexBuffer);
                    ib.IsWriteOnly = (IndexBufferType == DynamicModelContent.BufferType.DynamicWriteOnly);
                    _indexBufferCache[part.IndexBuffer] = ib;
                }
                part.IndexBuffer = ib;
            }
        }
        
        
        
    }
}