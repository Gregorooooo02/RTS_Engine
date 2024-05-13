using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AnimationPipeline.Animation
{
    public class AnimationsContent
    {
        public List<Matrix> BindPose { get; private set; }
        public List<Matrix> InvBindPose { get; private set; }
        public List<int> SkeletonHierarchy { get; private set; }
        public List<string> BoneNames { get; private set; }
        public Dictionary<string, ClipContent> Clips { get; private set; }


        internal AnimationsContent(List<Matrix> bindPose, List<Matrix> invBindPose, List<int> skeletonHierarchy, List<string> boneNames, Dictionary<string, ClipContent> clips)
        {
            BindPose = bindPose;
            InvBindPose = invBindPose;
            SkeletonHierarchy = skeletonHierarchy;
            BoneNames = boneNames;
            Clips = clips;
        }
    }
}