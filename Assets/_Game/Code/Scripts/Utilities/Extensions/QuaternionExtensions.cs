using UnityEngine;

namespace VinhLB.Utilities
{
    public static class QuaternionExtensions
    {
        public static Quaternion WithEulerX(this Quaternion quaternion, float x)
        {
            var eulerAngles = quaternion.eulerAngles;
            eulerAngles.x = x;

            return Quaternion.Euler(eulerAngles);
        }

        public static Quaternion WithEulerY(this Quaternion quaternion, float y)
        {
            var eulerAngles = quaternion.eulerAngles;
            eulerAngles.y = y;

            return Quaternion.Euler(eulerAngles);
        }

        public static Quaternion WithEulerZ(this Quaternion quaternion, float z)
        {
            var eulerAngles = quaternion.eulerAngles;
            eulerAngles.z = z;

            return Quaternion.Euler(eulerAngles);
        }
    }
}