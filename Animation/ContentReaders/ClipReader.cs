﻿using System;
using RTS.Animation;
using Microsoft.Xna.Framework.Content;

namespace Animation.ContentReaders
{
    public class ClipReader : ContentTypeReader<Clip>
    {
        protected override Clip Read(ContentReader input, Clip existingInstance)
        {
            Clip animationClip = existingInstance;

            if (existingInstance == null)
            {
                TimeSpan duration = ReadDuration(input);
                Keyframe[] keyframes = ReadKeyframes(input, null);
                animationClip = new Clip(duration, keyframes);
            }
            else
            {
                animationClip.Duration = ReadDuration(input);
                ReadKeyframes(input, animationClip.Keyframes);
            }

            return animationClip;                       
        }
        
        private TimeSpan ReadDuration(ContentReader input)
        {
            return new TimeSpan(input.ReadInt64());
        }

        private Keyframe[] ReadKeyframes(ContentReader input, Keyframe[] existingInstance)
        {
            Keyframe[] keyframes = existingInstance;

            int count = input.ReadInt32();
            if (keyframes == null)
                keyframes = new Keyframe[count];
            
            for (int i = 0; i < count; i++)
            {
                keyframes[i]._bone = input.ReadInt32();
                keyframes[i]._time = new TimeSpan(input.ReadInt64());
                keyframes[i]._transform.M11 = input.ReadSingle();
                keyframes[i]._transform.M12 = input.ReadSingle();
                keyframes[i]._transform.M13 = input.ReadSingle();
                keyframes[i]._transform.M14 = 0;
                keyframes[i]._transform.M21 = input.ReadSingle();
                keyframes[i]._transform.M22 = input.ReadSingle();
                keyframes[i]._transform.M23 = input.ReadSingle();
                keyframes[i]._transform.M24 = 0;
                keyframes[i]._transform.M31 = input.ReadSingle();
                keyframes[i]._transform.M32 = input.ReadSingle();
                keyframes[i]._transform.M33 = input.ReadSingle();
                keyframes[i]._transform.M34 = 0;
                keyframes[i]._transform.M41 = input.ReadSingle();
                keyframes[i]._transform.M42 = input.ReadSingle();
                keyframes[i]._transform.M43 = input.ReadSingle();
                keyframes[i]._transform.M44 = 1;
            }

            return keyframes;
        }
        
    }
    
}
