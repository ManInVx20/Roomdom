using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public class UIScreen : MonoBehaviour
    {
        private Canvas _canvas;

        public Canvas Canvas
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = GetComponent<Canvas>();
                }
                
                return _canvas;
            }
        }

        public virtual void Initialize()
        {
            
        }
    }
}