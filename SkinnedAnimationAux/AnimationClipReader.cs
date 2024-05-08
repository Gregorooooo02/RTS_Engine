using System;
using Microsoft.Xna.Framework.Content;

namespace RTS_Engine;

public class AnimationClipReader : ContentTypeReader<AnimationClip>
{
    protected override AnimationClip Read(ContentReader input, AnimationClip existingInstance)
    {
        AnimationClip clip = new AnimationClip();
        clip.Name = input.ReadString();
        clip.Duration = input.ReadDouble();

        int boneCount = input.ReadInt32();

        for (int i = 0; i < boneCount; i++)
        {
            AnimationClip.Bone bone = new AnimationClip.Bone();
            clip.Bones.Add(bone);

            bone.Name = input.ReadString();

            int count = input.ReadInt32();

            for (int j = 0; j < count; j++)
            {
                AnimationClip.Keyframe keyframe = new AnimationClip.Keyframe();
                keyframe.Time = input.ReadDouble();
                keyframe.Rotation = input.ReadQuaternion();
                keyframe.Translation = input.ReadVector3();

                bone.Keyframes.Add(keyframe);
            }
        }

        return clip;   
    }
}
