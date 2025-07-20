using System;
using DG.Tweening;
using EPOOutline;
using UnityEngine;
using VinhLB.Utilities;

namespace VinhLB
{
    public abstract class BaseItem : MonoBehaviour, IInteractable
    {
        [Header("Base Settings")]
        [SerializeField]
        private TargetSlot _targetSlot;
        [SerializeField]
        private Outlinable _outlinable;

        public TargetSlot TargetSlot
        {
            get => _targetSlot;
            set => _targetSlot = value;
        }
        public Outlinable Outlinable => _outlinable;
        public bool IsInteractable { get; set; } = true;
        public ItemSlotFactory Factory { get; private set; }

        public virtual void Initialize(ItemSlotFactory factory)
        {
            Factory = factory;
            
            // _outlinable.enabled = false;
        }

        public virtual void Interact()
        {
            Debug.Log($"{gameObject.name} is interacted");

            IsInteractable = false;
        }
    }
}