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
        private Transform _targetBoardHolderTf;
        [SerializeField]
        private TargetBoard _targetBoardPrefab;
        [SerializeField]
        private TargetSlot _targetSlotPrefab;
        [SerializeField]
        private List<QueueSlot> _queueSlotList;
        [SerializeField]
        private List<RoomItem> _roomItemList;

        private const int MAX_SLOTS_PER_BOARD = 3;
        private const int SLOTS_TO_SHOW = 3;
        private const int SLOTS_TO_UNLOCK = 2;
        
        private List<TargetBoard> _targetBoardList;
        private int _firstLockedTargetBoardIndex;

        public void Initialize()
        {
            List<RoomItem> randomRoomItemList = GetSortedItemListByDependentDepth(_roomItemList);
            int numBoardsToSpawn = randomRoomItemList.Count / MAX_SLOTS_PER_BOARD;
            int leftOverItems = randomRoomItemList.Count - numBoardsToSpawn * MAX_SLOTS_PER_BOARD;

            _targetBoardList =  new List<TargetBoard>();
            SpawnAndInitializeSlotsAndItems(numBoardsToSpawn, MAX_SLOTS_PER_BOARD);
            if (leftOverItems > 0)
            {
                SpawnAndInitializeSlotsAndItems(1, leftOverItems);
            }

            for (int i = 0; i < _queueSlotList.Count; i++)
            {
                _queueSlotList[i].Initialize();
            }

            SetupTargetBoards();

            return;

            void SpawnAndInitializeSlotsAndItems(int numBoards, int maxSlots)
            {
                for (int i = 0; i < numBoards; i++)
                {
                    TargetBoard board = Instantiate(_targetBoardPrefab, _targetBoardHolderTf);

                    board.Initialize(_targetSlotPrefab, maxSlots);

                    for (int j = 0; j < board.TargetSlotList.Count; j++)
                    {
                        board.TargetSlotList[j].Initialize(randomRoomItemList[0]);

                        randomRoomItemList[0].Initialize(this, board.TargetSlotList[j]);

                        randomRoomItemList.RemoveAt(0);
                    }

                    board.IsLocked = true;
                    
                    _targetBoardList.Add(board);
                }
            }
        }

        public bool TryMoveItemToSuitableSlot(RoomItem item)
        {
            if (item.TargetSlot != null && item.TargetSlot.IsAvailable)
            {
                item.CurrentSlot = item.TargetSlot;
                item.TargetSlot.CurrentItem = item;

                item.ReachedSlot += CheckUnlockedTargetBoard;
                item.MoveToSlot(item.TargetSlot, true);

                return true;
            }

            if (TryGetAvailableQueueSlot(out QueueSlot queueSlot))
            {
                item.CurrentSlot = queueSlot;
                queueSlot.CurrentItem = item;

                item.ReachedSlot += MoveQueueItemToTargetSlot;
                item.MoveToSlot(queueSlot, true);

                return true;
            }

            Debug.Log("No slot is available");

            return false;
        }

        private void SetupTargetBoards()
        {
            for (int i = 0; i < _targetBoardList.Count; i++)
            {
                _targetBoardList[i].IsLocked = i >= SLOTS_TO_UNLOCK;
                _targetBoardList[i].gameObject.SetActive(i < SLOTS_TO_SHOW);
            }

            _firstLockedTargetBoardIndex = SLOTS_TO_UNLOCK;
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

        private void MoveQueueItemsToTargetSlots()
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

                MoveQueueItemToTargetSlot(queueRoomItem);
            }
        }

        private void MoveQueueItemToTargetSlot(RoomItem item)
        {
            if (item.TargetSlot != null && item.TargetSlot.IsAvailable)
            {
                item.CurrentSlot.CurrentItem = null;
                item.CurrentSlot.SetFullState(false);

                item.CurrentSlot = item.TargetSlot;
                item.TargetSlot.CurrentItem = item;

                item.ReachedSlot -= MoveQueueItemToTargetSlot;
                item.ReachedSlot += CheckUnlockedTargetBoard;
                item.MoveToSlot(item.TargetSlot, false);
            }
        }

        private void CheckUnlockedTargetBoard(RoomItem item)
        {
            TargetBoard board = GetTargetBoard((TargetSlot)item.CurrentSlot, out int index);
            if (!board.IsAllSlotsFull())
            {
                return;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(1f);
            sequence.AppendCallback(() =>
            {
                board.gameObject.SetActive(false);
            });

            if (_firstLockedTargetBoardIndex < _targetBoardList.Count)
            {
                TargetBoard nextUnlockedBoard =  _targetBoardList[_firstLockedTargetBoardIndex];
                TargetBoard nextVisibleLockedBoard = null;
                if (_firstLockedTargetBoardIndex + 1 < _targetBoardList.Count)
                {
                    nextVisibleLockedBoard = _targetBoardList[_firstLockedTargetBoardIndex + 1];
                }

                _firstLockedTargetBoardIndex += 1;
                
                sequence.AppendCallback(() =>
                {
                    // (_targetBoardList[index], _targetBoardList[_firstLockedTargetBoardIndex]) = 
                    //     (_targetBoardList[_firstLockedTargetBoardIndex], _targetBoardList[index]);
                    //
                    // _targetBoardList[_firstLockedTargetBoardIndex].transform.SetSiblingIndex(_firstLockedTargetBoardIndex);
                    // _targetBoardList[index].transform.SetSiblingIndex(index);
                    //
                    // _targetBoardList[index].gameObject.SetActive(true);

                    nextUnlockedBoard.gameObject.SetActive(true);
                    nextVisibleLockedBoard?.gameObject.SetActive(true);
                });
                sequence.AppendInterval(0.5f);
                sequence.AppendCallback(() =>
                {
                    nextUnlockedBoard.IsLocked = false;
                });
                sequence.AppendInterval(0.5f);
                sequence.OnComplete(MoveQueueItemsToTargetSlots);
            }
            else if (IsAllTargetSlotsFull())
            {
                sequence.OnComplete(() =>
                { 
                    Debug.Log("End");
                });
            }
        }

        private TargetBoard GetTargetBoard(TargetSlot slot, out int index)
        {
            for (int i = 0; i < _targetBoardList.Count; i++)
            {
                if (_targetBoardList[i].IsLocked)
                {
                    continue;
                }

                if (_targetBoardList[i].TargetSlotList.Contains(slot))
                {
                    index = i;
                    
                    return _targetBoardList[i];
                }
            }

            index = -1;

            return null;
        }

        private bool IsAllTargetSlotsFull()
        {
            for (int i = 0; i < _targetBoardList.Count; i++)
            {
                if (!_targetBoardList[i].IsAllSlotsFull())
                {
                    return false;
                }
            }

            return true;
        }

        private List<RoomItem> GetSortedItemListByDependentDepth(List<RoomItem> itemList)
        {
            List<RoomItem> sortedItemList = itemList.Shuffle().ToList();
            Dictionary<RoomItem, int> itemDependentDepthDict = new();
            for (int i = 0; i < itemList.Count; i++)
            {
                itemDependentDepthDict[itemList[i]] = GetItemDependentDepth(itemList[i]);
            }

            Dictionary<RoomItem, int> itemMaxDependenceDict = new();
            for (int i = 0; i < itemList.Count; i++)
            {
                itemMaxDependenceDict[itemList[i]] = GetItemMaxDependence(itemList[i]);
            }
            
            sortedItemList = sortedItemList
                .OrderBy(x => itemDependentDepthDict[x])
                .ThenBy(x => itemMaxDependenceDict[x])
                .ToList();

            // Debug.LogWarning($"Dict {itemList}");
            // foreach (DraggableItem item in sortedItemList)
            // {
            //     Debug.LogWarning($"{item.gameObject.name}: " +
            //                      $"{itemSlotDependentDepthDict[item]} | {itemSlotMaxDependentDict[item]}");
            // }

            return sortedItemList; 
        }

        private int GetItemDependentDepth(RoomItem item)
        {
            if (item == null)
            {
                Debug.LogError("Slot is null");
                return -1;
            }

            if (item.DependentItems == null || item.DependentItems.Length == 0)
            {
                return 0;
            }

            int depth = 0;
            for (int i = 0; i < item.DependentItems.Length; i++)
            {
                int currentDepth = 1 + GetItemDependentDepth(item.DependentItems[i]);
                if (currentDepth > depth)
                {
                    depth = currentDepth;
                }
            }

            return depth;
        }

        private int GetItemMaxDependence(RoomItem item)
        {
            if (item == null)
            {
                Debug.LogError("Slot is null");
                return -1;
            }

            if (item.DependentItems == null || item.DependentItems.Length == 0)
            {
                return 0;
            }

            int dependence = 0;
            for (int i = 0; i < item.DependentItems.Length; i++)
            {
                int currentDependence = 1 + GetItemMaxDependence(item.DependentItems[i]);
                dependence += currentDependence;
            }

            return dependence;
        }

        [ContextMenu(nameof(CollectComponents))]
        private void CollectComponents()
        {
            _queueSlotList = FindObjectsByType<QueueSlot>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
            _roomItemList = FindObjectsByType<RoomItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
        }
    }
}