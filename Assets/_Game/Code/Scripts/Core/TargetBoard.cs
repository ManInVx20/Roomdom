using System.Collections.Generic;
using UnityEngine;

namespace VinhLB
{
    public class TargetBoard : MonoBehaviour
    {
        [SerializeField]
        private Transform _lockTf;
        [SerializeField]
        private Transform _targetSlotHolderTf;

        private bool _isLocked;

        public List<TargetSlot> TargetSlotList { get; private set; }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetLocked(value);
        }

        public void Initialize(TargetSlot targetSlotPrefab, int numSlotToSpawn)
        {
            TargetSlotList = new List<TargetSlot>();
            for (int i = 0; i < numSlotToSpawn; i++)
            {
                TargetSlot slot = Instantiate(targetSlotPrefab, _targetSlotHolderTf);

                TargetSlotList.Add(slot);
            }
        }

        public bool IsAllSlotsFull()
        {
            for (int i = 0; i < TargetSlotList.Count; i++)
            {
                if (!TargetSlotList[i].IsFull)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetLocked(bool value)
        {
            _isLocked = value;

            for (int i = 0; i < TargetSlotList.Count; i++)
            {
                TargetSlotList[i].IsLocked = value;
            }

            _lockTf.gameObject.SetActive(value);
        }
    }
}