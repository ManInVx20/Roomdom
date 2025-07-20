using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using VinhLB.Utilities;

namespace VinhLB
{
    public class RoomItem : BaseItem, IPointerClickHandler
    {
        [Header("Specific Settings")]
        [SerializeField]
        private float _pickUpOffset = 1f;

        public event System.Action Clicked;
        public event System.Action ReachedSlot;

        public override void Interact()
        {
            base.Interact();

            Factory.MoveItemToSuitableSlot(this);

            Clicked?.Invoke();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!CanClick())
            {
                return;
            }

            Interact();
        }

        public void MoveToSlot(BaseSlot slot)
        {
            // Outlinable.enabled = true;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMoveY(transform.position.y + _pickUpOffset, 0.375f)
                .SetEase(Ease.OutSine));
            sequence.AppendCallback(() =>
            {
                transform.SetParent(slot.transform, true);
                gameObject.layer = VLBLayer.GameUI;

                // Outlinable.enabled = false;
            });

            float duration = 0.75f;
            Vector3 targetPosition = slot.IconTf.localPosition;
            Vector3 targetEulerAngles = slot.IconTf.localEulerAngles;
            Vector3 targetScale = slot.IconTf.localScale;
            sequence.Append(transform.DOLocalMove(targetPosition, duration)
                .SetEase(Ease.OutSine));
            sequence.Join(transform.DOLocalRotate(targetEulerAngles, duration)
                .SetEase(Ease.OutSine));
            sequence.Join(transform.DOScale(targetScale, duration)
                .SetEase(Ease.OutSine));
            sequence.OnComplete(() =>
            {
                slot.Finish();

                ReachedSlot?.Invoke();
            });
        }

        private bool CanClick()
        {
            return IsInteractable && !VLBInput.IsPointerOnUI();
        }
    }
}