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

            this.Texture = new WebCamTexture();
            this.webCamTexture = this.Texture as WebCamTexture;
            this.webCamTexture.Play();

            yield return new WaitUntil(() => this.webCamTexture.width > 16);

            this.Resolution = new Resolution(this.webCamTexture.width, this.webCamTexture.height);
        }

        private void OnDestroy()
        {
            if(this.Texture is not null)
                this.webCamTexture.Stop();
        }
    }
}