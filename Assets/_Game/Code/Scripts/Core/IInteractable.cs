using UnityEngine;

namespace VinhLB
{
    public interface IInteractable
    {
        bool IsInteractable { get; set; }

        void Interact();
    }
}