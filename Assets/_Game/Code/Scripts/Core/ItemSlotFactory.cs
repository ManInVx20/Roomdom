using System.Collections.Generic;
using UnityEngine;

namespace VinhLB
{
    public class ItemSlotFactory : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private List<TargetSlot> _targetSlotList;
        [SerializeField]
        private List<QueueSlot> _queueSlotList;
        [SerializeField]
        private List<RoomItem> _roomItemList;

        public void Initialize()
        {
            for (int i = 0; i < _targetSlotList.Count; i++)
            {
                _targetSlotList[i].Initialize();
            }

            for (int i = 0; i < _queueSlotList.Count; i++)
            {
                _queueSlotList[i].Initialize();
            }

            for (int i = 0; i < _roomItemList.Count; i++)
            {
                _roomItemList[i].Initialize(this);
            }
        }

        public void MoveItemToSuitableSlot(RoomItem item)
        {
            if (item.TargetSlot != null && item.TargetSlot.IsAvailable)
            {
                item.MoveToSlot(item.TargetSlot);
            }
            else
            {
                if (TryGetAvailableQueueSlot(out QueueSlot queueSlot))
                {
                    item.MoveToSlot(queueSlot);
                }
                else
                {
                    Debug.Log("No slot is available");
                }
            }
        }

        private bool TryGetAvailableQueueSlot(out QueueSlot queueSlot)
        {
            queueSlot = null;

            for (int i = 0; i < _queueSlotList.Count; i++)
            {
                if (_queueSlotList[i].IsAvailable)
                {
                    queueSlot = _queueSlotList[i];

                    return true;
                }
            }

            return false;
        }
    }
}