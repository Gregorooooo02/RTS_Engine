namespace Animation.Controllers
{
    /// <summary>
    /// Defines an interface for blendable animations.
    /// </summary>
    public interface IBlendable
    {
        /// <summary>
        /// Gets the local pose of all skeleton's bones in depth-first order.
        /// </summary>
        Pose[] LocalBonePoses { get; }

        /// <summary>
        /// Gets or sets the blend weight.
        /// The blend weight must be a positive value between 0 and 1.
        /// </summary>
        float BlendWeight { get; set; }
    }
}