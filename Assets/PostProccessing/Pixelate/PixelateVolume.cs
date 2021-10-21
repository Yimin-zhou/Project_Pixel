using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("Post-processing/Pixelate")]
    public sealed class Pixelate : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")]
        public ClampedIntParameter pixelatePower = new ClampedIntParameter(5,1,124);
        public bool IsActive() => pixelatePower.value > 0f;

        public bool IsTileCompatible() => false;
    }
}