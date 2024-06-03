using System.Collections.Generic;
using System.ComponentModel;
using AnimationImporters.Animation;
using GraphicsImporters.Graphics;
using GraphicsImporters.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;

namespace AnimationImporters.Processors
{
    [ContentProcessor(DisplayName = "CPU AnimatedModel - Custom")]
    class CpuAnimatedModelProcessor : DynamicModelProcessor, IContentProcessor
    {
        private int _maxBones = SkinnedEffect.MaxBones;
        private int _generateKeyframesFrequency = 0;
        private bool _fixRealBoneRoot = false;

        // used to avoid creating clones/duplicates of the same VertexBufferContent
        Dictionary<VertexBufferContent, CpuAnimatedVertexBufferContent> _vertexBufferCache = new Dictionary<VertexBufferContent,CpuAnimatedVertexBufferContent>();

        
        [DefaultValue(DynamicModelContent.BufferType.DynamicWriteOnly)]
        public new DynamicModelContent.BufferType VertexBufferType
        {
            get { return  base.VertexBufferType; }
            set { base.VertexBufferType = value; }
        }
        
        [DefaultValue(DynamicModelContent.BufferType.Default)]
        public new DynamicModelContent.BufferType IndexBufferType
        {
            get { return base.IndexBufferType; }
            set { base.IndexBufferType = value; }
        }
        
        [DisplayName("MaxBones")]
        [DefaultValue(SkinnedEffect.MaxBones)]
        public virtual int MaxBones
        {
            get { return _maxBones; }
            set { _maxBones = value; }
        }
        
        [DisplayName("Generate Keyframes Frequency")]
        [DefaultValue(0)] // (0=no, 30=30fps, 60=60fps)
        public virtual int GenerateKeyframesFrequency
        {
            get { return _generateKeyframesFrequency; }
            set { _generateKeyframesFrequency = value; }
        }

        [DisplayName("Fix BoneRoot from MG importer")]
        [DefaultValue(false)]
        public virtual bool FixRealBoneRoot
        {
            get { return _fixRealBoneRoot; }
            set { _fixRealBoneRoot = value; }
        }
        
        public CpuAnimatedModelProcessor()
        {
            VertexBufferType = DynamicModelContent.BufferType.DynamicWriteOnly;
            IndexBufferType  = DynamicModelContent.BufferType.Default;
        }

        object IContentProcessor.Process(object input, ContentProcessorContext context)
        {
            ModelContent model = Process((NodeContent)input, context);
            DynamicModelContent outputModel = new DynamicModelContent(model);
            
            foreach(DynamicModelMeshContent mesh in outputModel.Meshes)
            {
                foreach(DynamicModelMeshPartContent part in mesh.MeshParts)
                {
                    ProcessVertexBuffer(outputModel, context, part);
                    ProcessIndexBuffer(outputModel, context, part);
                }
            }

            // import animation
            AnimationsProcessor animationProcessor = new AnimationsProcessor();
            animationProcessor.MaxBones = this.MaxBones;
            animationProcessor.GenerateKeyframesFrequency = this.GenerateKeyframesFrequency;
            animationProcessor.FixRealBoneRoot = this._fixRealBoneRoot;
            AnimationsContent animation = animationProcessor.Process((NodeContent)input, context);
            outputModel.Tag = animation;

            //ProcessNode((NodeContent)input);

            return outputModel;
        }

        protected override void ProcessVertexBuffer(DynamicModelContent dynamicModel, ContentProcessorContext context, DynamicModelMeshPartContent part)
        {
            if (VertexBufferType != DynamicModelContent.BufferType.Default)
            {
                // Replace the default VertexBufferContent with CpuAnimatedVertexBufferContent.
                CpuAnimatedVertexBufferContent vb;
                if (!_vertexBufferCache.TryGetValue(part.VertexBuffer, out vb))
                {
                    vb = new CpuAnimatedVertexBufferContent(part.VertexBuffer);
                    vb.IsWriteOnly = (VertexBufferType == DynamicModelContent.BufferType.DynamicWriteOnly);
                    _vertexBufferCache[part.VertexBuffer] = vb;
                }
                part.VertexBuffer = vb;
            }
        }
        
    }
}