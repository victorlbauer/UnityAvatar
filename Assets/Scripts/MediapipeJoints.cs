using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mediapipe;

namespace UnityAvatar
{
    class MediapipeJoints
    {
        public const int LandmarkCount = 33;

        public enum LandmarkId
        {
            // Pose
            Nose,
            LeftEyeInner,
            LeftEye,
            LeftEyeOuter,
            RightEyeInner,
            RightEye,
            RightEyeOuter,
            LeftEar,
            RightEar,
            MouthLeft,
            MouthRight,
            LeftShoulder,
            RightShoulder,
            LeftElbow,
            RightElbow,
            LeftWrist,
            RightWrist,
            LeftPinky,
            RightPinky,
            LeftIndex,
            RightIndex,
            LeftThumb,
            RightThumb,
            LeftHip,
            RightHip,
            LeftKnee,
            RightKnee,
            LeftAnkle,
            RightAnkle,
            LeftHeel,
            RightHeel,
            LeftFootIndex,
            RightFootIndex,

            // Not provided by Mediapipe
            Hips,
            Spine,
            Neck,
            Head,

            None
        }

        // WARNING: Not all landmarks have a bone equivalent
        public static Dictionary<LandmarkId, (HumanBodyBones bone, LandmarkId child)> LandmarkToBoneMap = new Dictionary<LandmarkId, (HumanBodyBones, LandmarkId)>()
        {
            // Left arm
            {LandmarkId.LeftShoulder, (HumanBodyBones.LeftUpperArm, LandmarkId.LeftElbow)},
            {LandmarkId.LeftElbow, (HumanBodyBones.LeftLowerArm, LandmarkId.LeftWrist)},
            {LandmarkId.LeftWrist, (HumanBodyBones.LeftHand, LandmarkId.None)},

            // Left leg
            {LandmarkId.LeftHip, (HumanBodyBones.LeftUpperLeg, LandmarkId.LeftKnee)},
            {LandmarkId.LeftKnee, (HumanBodyBones.LeftLowerLeg, LandmarkId.LeftAnkle)},
            {LandmarkId.LeftAnkle, (HumanBodyBones.LeftFoot, LandmarkId.LeftFootIndex)},
            {LandmarkId.LeftFootIndex, (HumanBodyBones.LeftToes, LandmarkId.None)},

            // Right arm
            {LandmarkId.RightShoulder, (HumanBodyBones.RightUpperArm, LandmarkId.RightElbow)},
            {LandmarkId.RightElbow, (HumanBodyBones.RightLowerArm, LandmarkId.RightWrist)},
            {LandmarkId.RightWrist, (HumanBodyBones.RightHand, LandmarkId.None)},

            // Right leg
            {LandmarkId.RightHip, (HumanBodyBones.RightUpperLeg, LandmarkId.RightKnee)},
            {LandmarkId.RightKnee, (HumanBodyBones.RightLowerLeg, LandmarkId.RightAnkle)},
            {LandmarkId.RightAnkle, (HumanBodyBones.RightFoot, LandmarkId.RightFootIndex)},
            {LandmarkId.RightFootIndex, (HumanBodyBones.RightToes, LandmarkId.None)},

            // Others
            {LandmarkId.Hips, (HumanBodyBones.Hips, LandmarkId.None)},
            {LandmarkId.Spine, (HumanBodyBones.Spine, LandmarkId.Neck)},
            {LandmarkId.Neck, (HumanBodyBones.Neck, LandmarkId.Head)},
            {LandmarkId.Head, (HumanBodyBones.Head, LandmarkId.None)}
        };
    }
}