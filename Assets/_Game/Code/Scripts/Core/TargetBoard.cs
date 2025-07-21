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
        [SerializeField]
        private TargetSlot _targetSlotPrefab;

        private bool _isLocked;

        public List<TargetSlot> TargetSlotList { get; private set; }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetLocked(value);
        }

        public void Initialize(int numSlotToSpawn)
        {
            TargetSlotList = new List<TargetSlot>();
            for (int i = 0; i < numSlotToSpawn; i++)
            {
                TargetSlot slot = Instantiate(_targetSlotPrefab, _targetSlotHolderTf);

                TargetSlotList.Add(slot);
            }
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