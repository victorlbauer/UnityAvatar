using System.Collections;
using UnityEngine;
using System.Linq;

namespace UnityAvatar
{
    public class Webcam : Device
    {
        private WebCamTexture webCamTexture;

        public override IEnumerator Init()
        {
            if(WebCamTexture.devices.Length == 0)
                throw new System.Exception("No webcam devices were found.");

            var device = WebCamTexture.devices.First();

            this.texture = new WebCamTexture();
            this.webCamTexture = this.texture as WebCamTexture;
            this.webCamTexture.Play();

            yield return new WaitUntil(() => this.webCamTexture.width > 16);

            this.resolution = new Resolution(this.webCamTexture.width, this.webCamTexture.height);
        }

        private void OnDestroy()
        {
            if(this.texture is not null)
                this.webCamTexture.Stop();
        }
    }
}