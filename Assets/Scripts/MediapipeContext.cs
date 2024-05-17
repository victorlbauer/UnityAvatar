using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe;
using Mediapipe.Unity;

namespace UnityAvatar
{
    using Stopwatch = System.Diagnostics.Stopwatch;

    public class SidePacket
    {   
        private PacketMap packet;
        public PacketMap Content => this.packet;

        public SidePacket()
        {
            // TODO: make this configurable
            this.packet = new PacketMap();
            
            this.packet.Emplace("input_rotation", Packet.CreateInt(0));
            this.packet.Emplace("input_horizontally_flipped", Packet.CreateBool(false));
            this.packet.Emplace("input_vertically_flipped", Packet.CreateBool(true));
            this.packet.Emplace("output_rotation", Packet.CreateInt(0));
            this.packet.Emplace("output_horizontally_flipped", Packet.CreateBool(false));
            this.packet.Emplace("output_vertically_flipped", Packet.CreateBool(false));
            this.packet.Emplace("model_complexity", Packet.CreateInt(2));
            this.packet.Emplace("smooth_landmarks", Packet.CreateBool(true));
            this.packet.Emplace("enable_segmentation", Packet.CreateBool(true));
            this.packet.Emplace("smooth_segmentation", Packet.CreateBool(true));
        }

        public static implicit operator PacketMap(SidePacket packet) => packet.Content;
    }

    public class MediapipeContext : MonoBehaviour
    {
        public struct Landmark
        {
            public Vector3 Position;
            public Landmark(Vector3 position) => Position = position;
        }

        public const int LandmarkCount = 33;
        public bool IsReady = false;

        public List<Landmark> Landmarks;
        public List<Landmark> WorldLandmarks;

        private CalculatorGraph graph;
        private ResourceManager resourceManager;

        private Color32[] inputBuffer;
        private Texture2D input;

        private OutputStream<NormalizedLandmarkList> poseLandmarkStream;
        private OutputStream<LandmarkList> poseWorldLandmarkStream;

        public IEnumerator Init(Device device)
        {
            var cwd = Directory.GetCurrentDirectory();
            //var configFilePath = File.ReadAllText(Path.Combine(cwd, "Assets/cpuconfig.txt"));
            var configFilePath = File.ReadAllText(Path.Combine(cwd, "Assets/gpuconfig.txt"));
            this.graph = new CalculatorGraph(configFilePath);

            this.inputBuffer = new Color32[device.Resolution.Width * device.Resolution.Height];
            this.input = new Texture2D(device.Resolution.Width, device.Resolution.Height, TextureFormat.RGBA32, false);

            this.poseLandmarkStream = new OutputStream<NormalizedLandmarkList>(this.graph, "pose_landmarks");
            this.poseWorldLandmarkStream = new OutputStream<LandmarkList>(this.graph, "pose_world_landmarks");
            this.poseLandmarkStream.StartPolling();
            this.poseWorldLandmarkStream.StartPolling();

            this.Landmarks = new List<Landmark>(new Landmark[LandmarkCount]);
            this.WorldLandmarks = new List<Landmark>(new Landmark[LandmarkCount]);

            this.resourceManager = new StreamingAssetsResourceManager();
            yield return this.resourceManager.PrepareAssetAsync("pose_detection.bytes");
            yield return this.resourceManager.PrepareAssetAsync("pose_landmark_heavy.bytes");
            yield return this.resourceManager.PrepareAssetAsync("pose_landmarker_heavy.bytes");
        }

        public IEnumerator Run(Texture inputTexture)
        {
            var sidePacket = new SidePacket();
            this.graph.StartRun(sidePacket);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while(true)
            {
                var timestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
                yield return StartCoroutine(SetGraphInput(timestamp, inputTexture));
                yield return StartCoroutine(GetLandmarks());
                this.IsReady = true;
            }
        }

        private void OnDestroy()
        {
            this.poseLandmarkStream?.Dispose();
            this.poseWorldLandmarkStream?.Dispose();
            this.poseLandmarkStream = null;
            this.poseWorldLandmarkStream = null;

            if(this.graph is not null)
            {
                try
                {
                    this.graph.CloseInputStream("input_video");
                    this.graph.WaitUntilDone();
                }
                finally
                {
                    this.graph.Dispose();
                    this.graph = null;
                }
            }
        }

        private IEnumerator SetGraphInput(Int64 timestamp, Texture inputTexture)
        {
            var inputTextureRef = inputTexture as WebCamTexture;
            this.input.SetPixels32(inputTextureRef.GetPixels32(this.inputBuffer));

            // ImageFrame format (Mediapipe specification)
            var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, this.input.width, this.input.height, this.input.width * 4, this.input.GetRawTextureData<byte>());

            this.graph.AddPacketToInputStream("input_video", Packet.CreateImageFrameAt(imageFrame, timestamp));
            yield return new WaitForEndOfFrame();
        }
        
        private IEnumerator GetLandmarks()
        {
            // Normalized landmarks
            {
                var task = this.poseLandmarkStream.WaitNextAsync();
                yield return new WaitUntil(() => task.IsCompleted);

                if(task.Result.ok is false)
                    throw new Exception("Could not fetch data from the pose landmark stream.");

                var packet = task.Result.packet;
                if(packet is not null)
                {
                    var landmarkList = packet.Get(NormalizedLandmarkList.Parser);
                    SetLandmarks(landmarkList);
                }
            }

            // World landmarks
            {
                var task = this.poseWorldLandmarkStream.WaitNextAsync();
                yield return new WaitUntil(() => task.IsCompleted);

                if(task.Result.ok is false)
                    throw new Exception("Could not fetch data from the pose world landmark stream.");

                var packet = task.Result.packet;
                if(packet is not null)
                {
                    var landmarkList = packet.Get(LandmarkList.Parser);
                    SetLandmarks(landmarkList);
                }
            }
        }

        private void SetLandmarks(NormalizedLandmarkList landmarkList)
        {
            for(int i = 0; i < LandmarkCount; i++)
            {
                var landmark = new Landmark(GetLandmarkPosition(landmarkList.Landmark[i]));
                this.Landmarks[i] = landmark;
            }
        }

        private void SetLandmarks(LandmarkList landmarkList)
        {
            for(int i = 0; i < LandmarkCount; i++)
            {
                var landmark = new Landmark(GetLandmarkPosition(landmarkList.Landmark[i]));
                this.WorldLandmarks[i] = landmark;
            }
        }

        private Vector3 GetLandmarkPosition(Mediapipe.NormalizedLandmark landmark)
        {
            return new Vector3(landmark.X, landmark.Y, landmark.Z);
        }

        private Vector3 GetLandmarkPosition(Mediapipe.Landmark landmark)
        {
            return new Vector3(landmark.X, -landmark.Y, landmark.Z);
        }
    }
}