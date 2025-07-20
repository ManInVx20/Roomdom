using System;
using UnityEngine;

namespace VinhLB
{
    public abstract class BaseLevel : MonoBehaviour
    {
        [Serializable]
        protected class Config
        {
            public CameraRotationController CameraRotationController;
            public CameraZoomController CameraZoomController;
            public ItemSlotFactory ItemSlotFactory;
        }

        [Header("Base Settings")]
        [SerializeField]
        protected Config _config;

        protected virtual void Start()
        {
            _config.CameraRotationController.ResetRotation(true);
            _config.CameraRotationController.SetControl(true, true);

            _config.CameraZoomController.ResetZoom(true);
            _config.CameraZoomController.SetControl(true, true);

            _config.ItemSlotFactory.Initialize();
        }
    }
}