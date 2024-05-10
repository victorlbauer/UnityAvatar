using UnityEngine;
using System.Collections;

namespace UnityAvatar
{
    public abstract class Device : MonoBehaviour
    {
        public struct Resolution
        {
            public int width;
            public int height;

            public Resolution(int width, int height)
            {
                this.width = width;
                this.height = height;
            }
        }

        public Texture texture;
        public Resolution resolution;

        public abstract IEnumerator Init();
    }    
}