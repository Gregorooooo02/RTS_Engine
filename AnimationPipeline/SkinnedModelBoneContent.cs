using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Animation;

namespace AnimationPipeline
{
    public class SkinnedModelBoneContent
    {
        private readonly ushort index;
        private readonly string name;
        private SkinnedModelBoneContent parent;
        private SkinnedModelBoneContentCollection children;

        private readonly Pose bindPose;
        private readonly Matrix inverseBindPoseTransform;

        #region Properties

        public string Name
        {
            get { return name; }
        }

        public ushort Index
        {
            get { return index; }
        }

        public SkinnedModelBoneContent Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        public SkinnedModelBoneContentCollection Children
        {
            get { return children; }
            internal set { children = value; }
        }

        public Pose BindPose
        {
            get { return bindPose; }
        }

        public Matrix InverseBindPoseTransform
        {
            get { return inverseBindPoseTransform; }
        }

        #endregion

        internal SkinnedModelBoneContent(ushort index, string name, Pose bindPose,
            Matrix inverseBindPoseTransform)
        {
            this.index = index;
            this.name = name;
            this.bindPose = bindPose;
            this.inverseBindPoseTransform = inverseBindPoseTransform;
        }

        internal void Write(ContentWriter output)
        {
            output.Write(index);
            output.Write(name);

            // Write bind pose
            output.Write(bindPose.Translation);
            output.Write(bindPose.Orientation);
            output.Write(bindPose.Scale);

            output.Write(inverseBindPoseTransform);

            // Write parent and children
            output.WriteSharedResource(parent);
            output.Write(children.Count);
            foreach (SkinnedModelBoneContent childBone in children)
                output.WriteSharedResource(childBone);
        }
    }
}