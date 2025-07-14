using UnityEngine;

namespace VinhLB.Utilities
{
    public static class Vector2Extensions
    {
        public static Vector2 WithX(this Vector2 vector, float x)
        {
            return new Vector2(x, vector.y);
        }

        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector2(vector.x, y);
        }
    }
}