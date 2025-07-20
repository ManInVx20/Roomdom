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
        private CameraScaler _cameraScaler;

        private float _startZoomSize;

        private void Awake()
        {
            _startZoomSize = _camera.orthographicSize;
        }

        private void LateUpdate()
        {
            _camera.orthographicSize = _startZoomSize / _cameraScaler.CameraZoom;
        }
    }
}