using UnityEngine;

namespace VinhLB
{
    public abstract class BaseController : MonoBehaviour
    {
        public bool IsControllable { get; protected set; } = true;
        public bool InUse { get; protected set; } = false;

        public virtual void SetControl(bool value)
        {
            IsControllable = value;
        }

        public static bool IsAnyControllerInUse(BaseController[] controllers)
        {
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].InUse)
                {
                    return true;
                }
            }

            return false;
        }
    }
}