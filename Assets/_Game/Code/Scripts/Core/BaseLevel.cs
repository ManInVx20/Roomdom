using System;
using UnityEngine;

namespace VinhLB
{
    public abstract class BaseLevel : MonoBehaviour
    {
        [Serializable]
        protected class Config
        {
            public CameraRotationController cameraRotationController;
            public CameraZoomController cameraZoomController;
        }

        [Header("Base Settings")]
        [SerializeField]
        protected Config _config;

        protected virtual void Start()
        {
            _config.cameraRotationController.ResetRotation(true);
            _config.cameraRotationController.SetControl(true, true);

            _config.cameraZoomController.ResetZoom(true);
            _config.cameraZoomController.SetControl(true, true);
        }
    }
}