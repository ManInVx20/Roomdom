using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using VinhLB.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VinhLB
{
    public class ItemSlotFactory : MonoBehaviour
    {
        [System.Serializable]
        private class RoomItemConfig
        {
            public List<RoomItem> ItemList;
        }

        [System.Serializable]
        private class SimilarItemConfig
        {
            public List<RoomItem> ItemList;
        }

        [Header("Settings")]
        [SerializeField]
        private Transform _targetBoardHolderTf;
        [SerializeField]
        private TargetBoard _targetBoardPrefab;
        [SerializeField]
        private TargetSlot _targetSlotPrefab;
        [SerializeField]
        private List<QueueSlot> _queueSlotList;

        [Space]
        [SerializeField]
        private RoomItem _roomItemPrefab;
        [SerializeField]
        private Transform _roomItemHolderTf;
        [SerializeField]
        private List<Transform> _modelRefList;

        [Space]
        [SerializeField]
        private List<RoomItemConfig> _roomItemConfigList;
        [SerializeField]
        private List<SimilarItemConfig> _similarItemConfigList;

        private const int MAX_SLOTS_PER_BOARD = 1;
        // private const int SLOTS_TO_SHOW = 2;
        private const int SLOTS_TO_UNLOCK = 2;

        private List<TargetBoard> _targetBoardList = new();
        private int _firstLockedTargetBoardIndex;
        // private int _secondLockedTargetBoardIndex;

        public event System.Action RunOutOfQueueSlot;
        public event System.Action HadAllTargetSlotsFull;

        public void Initialize()
        {
            List<RoomItem> randomRoomItemList = new();
            for (int i = 0; i < _roomItemConfigList.Count; i++)
            {
                randomRoomItemList.AddRange(_roomItemConfigList[i].ItemList);
            }

            while (randomRoomItemList.Count > 0)
            {
                int maxSlots = randomRoomItemList.Count / MAX_SLOTS_PER_BOARD > 0 
                    ? MAX_SLOTS_PER_BOARD
                    : randomRoomItemList.Count % MAX_SLOTS_PER_BOARD;

                TargetBoard board = Instantiate(_targetBoardPrefab, _targetBoardHolderTf);
                board.Initialize(_targetSlotPrefab, maxSlots);

                for (int i = 0; i < maxSlots; i++)
                {
                    if (TryGetSimilarItemConfig(randomRoomItemList[0], out SimilarItemConfig config))
                    {
                        board.TargetSlotList[i].Initialize(randomRoomItemList[0], config.ItemList.Count);

                        for (int j = 0; j < config.ItemList.Count; j++)
                        {
                            config.ItemList[j].Initialize(this);
                            config.ItemList[j].SetTargetSlots(new TargetSlot[] { board.TargetSlotList[i] });

                            randomRoomItemList.Remove(config.ItemList[j]);
                        }
                    }
                    else
                    {
                        board.TargetSlotList[i].Initialize(randomRoomItemList[0], 1);

                        randomRoomItemList[0].Initialize(this);
                        randomRoomItemList[0].SetTargetSlots(new TargetSlot[] { board.TargetSlotList[i] });

                        randomRoomItemList.RemoveAt(0);
                    }
                }

                board.IsLocked = true;

                _targetBoardList.Add(board);
            }

            // SetupSimilarItemConfig();

            for (int i = 0; i < _queueSlotList.Count; i++)
            {
                _queueSlotList[i].Initialize();
            }

            SetupTargetBoards();
        }

        public bool TryMoveItemToSuitableSlot(RoomItem item)
        {
            if (!IsAnyQueueSlotAvailable())
            {
                return false;
            }

            if (item.TryGetAvailableTargetSlot(out TargetSlot targetSlot))
            {
                item.CurrentSlot = targetSlot;
                targetSlot.AddItem(item);

                item.ReachedSlot += CheckUnlockedTargetBoard;
                item.MoveToSlot(targetSlot, true);

                return true;
            }

            if (TryGetAvailableQueueSlot(out QueueSlot queueSlot))
            {
                item.CurrentSlot = queueSlot;
                queueSlot.AddItem(item);

                item.ReachedSlot += CheckAvailableQueueSlot;
                item.MoveToSlot(queueSlot, true);

                return true;
            }

            return false;
        }

        private void SetupSimilarItemConfig()
        {
            for (int i = 0; i < _similarItemConfigList.Count; i++)
            {
                List<TargetSlot> targetSlotList = new();
                for (int j = 0; j < _similarItemConfigList[i].ItemList.Count; j++)
                {
                    targetSlotList.Add(_similarItemConfigList[i].ItemList[j].TargetSlots[0]);
                }
                
                for (int j = 0; j < _similarItemConfigList[i].ItemList.Count; j++)
                {
                    _similarItemConfigList[i].ItemList[j].SetTargetSlots(targetSlotList.ToArray());
                }
            }
        }

        private void SetupTargetBoards()
        {
            for (int i = 0; i < _targetBoardList.Count; i++)
            {
                _targetBoardList[i].IsLocked = i >= SLOTS_TO_UNLOCK;
                _targetBoardList[i].gameObject.SetActive(i < SLOTS_TO_UNLOCK);
            }

            _firstLockedTargetBoardIndex = SLOTS_TO_UNLOCK;
            // _secondLockedTargetBoardIndex = SLOTS_TO_SHOW;
        }

        private bool TryGetSimilarItemConfig(RoomItem item, out SimilarItemConfig config)
        {
            for (int i = 0; i < _similarItemConfigList.Count; i++)
            {
                if (_similarItemConfigList[i].ItemList.Contains(item))
                {
                    config = _similarItemConfigList[i];

                    return true;
                }
            }

            config = null;

            return false;
        }

        private bool IsAnyQueueSlotAvailable()
        {
            for (int i = 0; i < _queueSlotList.Count; i++)
            {
                if (_queueSlotList[i].IsAvailable)
                {
                    return true;
                }
            }

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

        private void MoveQueueItemsToTargetSlots()
        {
            for (int i = 0; i < _queueSlotList.Count; i++)
            {
                if (!_queueSlotList[i].IsFull)
                {
                    continue;
                }

                RoomItem queueRoomItem = _queueSlotList[i].CurrentItemList[0] as RoomItem;
                if (queueRoomItem == null)
                {
                    continue;
                }

                MoveQueueItemToTargetSlot(queueRoomItem);
            }
        }

        private void MoveQueueItemToTargetSlot(RoomItem item)
        {
            if (item.TryGetAvailableTargetSlot(out TargetSlot targetSlot))
            {
                item.CurrentSlot.RemoveItem(item);
                item.CurrentSlot.UpdateFullState(item.CurrentSlot.AvailableSpaces, true);

                item.CurrentSlot = targetSlot;
                targetSlot.AddItem(item);

                item.ReachedSlot += CheckUnlockedTargetBoard;
                item.MoveToSlot(targetSlot, false);
            }
        }

        private void CheckAvailableQueueSlot(RoomItem item)
        {
            item.ReachedSlot -= CheckAvailableQueueSlot;

            if (IsAnyQueueSlotAvailable())
            {
                MoveQueueItemToTargetSlot(item);
            }
            else
            {
                // Debug.Log("No available queue slot");
                RunOutOfQueueSlot?.Invoke();
            }
        }

        private void CheckUnlockedTargetBoard(RoomItem item)
        {
            item.ReachedSlot -= CheckUnlockedTargetBoard;

            TargetBoard board = GetTargetBoard((TargetSlot)item.CurrentSlot, out int index);
            if (!board.AreAllSlotsFull())
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
                int nextUnlockedBoardIndex = _firstLockedTargetBoardIndex;
                TargetBoard nextUnlockedBoard = _targetBoardList[nextUnlockedBoardIndex];
                // TargetBoard nextVisibleLockedBoard = null;
                // if (_secondLockedTargetBoardIndex > _firstLockedTargetBoardIndex
                //     && _secondLockedTargetBoardIndex < _targetBoardList.Count)
                // {
                //     nextVisibleLockedBoard = _targetBoardList[_secondLockedTargetBoardIndex];
                //
                //     _secondLockedTargetBoardIndex += 1;
                // }

                (_targetBoardList[index], _targetBoardList[nextUnlockedBoardIndex]) = 
                    (_targetBoardList[nextUnlockedBoardIndex], _targetBoardList[index]);

                _firstLockedTargetBoardIndex += 1;

                sequence.AppendCallback(() =>
                {
                    board.transform.SetSiblingIndex(nextUnlockedBoardIndex);
                    nextUnlockedBoard.transform.SetSiblingIndex(index);

                    nextUnlockedBoard.gameObject.SetActive(true);
                    // nextVisibleLockedBoard?.gameObject.SetActive(true);
                });
                sequence.AppendInterval(0.5f);
                sequence.AppendCallback(() =>
                {
                    nextUnlockedBoard.IsLocked = false;
                });
                sequence.AppendInterval(0.5f);
                sequence.OnComplete(MoveQueueItemsToTargetSlots);
            }
            else if (AreAllTargetSlotsFull())
            {
                sequence.OnComplete(() =>
                {
                    // Debug.Log("End");
                    HadAllTargetSlotsFull?.Invoke();
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

        private bool AreAllTargetSlotsFull()
        {
            for (int i = 0; i < _targetBoardList.Count; i++)
            {
                if (!_targetBoardList[i].AreAllSlotsFull())
                {
                    return false;
                }
            }

            return true;
        }

        #region Utilities Methods

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
        
        #endregion

#if UNITY_EDITOR
        [ContextMenu(nameof(CreateItems))]
        private void CreateItems()
        {
            for (int i = 0; i < _modelRefList.Count; i++)
            {
                RoomItem item = PrefabUtility.InstantiatePrefab(_roomItemPrefab, _roomItemHolderTf) as RoomItem;
                if (item == null)
                {
                    continue;
                }

                item.gameObject.name = $"Item ({i + 1}) ({_modelRefList[i].gameObject.name})";
                item.transform.position = _modelRefList[i].position;

                Transform itemModelTf = Instantiate(_modelRefList[i], item.ModelTf);
                itemModelTf.gameObject.name = _modelRefList[i].gameObject.name;
                itemModelTf.gameObject.layer = VLBLayer.Interactable;
                itemModelTf.gameObject.SetLayerInChildren(VLBLayer.Interactable);
                itemModelTf.localPosition = Vector3.zero;

                item.CollectComponents();

                Undo.RegisterCreatedObjectUndo(item, "Create RoomItem");
            }
        }

        [ContextMenu(nameof(DeleteItems))]
        private void DeleteItems()
        {
            CollectItemsForFirstConfig();

            while (_roomItemConfigList[0].ItemList.Count > 0)
            {
                if (_roomItemConfigList[0].ItemList[0] != null)
                {
                    Undo.DestroyObjectImmediate(_roomItemConfigList[0].ItemList[0]);
                }

                _roomItemConfigList[0].ItemList.RemoveAt(0);
            }

            _roomItemConfigList.Clear();
        }

        [ContextMenu(nameof(CollectItemsForFirstConfig))]
        private void CollectItemsForFirstConfig()
        {
            _roomItemConfigList.Clear();

            RoomItemConfig riConfig = new()
            {
                ItemList = FindObjectsByType<RoomItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList()
            };

            _roomItemConfigList.Add(riConfig);
        }

        [ContextMenu(nameof(SortItems))]
        private void SortItems()
        {
            for (int i = 0; i < _roomItemConfigList.Count; i++)
            {
                _roomItemConfigList[i].ItemList = GetSortedItemListByDependentDepth(_roomItemConfigList[i].ItemList);
            }
        }

        [ContextMenu(nameof(ShowModelRefs))]
        private void ShowModelRefs()
        {
            for (int i = 0; i < _modelRefList.Count; i++)
            {
                _modelRefList[i].gameObject.SetActive(true);
                EditorUtility.SetDirty(_modelRefList[i]);
            }
        }

        [ContextMenu(nameof(HideModelRefs))]
        private void HideModelRefs()
        {
            for (int i = 0; i < _modelRefList.Count; i++)
            {
                _modelRefList[i].gameObject.SetActive(false);
                EditorUtility.SetDirty(_modelRefList[i]);
            }
        }
#endif
    }
}