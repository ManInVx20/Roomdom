using UnityEngine;

namespace VinhLB.Utilities
{
    public static class LayerMaskExtensions
    {
        public static bool HasLayer(this LayerMask layerMask, int layer)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        public static LayerMask WithLayers(this LayerMask layerMask, params int[] layers)
        {
            foreach (var layer in layers)
            {
                layerMask |= 1 << layer;
            }

            return layerMask;
        }

        public static LayerMask WithoutLayers(this LayerMask layerMask, params int[] layers)
        {
            foreach (var layer in layers)
            {
                layerMask &= ~(1 << layer);
            }

            return layerMask;
        }
    }
}