using System;
using System.Collections.Generic;

namespace RTS_Engine;

public class ModelProperties
{
    private List<int> _skeleton = new List<int>();
    private List<AnimationClip> _animationClips = new List<AnimationClip>();

    public List<int> Skeleton
    {
        get => _skeleton;
        set => _skeleton = value;
    }

    public List<AnimationClip> AnimationClips
    {
        get => _animationClips;
        set => _animationClips = value;
    }
}
