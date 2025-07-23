using UnityEngine;

namespace VinhLB
{
    public abstract class BaseController : MonoBehaviour
    {
        public event System.Action<BaseController> StartedInUse;
        public event System.Action<BaseController> EndedInUse;

        public bool IsControllable { get; private set; } = true;
        public bool InUse { get; private set; } = false;

        public virtual void SetControl(bool value)
        {
            IsControllable = value;
        }

        public virtual void SetInUse(bool value)
        {
            if (InUse == value)
            {
                return;
            }

            InUse = value;

            if (value)
            {
                StartedInUse?.Invoke(this);
            }
            else
            {
                EndedInUse?.Invoke(this);
            }
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