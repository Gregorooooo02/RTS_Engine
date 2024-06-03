using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS.Animation
{    
    public class Animations
    {
        public List<Matrix> _bindPose;
        public List<Matrix> _invBindPose;
        public List<int> _skeletonHierarchy;
        public Dictionary<string, int> _boneMap;

        public Matrix[] _boneTransforms;
        public Matrix[] _worldTransforms;
        public Matrix[] _animationTransforms;

        private int _currentKeyframe;


        public Dictionary<string, Clip> Clips { get; set; }
        public Clip CurrentClip { get; set; }
        public TimeSpan CurrentTime { get; set; }

        /// <summary>
        /// The current bone transform matrices, relative to their parent bones.
        /// </summary>
        public Matrix[] BoneTransforms { get { return _boneTransforms; } }

        /// <summary>
        /// The current bone transform matrices, in absolute format.
        /// </summary>
        public Matrix[] WorldTransforms { get { return _worldTransforms; } }

        /// <summary>
        /// The current bone transform matrices, relative to the animation bind pose.
        /// </summary>
        public Matrix[] AnimationTransforms { get { return _animationTransforms; } }


        public Animations(List<Matrix> bindPose, List<Matrix> invBindPose, List<int> skeletonHierarchy, Dictionary<string, int> boneMap, Dictionary<string, Clip> clips)
        {
            _bindPose = bindPose;
            _invBindPose = invBindPose;
            _skeletonHierarchy = skeletonHierarchy;
            _boneMap = boneMap;
            Clips = clips;
            
            // initialize
            _boneTransforms = new Matrix[_bindPose.Count];
            _worldTransforms = new Matrix[_bindPose.Count];
            _animationTransforms = new Matrix[_bindPose.Count];
        }

        public void SetClip(string clipName)
        {
            Clip clip = Clips["Base Stack"];
            SetClip(clip);
        }

        public void SetClip(Clip clip)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");

            CurrentClip = clip;
            CurrentTime = TimeSpan.Zero;
            _currentKeyframe = 0;

            // Initialize bone transforms to the bind pose.
            _bindPose.CopyTo(_boneTransforms, 0);
        }
        
        public int GetBoneIndex(string boneName)
        {
            int boneIndex;
            if (!_boneMap.TryGetValue(boneName, out boneIndex))
                boneIndex = -1;
            return boneIndex;
        }

        public void Update(TimeSpan time, bool relativeToCurrentTime, Matrix rootTransform)
        {
            UpdateBoneTransforms(time, relativeToCurrentTime);
            UpdateWorldTransforms(rootTransform);
            UpdateAnimationTransforms();
        }

        public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
        {
            // Update the animation position.
            if (relativeToCurrentTime)
            {
                time += CurrentTime;

                // If we reached the end, loop back to the start.
                while (time >= CurrentClip.Duration)
                    time -= CurrentClip.Duration;
            }

            if (time < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("time out of range");
            if (time > CurrentClip.Duration)
                //throw new ArgumentOutOfRangeException("time out of range");
                time = CurrentClip.Duration;

            // If the position moved backwards, reset the keyframe index.
            if (time < CurrentTime)
            {
                _currentKeyframe = 0;
                _bindPose.CopyTo(_boneTransforms, 0);
            }

            CurrentTime = time;

            // Read keyframe matrices.
            IList<Keyframe> keyframes = CurrentClip.Keyframes;

            while (_currentKeyframe < keyframes.Count)
            {
                Keyframe keyframe = keyframes[_currentKeyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > CurrentTime)
                    break;

                // Use this keyframe.
                _boneTransforms[keyframe.Bone] = keyframe.Transform;

                _currentKeyframe++;
            }
        }

        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            // Root bone.
            Matrix.Multiply(ref _boneTransforms[0], ref rootTransform, out _worldTransforms[0]);

            // Child bones.
            for (int bone = 1; bone < _worldTransforms.Length; bone++)
            {
                int parentBone = _skeletonHierarchy[bone];

                Matrix.Multiply(ref _boneTransforms[bone], ref _worldTransforms[parentBone], out _worldTransforms[bone]);
            }
        }

        public void UpdateAnimationTransforms()
        {
            for (int bone = 0; bone < _animationTransforms.Length; bone++)
            {
                Matrix _tmpInvBindPose = _invBindPose[bone]; //can not pass it as 'ref'
                Matrix.Multiply(ref _tmpInvBindPose, ref _worldTransforms[bone], out _animationTransforms[bone]);
            }
        }
    }
}
