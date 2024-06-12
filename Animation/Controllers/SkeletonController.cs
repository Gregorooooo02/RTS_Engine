namespace Animation.Controllers
{
    /// <summary>
    /// Controls the pose of each bone in a skeleton. Allows custom skeleton poses to be blended 
    /// with other <see cref="T:XNAnimation.Controllers.IBlendable" /> objects.
    /// </summary>
    public class SkeletonController : ISkeletonController, IBlendable
    {
        private SkinnedModelBoneDictionary skeletonDictionary;
        private Pose[] localBonePoses;
        private float blendWeight;

        public Pose[] LocalBonePoses
        {
            get { return localBonePoses; }
        }

        public float BlendWeight
        {
            get { return blendWeight; }
            set { blendWeight = value; }
        }

        /// <summary>Initializes a new instance of the 
        /// <see cref="T:XNAnimation.Controllers.SkeletonController"></see>
        /// class.
        /// </summary>
        /// <param name="skeletonDictionary"></param>
        public SkeletonController(SkinnedModelBoneDictionary skeletonDictionary)
        {
            this.skeletonDictionary = skeletonDictionary;
            localBonePoses = new Pose[skeletonDictionary.Count];

            blendWeight = 1.0f;
        }

        /// <inheritdoc />
        public void SetBonePose(string channelName, ref Pose pose)
        {
            localBonePoses[skeletonDictionary[channelName].Index] = pose;
        }

        /// <inheritdoc />
        public void SetBonePose(string channelName, Pose pose)
        {
            localBonePoses[skeletonDictionary[channelName].Index] = pose;
        }
    }
}