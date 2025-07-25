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
        private Image _checkmarkImage;

        public override void Initialize()
        {
            base.Initialize();

            _checkmarkImage.gameObject.SetActive(false);
        }

        public override void SetFullState(bool value)
        {
            base.SetFullState(value);

            _checkmarkImage.gameObject.SetActive(value);
        }

        public void Initialize(RoomItem item)
        {
            Initialize();

            if (item != null)
            {
                MeshRenderer mainMeshRenderer = item.GetMainRenderer();
                // Debug.Log($"{item.name} | {mainMeshRenderer}");
                if (mainMeshRenderer != null)
                {
                    _iconMeshFilter.sharedMesh = mainMeshRenderer.GetComponent<MeshFilter>()?.sharedMesh;

                    IconTf.localEulerAngles = item.ModelFinalEulerAngles;
                    IconTf.localScale = item.GetFinalScale(mainMeshRenderer);
                }
            }
        }
    }
}