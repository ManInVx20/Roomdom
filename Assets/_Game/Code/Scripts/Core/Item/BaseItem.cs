using UnityEngine;

namespace VinhLB
{
    public abstract class BaseItem : MonoBehaviour, IInteractable
    {
        [Header("Base Settings")]
        [SerializeField]
        private BaseSlot _currentSlot;

        public BaseSlot CurrentSlot
        {
            get => _currentSlot;
            set => _currentSlot = value;
        }
        public bool IsInteractable { get; set; } = true;
        public ItemSlotFactory Factory { get; private set; }

        public virtual void Initialize(ItemSlotFactory factory)
        {
            Factory = factory;
        }

        public virtual void Interact()
        {
            Debug.Log($"{gameObject.name} is interacted");
        }
    }
}