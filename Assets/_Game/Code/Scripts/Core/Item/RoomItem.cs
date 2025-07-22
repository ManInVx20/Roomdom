using System;
using DG.Tweening;
using EPOOutline;
using UnityEngine;
using UnityEngine.EventSystems;
using VinhLB.Utilities;

namespace VinhLB
{
    public class RoomItem : BaseItem, IPointerClickHandler
    {
        [Header("Specific Settings")]
        [SerializeField]
        private TargetSlot _targetSlot;
        [SerializeField]
        private RoomItem[] _dependentItems;
        [SerializeField]
        private MeshRenderer[] _meshRenderers;
        [SerializeField]
        private Collider[] _colliders;
        [SerializeField]
        private Outlinable _outlinable;
        [SerializeField]
        private float _pickUpOffset = 1f;
        [SerializeField]
        private Vector3 _finalEulerAngles = Vector3.zero;

        private const float SIZE_FACTOR = 74f;

        private Tween _shakingTween;

        public event System.Action<RoomItem> Clicked;
        public event System.Action<RoomItem> ReachedSlot;

        public TargetSlot TargetSlot
        {
            get => _targetSlot;
            set => _targetSlot = value;
        }
        public RoomItem[] DependentItems => _dependentItems;
        public Outlinable Outlinable => _outlinable;
        public Vector3 FinalEulerAngles => _finalEulerAngles;

        public override void Initialize(ItemSlotFactory factory)
        {
            base.Initialize(factory);

            // _outlinable.enabled = false;
        }

        public override void Interact()
        {
            base.Interact();

            IsInteractable = false;

            if (HasAnyDependentItemAvailable() || !Factory.TryMoveItemToSuitableSlot(this))
            {
                PlayShakingAnim();

                IsInteractable = true;
            }

            Clicked?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!CanClick())
            {
                return;
            }

            Interact();
        }

        public void Initialize(ItemSlotFactory factory, TargetSlot targetSlot)
        {
            Initialize(factory);

            _targetSlot = targetSlot;
        }

        [ContextMenu(nameof(CollectComponents))]
        public void CollectComponents()
        {
            _meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
            _colliders = transform.GetComponentsInChildren<Collider>();
            _outlinable = transform.GetComponentInChildren<Outlinable>();
        }

        public void MoveToSlot(BaseSlot slot, bool fromWorld)
        {
            // _outlinable.enabled = true;

            Sequence sequence = DOTween.Sequence();
            if (fromWorld)
            {
                sequence.Append(transform.DOMoveY(transform.position.y + _pickUpOffset, 0.375f)
                    .SetEase(Ease.OutSine));
            }

            sequence.AppendCallback(() =>
            {
                transform.SetParent(slot.transform, true);
                gameObject.layer = VLBLayer.GameUI;

                // _outlinable.enabled = false;
            });
            float duration = 0.375f;
            Vector3 targetScale = Vector3.one;
            if (!fromWorld)
            {
                targetScale = transform.localScale;
                sequence.Append(transform.DOScale(targetScale * 1.2f, duration)
                    .SetEase(Ease.OutSine));
                sequence.Join(transform.DOLocalMoveZ(transform.localPosition.z - 400f, duration)
                    .SetEase(Ease.OutSine));
            }

            duration = 0.75f;
            Vector3 targetPosition = slot.IconTf.localPosition;
            sequence.Append(transform.DOLocalMove(targetPosition, duration)
                .SetEase(Ease.OutSine));
            Vector3 targetEulerAngles = _finalEulerAngles;
            sequence.Join(transform.DOLocalRotate(targetEulerAngles, duration)
                .SetEase(Ease.OutSine));
            if (fromWorld)
            {
                targetScale = GetFinalScale(GetMainRenderer());
            }
            sequence.Join(transform.DOScale(targetScale, duration)
                .SetEase(Ease.OutSine));
            sequence.OnComplete(() =>
            {
                slot.SetFullState(true);

                ReachedSlot?.Invoke(this);
            });
        }

        public Vector3 GetFinalScale(MeshRenderer mainMeshRenderer)
        {
            Vector3 finalSize = Quaternion.Euler(_finalEulerAngles) * mainMeshRenderer.bounds.size;
            float maxValue = Mathf.Max(Mathf.Abs(finalSize.x), Mathf.Abs(finalSize.y), Mathf.Abs(finalSize.z));

            return transform.localScale * SIZE_FACTOR / maxValue;
        }

        public MeshRenderer GetMainRenderer()
        {
            MeshRenderer mainMeshRenderer = null;
            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                if (mainMeshRenderer == null)
                {
                    mainMeshRenderer = _meshRenderers[i];
                }
                else
                {
                    if (_meshRenderers[i].bounds.size.sqrMagnitude > mainMeshRenderer.bounds.size.sqrMagnitude)
                    {
                        mainMeshRenderer = _meshRenderers[i];
                    }
                }
            }

            return mainMeshRenderer;
        }

        private bool CanClick()
        {
            return IsInteractable && !VLBInput.IsPointerOnUI();
        }

        private bool HasAnyDependentItemAvailable()
        {
            for (int i = 0; i < _dependentItems.Length; i++)
            {
                if (_dependentItems[i] != null && _dependentItems[i].CurrentSlot == null)
                {
                    return true;
                }
            }

            return false;
        }

        private void PlayShakingAnim()
        {
            _shakingTween?.Complete();

            _shakingTween = transform.DOShakePosition(0.5f, 0.05f, 50);
        }
    }
}