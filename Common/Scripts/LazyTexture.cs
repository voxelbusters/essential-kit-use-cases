using System;
using UnityEngine;

namespace VoxelBusters.UseCases
{
    public class LazyTexture
    {        
        public Action<Texture> OnTextureLoaded;

        public LazyTexture()
        {
        }

        public void SetTexture(Texture texture)
        {
            OnTextureLoaded?.Invoke(texture);
        }
    }
}
