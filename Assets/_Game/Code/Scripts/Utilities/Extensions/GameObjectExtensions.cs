using UnityEngine;

namespace VinhLB.Utilities
{
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var attachedComponent))
            {
                attachedComponent = gameObject.AddComponent<T>();
            }

            return attachedComponent;
        }
        
        public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T result)
        {
            result = gameObject.GetComponentInChildren<T>();
            
            return result is not null;
        }

        public static bool IsInCullingMask(this GameObject gameObject, LayerMask cullingMask)
        {
            return (cullingMask & (1 << gameObject.layer)) != 0;
        }

        public static void SetLayerInChildren(this GameObject gameObject, int layer, bool recursive = false)
        {
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = layer;

                if (recursive)
                {
                    SetLayerInChildren(child.gameObject, layer, true);
                }
            }
        }
    }
}