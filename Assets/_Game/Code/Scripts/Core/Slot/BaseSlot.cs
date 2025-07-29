using System.Collections.Generic;
using UnityEngine;

namespace VinhLB
{
    public abstract class BaseSlot : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField]
        [Min(1)]
        private int _maxItems = 1;
        [SerializeField]
        private List<BaseItem> _currentItemList;
        [SerializeField]
        private Transform _iconTf;

        public int MaxItems => _maxItems;
        public List<BaseItem> CurrentItemList => _currentItemList;
        public Transform IconTf => _iconTf;
        public bool IsLocked { get; set; }
        public bool IsFull { get; private set; }
        public int AvailableSpaces => _maxItems - CurrentItemList.Count;
        public bool IsAvailable => !IsLocked && AvailableSpaces > 0;

        public virtual void Initialize()
        {
            // _iconTf.gameObject.SetActive(true);
        }

        public virtual void SetMaxItems(int value)
        {
            _maxItems = value;
        }

        public virtual void UpdateFullState(int availableSpaces, bool withAnim)
        {
            IsFull = availableSpaces == 0;

            // _iconTf.gameObject.SetActive(!IsFull);
        }

        public virtual void AddItem(BaseItem item)
        {
            if (item == null)
            {
                return;
            }

            _currentItemList.Add(item);
        }

        public virtual void RemoveItem(BaseItem item)
        {
            if (item == null)
            {
                return;
            }

            _currentItemList.Remove(item);
        }
    }
}