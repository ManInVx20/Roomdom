using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using VinhLB.Utilities;

namespace VinhLB
{
    public class ItemSlotFactory : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private List<TargetBoard> _targetBoardList;
        [SerializeField]
        private List<RoomItem> _roomItemList;
        [SerializeField]
        private List<QueueSlot> _queueSlotList;

        public void Initialize()
        {
            List<RoomItem> randomRoomItemList = _roomItemList.Shuffle().ToList();
            int numSlotsPerBoard = 3;
            for (int i = 0; i < _targetBoardList.Count; i++)
            {
                _targetBoardList[i].Initialize(numSlotsPerBoard);

                for (int j = 0; j < _targetBoardList[i].TargetSlotList.Count; j++)
                {
                    _targetBoardList[i].TargetSlotList[j].Initialize(randomRoomItemList[0]);

                    randomRoomItemList[0].Initialize(this, _targetBoardList[i].TargetSlotList[j]);

                    randomRoomItemList.RemoveAt(0);
                }

                _targetBoardList[i].IsLocked = true;
            }

            for (int i = 0; i < randomRoomItemList.Count; i++)
            {
                randomRoomItemList[i].Initialize(this);
            }

            for (int i = 0; i < _queueSlotList.Count; i++)
            {
                _queueSlotList[i].Initialize();
            }

            _targetBoardList[0].IsLocked = false;
        }

        public bool TryMoveItemToSuitableSlot(RoomItem item)
        {
            if (item.TargetSlot != null && item.TargetSlot.IsAvailable)
            {
                item.CurrentSlot = item.TargetSlot;
                item.TargetSlot.CurrentItem = item;

                item.ReachedSlot += CheckCurrentUnlockedTargetBoard;
                item.MoveToSlot(item.TargetSlot, true);

                return true;
            }

            if (TryGetAvailableQueueSlot(out QueueSlot queueSlot))
            {
                item.CurrentSlot = queueSlot;
                queueSlot.CurrentItem = item;

                item.MoveToSlot(queueSlot, true);

                return true;
            }

            Debug.Log("No slot is available");

            return false;
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

        private void MoveQueueItemsToAvailableTargetSlots()
        {
            for (int i = 0; i < _queueSlotList.Count; i++)
            {
                if (_queueSlotList[i].IsAvailable)
                {
                    continue;
                }

                RoomItem queueRoomItem = _queueSlotList[i].CurrentItem as RoomItem;
                if (queueRoomItem == null)
                {
                    continue;
                }

                if (queueRoomItem.TargetSlot != null && queueRoomItem.TargetSlot.IsAvailable)
                {
                    _queueSlotList[i].CurrentItem = null;
                    _queueSlotList[i].SetFullState(false);

                    queueRoomItem.CurrentSlot = queueRoomItem.TargetSlot;
                    queueRoomItem.TargetSlot.CurrentItem = queueRoomItem;

                    queueRoomItem.ReachedSlot += CheckCurrentUnlockedTargetBoard;
                    queueRoomItem.MoveToSlot(queueRoomItem.TargetSlot, false);
                }
            }
        }

        private void CheckCurrentUnlockedTargetBoard()
        {
            List<TargetSlot> targetSlotList = _targetBoardList[0].TargetSlotList;
            for (int i = 0; i < targetSlotList.Count; i++)
            {
                if (!targetSlotList[i].IsFull)
                {
                    return;
                }
            }

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(1f);
            sequence.AppendCallback(() =>
            {
                Destroy(_targetBoardList[0].gameObject);
                _targetBoardList.RemoveAt(0);
            });
            sequence.AppendInterval(0.5f);
            sequence.AppendCallback(() =>
            {
                _targetBoardList[0].IsLocked = false;
            });
            sequence.AppendInterval(0.5f);
            sequence.OnComplete(MoveQueueItemsToAvailableTargetSlots);
        }
    }
}