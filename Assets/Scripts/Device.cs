using UnityEngine;
using System.Collections;

namespace UnityAvatar
{
    public struct Resolution
    {
        public int Width;
        public int Height;

        public Resolution(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }

    public abstract class Device : MonoBehaviour
    {
        public Texture Texture;
        public Resolution Resolution;
        public abstract IEnumerator Init();
    }    
}