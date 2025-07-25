using System.Collections.Generic;
using UnityEngine;
using VinhLB.Utilities;

namespace VinhLB
{
    public class UIManager : PersistentMonoSingleton<UIManager>
    {
        [SerializeField]
        private Canvas _rootCanvas;

        private List<UIScreen> _screenList = new();

        public T GetScreen<T>() where T : UIScreen
        {
            for (int i = 0; i < _screenList.Count; i++)
            {
                if (_screenList[i] is T)
                {
                    return _screenList[i] as T;
                }
            }

            T screen = _rootCanvas.GetComponentInChildren<T>();
            if (screen != null)
            {
                _screenList.Add(screen);
            }

            return screen;
        }
    }
}