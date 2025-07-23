using System;
using UnityEngine;
using UnityEngine.EventSystems;
using VinhLB.Utilities;

namespace VinhLB
{
    public abstract class BaseLevel : MonoSingleton<BaseLevel>
    {
        [Serializable]
        protected class Config
        {
            public CameraRotationController CameraRotationController;
            public CameraZoomController CameraZoomController;
            public PhysicsRaycaster MainCameraPhysicsRaycaster;

            public ItemSlotFactory ItemSlotFactory;
        }

        [Header("Base Settings")]
        [SerializeField]
        protected Config _config;

        protected override void Awake()
        {
            base.Awake();

            Application.targetFrameRate = 60;
        }

        protected virtual void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // _config.CameraRotationController.StartedInUse += CameraController_StartedInUse;
            // _config.CameraRotationController.EndedInUse += CameraController_EndedInUse;
            //
            // _config.CameraZoomController.StartedInUse += CameraController_StartedInUse;
            // _config.CameraZoomController.EndedInUse += CameraController_EndedInUse;

            _config.CameraRotationController.ResetRotation(true);
            _config.CameraRotationController.SetControl(true, true);

            _config.CameraZoomController.ResetZoom(true);
            _config.CameraZoomController.SetControl(true, true);

            _config.ItemSlotFactory.Initialize();
        }

        private void CameraController_StartedInUse(BaseController controller)
        {
            Debug.Log($"{controller.GetType().Name} Started");
            _config.MainCameraPhysicsRaycaster.enabled = false;
        }

        private void CameraController_EndedInUse(BaseController controller)
        {
            Debug.Log($"{controller.GetType().Name} Ended");
            _config.MainCameraPhysicsRaycaster.enabled = true;
        }
    }
}