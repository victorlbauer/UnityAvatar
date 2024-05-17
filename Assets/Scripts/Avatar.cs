using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityAvatar
{
    public class Avatar : MonoBehaviour
    {
        public class Joint
        {
            public Vector3 LandmarkPosition;
            public Vector3 LastLandmarkPosition;

            public Transform Transform = null;
            public Quaternion InitRotation;
            public Quaternion Inverse;

            public Joint Child = null;
        }

        [Header("FBX Model")]
        [SerializeField] private GameObject prefab;

        private GameObject avatar;
        private Animator anim;
        private Joint[] joints;

        public Transform Transform => this.avatar.transform;

        public void Init()
        {
            var spawn = GameObject.Find("Stage/Spawn");
            this.avatar = Instantiate(this.prefab, spawn.transform.position, Quaternion.identity);
            this.anim = this.avatar.GetComponent<Animator>();

            // Hips act as the anchor point
            this.avatar.transform.position = this.anim.GetBoneTransform(HumanBodyBones.Hips).position;
           
            GetJoints();
        }

        public void UpdateJoints(ref List<MediapipeContext.Landmark> landmarks)
        {
            GetLandmarkWorldPosition(ref landmarks);
            
            Joint hips = this.joints[(int)MediapipeJoints.LandmarkId.Hips];
            Joint leftHip = this.joints[(int)MediapipeJoints.LandmarkId.LeftHip];
            Joint rightHip = this.joints[(int)MediapipeJoints.LandmarkId.RightHip];
            Vector3 forward = GetNormal(hips.LandmarkPosition, leftHip.LandmarkPosition, rightHip.LandmarkPosition);

            hips.Transform.position = hips.LandmarkPosition + this.avatar.transform.position;
            hips.Transform.rotation = Quaternion.LookRotation(forward) * hips.Inverse * hips.InitRotation;

            foreach(Joint joint in this.joints)
            {
                if(joint.Transform is not null)
                    joint.Transform.position = this.avatar.transform.position + Vector3.Scale(joint.LandmarkPosition, this.avatar.transform.localScale);

                if(joint.Child is not null)
                    joint.Transform.rotation = Quaternion.LookRotation(joint.LandmarkPosition - joint.Child.LandmarkPosition, forward) * joint.Inverse * joint.InitRotation;
            }

            // Head rotation
            Joint leftEar = this.joints[(int)MediapipeJoints.LandmarkId.LeftEar];
            Joint rightEar = this.joints[(int)MediapipeJoints.LandmarkId.RightEar];
            Joint nose = this.joints[(int)MediapipeJoints.LandmarkId.Nose];
            Joint head = this.joints[(int)MediapipeJoints.LandmarkId.Head];

            Vector3 gaze = Vector3.Normalize(nose.LandmarkPosition - (0.5f * leftEar.LandmarkPosition + 0.5f * rightEar.LandmarkPosition));
            head.Transform.rotation = Quaternion.LookRotation(gaze) * head.Inverse * head.InitRotation;
        }

        private void GetJoints()
        {
            var nJoints = Enum.GetValues(typeof(MediapipeJoints.LandmarkId)).Length;
            this.joints = new Joint[nJoints];

            for(int i = 0; i < nJoints; i++)
                this.joints[i] = new Joint();

            foreach(var entry in MediapipeJoints.LandmarkToBoneMap)
            {
                var i = (int)entry.Key;
                this.joints[i].Transform = this.anim.GetBoneTransform(entry.Value.bone);
                this.joints[i].InitRotation = this.joints[i].Transform.rotation;
            }

            // Body orientation
            Joint head = this.joints[(int)MediapipeJoints.LandmarkId.Head];
            Joint hips = this.joints[(int)MediapipeJoints.LandmarkId.Hips];
            Joint leftHip = this.joints[(int)MediapipeJoints.LandmarkId.LeftHip];
            Joint rightHip = this.joints[(int)MediapipeJoints.LandmarkId.RightHip];

            Vector3 forward = GetNormal(hips.Transform.position, leftHip.Transform.position, rightHip.Transform.position);

            head.Inverse = GetInverse(forward);
            hips.Inverse = GetInverse(forward);

            foreach(var entry in MediapipeJoints.LandmarkToBoneMap)
            {
                var i = (int)entry.Key;
                if(entry.Value.child is not MediapipeJoints.LandmarkId.None)
                {
                    this.joints[i].Child = this.joints[(int)entry.Value.child];
                    this.joints[i].Inverse = GetInverse(this.joints[i], this.joints[i].Child, forward);
                }
            }
        }

        private void GetLandmarkWorldPosition(ref List<MediapipeContext.Landmark> landmarks)
        {
            for(int i = 0; i < MediapipeContext.LandmarkCount; ++i)
                this.joints[i].LandmarkPosition = landmarks[i].Position;

            // Not given by Mediapipe, we need to manually calculate it
            Joint leftHip = this.joints[(int)MediapipeJoints.LandmarkId.LeftHip];
            Joint rightHip = this.joints[(int)MediapipeJoints.LandmarkId.RightHip];
            Joint leftShoulder = this.joints[(int)MediapipeJoints.LandmarkId.LeftShoulder];
            Joint rightShoulder = this.joints[(int)MediapipeJoints.LandmarkId.RightShoulder];

            Vector3 midPointBottom = 0.5f * leftHip.LandmarkPosition + 0.5f * rightHip.LandmarkPosition;
            Vector3 midPointUpper = 0.5f * leftShoulder.LandmarkPosition + 0.5f * rightShoulder.LandmarkPosition;

            // TODO: improve estimations

            // Hips
            this.joints[(int)MediapipeJoints.LandmarkId.Hips].LandmarkPosition = Lerp(midPointBottom, midPointUpper, 0.05f);

            // Spine
            this.joints[(int)MediapipeJoints.LandmarkId.Spine].LandmarkPosition = Lerp(midPointBottom, midPointUpper, 0.25f);
            
            // Neck
            this.joints[(int)MediapipeJoints.LandmarkId.Neck].LandmarkPosition = Lerp(midPointBottom, midPointUpper, 1.05f);
            
            // Head
            this.joints[(int)MediapipeJoints.LandmarkId.Head].LandmarkPosition = Lerp(this.joints[(int)MediapipeJoints.LandmarkId.Neck].LandmarkPosition, Vector3.up, 0.15f);
        
            // Lerp
            foreach(Joint joint in this.joints)
            {
                joint.LandmarkPosition = Lerp(joint.LandmarkPosition, joint.LastLandmarkPosition, 0.5f);
                joint.LastLandmarkPosition = joint.LandmarkPosition;
            }
        }

        private Vector3 GetNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 v0v1 = v0 - v1;
            Vector3 v0v2 = v0 - v2;
            Vector3 normal = Vector3.Cross(v0v1, v0v2);
            normal.Normalize();

            return normal;
        }

        private Quaternion GetInverse(Joint from, Joint to, Vector3 forward)
        {
            return Quaternion.Inverse(Quaternion.LookRotation(from.Transform.position - to.Transform.position, forward));
        }

        private Quaternion GetInverse(Vector3 forward)
        {
            return Quaternion.Inverse(Quaternion.LookRotation(forward));
        }

        private Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return a + (b - a) * t;
        }
    }
}