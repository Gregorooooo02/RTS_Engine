using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;

namespace AnimationImporters.Processors
{
    [ContentProcessor(DisplayName = "GPU AnimatedModel - Custom")]
    public class GpuAnimatedModelProcessor : ModelProcessor
    {
        private int _maxBones = SkinnedEffect.MaxBones;
        private int _generateKeyframesFrequency = 0;
        private bool _fixRealBoneRoot = false;

#if !PORTABLE
        [DisplayName("MaxBones")]
#endif
        [DefaultValue(SkinnedEffect.MaxBones)]
        public virtual int MaxBones 
        {
            get { return _maxBones; }
            set { _maxBones = value; }
        }

#if !PORTABLE
        [DisplayName("Generate Keyframes Frequency")]
#endif
        [DefaultValue(0)] // (0=no, 30=30fps, 60=60fps)
        public virtual int GenerateKeyframesFrequency
        {
            get { return _generateKeyframesFrequency; }
            set { _generateKeyframesFrequency = value; }
        }

#if !PORTABLE
        [DisplayName("Fix BoneRoot from MG importer")]
#endif
        [DefaultValue(false)]
        public virtual bool FixRealBoneRoot
        {
            get { return _fixRealBoneRoot; }
            set { _fixRealBoneRoot = value; }
        }

        [DefaultValue(MaterialProcessorDefaultEffect.SkinnedEffect)]
        public override MaterialProcessorDefaultEffect DefaultEffect
        {
            get { return base.DefaultEffect; }
            set { base.DefaultEffect = value; }
        }
        
        public GpuAnimatedModelProcessor()
        {
            DefaultEffect = MaterialProcessorDefaultEffect.SkinnedEffect;
        }

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            var animationProcessor = new AnimationsProcessor();
            animationProcessor.MaxBones = this.MaxBones;   
            animationProcessor.GenerateKeyframesFrequency = this.GenerateKeyframesFrequency;
            animationProcessor.FixRealBoneRoot = this._fixRealBoneRoot;
            var animation = animationProcessor.Process(input, context);
            
            ModelContent model = base.Process(input, context);
            model.Tag = animation;
            return model;
        }
    }
}
