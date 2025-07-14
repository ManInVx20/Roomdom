using UnityEngine;

namespace VinhLB.Utilities
{
    public static class TransformExtensions
    {
        public static void Reset(this Transform transform, Space space = Space.Self)
        {
            switch (space)
            {
                case Space.Self:
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    break;

                case Space.World:
                    transform.position = Vector3.zero;
                    transform.rotation = Quaternion.identity;
                    break;
            }

            transform.localScale = Vector3.one;
        }
    }
}