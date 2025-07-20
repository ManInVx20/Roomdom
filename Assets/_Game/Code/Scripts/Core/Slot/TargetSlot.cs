using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class TargetSlot : BaseSlot
    {
        [Header("Specific Settings")]
        [SerializeField]
        private Image _checkmarkImage;

        public override void Initialize()
        {
            base.Initialize();

            _checkmarkImage.gameObject.SetActive(false);
        }

        public override void Finish()
        {
            base.Finish();

            _checkmarkImage.gameObject.SetActive(true);
        }
    }
}