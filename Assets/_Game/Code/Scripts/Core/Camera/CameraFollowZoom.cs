using System;
using EmeraldPowder.CameraScaler;
using UnityEngine;

namespace VinhLB
{
    public class CameraFollowZoom : MonoBehaviour
    {
        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private Camera _cameraToFollow;

        private void LateUpdate()
        {
            _camera.orthographicSize = _cameraToFollow.orthographicSize;
        }
    }
}