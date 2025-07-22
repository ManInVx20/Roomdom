using System;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public abstract class BaseSlot : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField]
        private BaseItem _currentItem;
        [SerializeField]
        private Transform _iconTf;

        public BaseItem CurrentItem
        {
            get => _currentItem;
            set => _currentItem = value;
        }
        public Transform IconTf => _iconTf;
        public bool IsLocked { get; set; }
        public bool IsFull { get; private set; }
        public bool IsAvailable => !IsLocked && CurrentItem == null;

        public virtual void Initialize()
        {
            _iconTf.gameObject.SetActive(true);
        }

        public virtual void SetFullState(bool value)
        {
            _iconTf.gameObject.SetActive(!value);
            
            IsFull = value;
        }
    }
}