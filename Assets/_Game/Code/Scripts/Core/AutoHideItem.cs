using System;
using UnityEngine;

namespace VinhLB
{
    public class AutoHideItem : MonoBehaviour
    {
        [SerializeField]
        private RoomItem[] _dependentItems;
        [SerializeField]
        private Vector3 _moveDirection = Vector3.up;
        [SerializeField]
        private float _moveSpeed = 6f;
        [SerializeField]
        private float _moveDuration = 3f;

        private bool _canMove;
        private float _moveTimer;

        private void Update()
        {
            if (!_canMove)
            {
                return;
            }

            _moveTimer += Time.deltaTime;
            if (_moveTimer < _moveDuration)
            {
                transform.Translate(_moveDirection * (_moveSpeed * Time.deltaTime));
            }
            else
            {
                Hide();

                _canMove = false;
            }
        }

        public void Initialize()
        {
            for (int i = 0; i < _dependentItems.Length; i++)
            {
                _dependentItems[i].ReachedSlot += DependentItem_ReachedSlot;
            }
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        private void DependentItem_ReachedSlot(RoomItem item)
        {
            item.ReachedSlot -= DependentItem_ReachedSlot;

            for (int i = 0; i < _dependentItems.Length; i++)
            {
                if (_dependentItems[i].CurrentSlot == null)
                {
                    return;
                }
            }

            _canMove = true;
        }
    }
}