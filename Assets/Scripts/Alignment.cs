using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAvatar
{
    class Alignment : MonoBehaviour
    {
        private new Camera camera;
        private Vector3[] source;
        private Vector3[] target;

        public void Init()
        {
            this.camera = GameObject.Find("Camera").GetComponent<Camera>();
            this.source = new Vector3[MediapipeContext.LandmarkCount];
            this.target = new Vector3[MediapipeContext.LandmarkCount];
        }

        public void AlignToScreen(ref Avatar avatar, ref List<MediapipeContext.Landmark> worldLandmarks, ref List<MediapipeContext.Landmark> landmarks)
        {
            ToCameraSpace(ref avatar, ref worldLandmarks, ref landmarks );
            ScaleToTarget(ref avatar);
            TranslateToTarget(ref avatar);
        }

        private void ToCameraSpace(ref Avatar avatar, ref List<MediapipeContext.Landmark> worldLandmarks, ref List<MediapipeContext.Landmark> landmarks)
        {
            for(int i = 0; i < MediapipeContext.LandmarkCount; ++i)
            {
                Vector3 landmarkWorldPos = avatar.Transform.position + Vector3.Scale(worldLandmarks[i].Position, avatar.Transform.localScale);
                this.source[i] = this.camera.WorldToScreenPoint(landmarkWorldPos);

                Vector3 landmarkPos = landmarks[i].Position;
                this.target[i] = LandmarkToScreenPoint(landmarkPos);
            }
        }

        private Vector3 LandmarkToScreenPoint(Vector3 pos)
        {
            // Mediapipe landmarks start from the screen's top-left most position
            return new Vector3(Screen.width * pos.x, Screen.height - (Screen.height * pos.y), pos.z);
        }

        private void ScaleToTarget(ref Avatar avatar)
        {
            Vector3 sourceHipsPos = 0.5f * source[(int)MediapipeJoints.LandmarkId.LeftHip] + 0.5f * source[(int)MediapipeJoints.LandmarkId.RightHip];
            Vector3 targetHipsPos = 0.5f * target[(int)MediapipeJoints.LandmarkId.LeftHip] + 0.5f * target[(int)MediapipeJoints.LandmarkId.RightHip];

            float sourceScaling = 1.0f;
            float targetScaling = 1.0f;

            for(int i = 0; i < MediapipeContext.LandmarkCount; ++i)
            {
                sourceScaling += Vector3.Distance(this.source[i], sourceHipsPos) / MediapipeContext.LandmarkCount;
                targetScaling += Vector3.Distance(this.target[i], targetHipsPos) / MediapipeContext.LandmarkCount;
            }

            avatar.Transform.localScale *= (targetScaling / sourceScaling);
        }

        private void TranslateToTarget(ref Avatar avatar)
        {
            Vector3 hipsLandmarkPos = 0.5f * target[(int)MediapipeJoints.LandmarkId.LeftHip] + 0.5f * target[(int)MediapipeJoints.LandmarkId.RightHip];
            Vector3 hipsLandmarkWorldPos = this.camera.ScreenToWorldPoint(hipsLandmarkPos);

            avatar.Transform.position = new Vector3(hipsLandmarkWorldPos.x, hipsLandmarkWorldPos.y, avatar.Transform.position.z);  
        }   
    }
}