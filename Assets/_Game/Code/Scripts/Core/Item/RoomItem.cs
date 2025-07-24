using DG.Tweening;
using EPOOutline;
using UnityEngine;
using UnityEngine.EventSystems;
using VinhLB.Utilities;

namespace VinhLB
{
    public class RoomItem : BaseItem, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Specific Settings")]
        [SerializeField]
        private TargetSlot[] _targetSlots;
        [SerializeField]
        private RoomItem[] _dependentItems;

        [Space]
        [SerializeField]
        private MeshRenderer[] _meshRenderers;
        [SerializeField]
        private Collider[] _colliders;
        [SerializeField]
        private Transform _modelTf;
        [SerializeField]
        private Outlinable _outlinable;
        [SerializeField]
        private Animator _inPlaceAnimator;

        [Space]
        [SerializeField]
        private float _pickUpOffset = 1f;
        [SerializeField]
        private Vector3 _finalEulerAngles = Vector3.zero;

        private const float CLICK_SENSITIVITY = 10f;
        private const float SIZE_FACTOR = 100f;

        private Vector2 _initialPointerPosition;
        private Tween _shakingTween;

        public event System.Action<RoomItem> Clicked;
        public event System.Action<RoomItem> ReachedSlot;

        public TargetSlot[] TargetSlots => _targetSlots;
        public RoomItem[] DependentItems => _dependentItems;
        public Transform ModelTf => _modelTf;
        public Outlinable Outlinable => _outlinable;
        public Animator InPlaceAnimator => _inPlaceAnimator;
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

        public void OnPointerDown(PointerEventData eventData)
        {
            _initialPointerPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            float distance = Vector2.Distance(eventData.position, _initialPointerPosition);
            // Debug.Log($"Item touch distance: {distance}");
            // Debug.Log($"Touch count: {Input.touchCount}");
            bool canClick = Input.touchCount == 1 && distance < CLICK_SENSITIVITY;
            eventData.eligibleForClick = canClick;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log(eventData.eligibleForClick);
            if (!CanClick())
            {
                return;
            }

            Interact();
        }

        public void MoveToSlot(BaseSlot slot, bool fromWorld)
        {
            _modelTf.gameObject.SetLayerInChildren(VLBLayer.Default);

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
                _modelTf.gameObject.SetLayerInChildren(VLBLayer.InGameUI);

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

        public void SetTargetSlots(TargetSlot[] targetSlots)
        {
            _targetSlots = targetSlots;
        }

        public bool TryGetAvailableTargetSlot(out TargetSlot targetSlot)
        {
            for (int i = 0; i < _targetSlots.Length; i++)
            {
                if (_targetSlots[i] != null && _targetSlots[i].IsAvailable)
                {
                    targetSlot = _targetSlots[i];

                    return true;
                }
            }

            targetSlot = null;
            
            return false;
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

#if UNITY_EDITOR
        [ContextMenu(nameof(CollectComponents))]
        public void CollectComponents()
        {
            _meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
            _colliders = transform.GetComponentsInChildren<Collider>();
            _modelTf = transform.GetChild(0);
            _outlinable = transform.GetComponentInChildren<Outlinable>();
        }
#endif
    }
}