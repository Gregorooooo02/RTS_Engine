namespace Animation.Controllers
{
    /// <summary>
    /// Defines an interface for an skeleton controller.
    /// </summary>
    public interface ISkeletonController
    {
        /// <summary>
        /// Gets the local pose of all skeleton's bones in depth-first order.
        /// </summary>
        Pose[] LocalBonePoses { get; }

        /// <summary>
        /// Sets a custom pose for an skeleton's bone.
        /// </summary>
        /// <param name="channelName">The name of the bone.</param>
        /// <param name="pose">The custom pose to be set.</param>
        void SetBonePose(string channelName, ref Pose pose);

        /// <summary>
        /// Sets a custom pose for an skeleton's bone.
        /// </summary>
        /// <param name="channelName">The name of the bone.</param>
        /// <param name="pose">The custom pose to be set.</param>
        void SetBonePose(string channelName, Pose pose);
    }
}