using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe;

namespace UnityAvatar
{
    public enum DeviceType { Webcam }

    public class Solution : MonoBehaviour
    {
        [Header("Device")]
        [SerializeField] private DeviceType deviceType;
     
        private Device device;
        private Avatar avatar;
        private MediapipeContext mediapipeContext;

        private bool isRunning = false;

        private IEnumerator Start()
        {
            this.avatar = GetComponent<Avatar>();
            this.avatar.Init();

            yield return StartCoroutine(SelectDevice());
            yield return StartCoroutine(InitMediapipe());

            RunMediapipe();

            this.isRunning = true;
        }

        private void FixedUpdate()
        {
            if(this.isRunning)
                this.avatar.UpdateJoints(ref this.mediapipeContext.WorldLandmarks);
        }

        private IEnumerator SelectDevice()
        {
            switch(this.deviceType)
            {
                case DeviceType.Webcam:
                {
                    var obj = new GameObject("Webcam", typeof(Webcam));
                    this.device = obj.GetComponent<Webcam>();
                    break;
                }
            }

            yield return StartCoroutine(this.device.Init());

            if(this.device is not null)
            {
                RawImage deviceScreen = GameObject.Find("Viewport/DeviceScreen").GetComponent<RawImage>();

                deviceScreen.rectTransform.sizeDelta = new Vector2(this.device.Resolution.Width, this.device.Resolution.Height);
                deviceScreen.texture = this.device.Texture;
            }
        }

        private IEnumerator InitMediapipe()
        {
            var obj = new GameObject("Mediapipe Context", typeof(MediapipeContext));
            this.mediapipeContext = obj.GetComponent<MediapipeContext>();

            if(this.mediapipeContext is not null)
                yield return StartCoroutine(this.mediapipeContext.Init(this.device));
        }

        private void RunMediapipe()
        {
            StartCoroutine(this.mediapipeContext.Run(this.device.Texture));
        }
    }

}
