using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class TargetSlot : BaseSlot
    {
        [Header("Specific Settings")]
        [SerializeField]
        private MeshFilter _iconMeshFilter;
        [SerializeField]
        private MeshRenderer _iconMeshRenderer;
        [SerializeField]
        private Transform _amountContentTf;
        [SerializeField]
        private TMP_Text _amountText;
        [SerializeField]
        private Image _checkmarkImage;

        public override void UpdateFullState(int availableSpaces, bool withAnim)
        {
            base.UpdateFullState(availableSpaces, withAnim);

            _amountText.gameObject.SetActive(!IsFull);
            _checkmarkImage.gameObject.SetActive(IsFull);

            UpdateAmountText(availableSpaces, withAnim);
        }

        public void Initialize(RoomItem item, int amount)
        {
            Initialize();

            SetMaxItems(amount);

            UpdateFullState(AvailableSpaces, false);

            if (item != null)
            {
                MeshRenderer mainMeshRenderer = item.GetMainRenderer();
                // Debug.Log($"{item.name} | {mainMeshRenderer}");
                if (mainMeshRenderer != null)
                {
                    _iconMeshFilter.sharedMesh = mainMeshRenderer.GetComponent<MeshFilter>()?.sharedMesh;
                    _iconMeshRenderer.sharedMaterials = mainMeshRenderer.sharedMaterials;

                    IconTf.localEulerAngles = item.ModelFinalEulerAngles;
                    IconTf.localScale = item.GetFinalScale(mainMeshRenderer);
                }
            }
            else
            {
                Debug.LogError("No item found");
            }
        }

        private void UpdateAmountText(int value, bool withAnim)
        {
            _amountText.text = value.ToString();

            if (withAnim)
            {
                _amountContentTf.DOComplete();
                _amountContentTf.DOPunchScale(Vector3.one * 0.5f, 0.25f, 1, 0);
            }
        }
    }
}