using UnityEngine;
using UnityEngine.EventSystems;
using VinhLB.Utilities;

namespace VinhLB
{
    public abstract class BaseLevel : MonoSingleton<BaseLevel>
    {
        [System.Serializable]
        protected class Config
        {
            public CameraRotationController CameraRotationController;
            public CameraZoomController CameraZoomController;
            public PhysicsRaycaster MainCameraPhysicsRaycaster;

            public ItemSlotFactory ItemSlotFactory;

            public bool HighFramerate;
        }

        [Header("Base Settings")]
        [SerializeField]
        protected Config _config;
        [SerializeField]
        protected AutoHideItem[] _autoHideItems;

        protected override void Awake()
        {
            base.Awake();

            UpdateFramerate();
        }

        protected virtual void OnValidate()
        {
            UpdateFramerate();
        }

        protected virtual void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            UIManager.Instance.GetScreen<InGameScreen>().Initialize(_config.CameraZoomController);

            // _config.CameraRotationController.StartedInUse += CameraController_StartedInUse;
            // _config.CameraRotationController.EndedInUse += CameraController_EndedInUse;
            //
            // _config.CameraZoomController.StartedInUse += CameraController_StartedInUse;
            // _config.CameraZoomController.EndedInUse += CameraController_EndedInUse;

            _config.CameraRotationController.ResetRotation(true);
            _config.CameraRotationController.SetControl(true, true);
            _config.CameraRotationController.StartRotateAround();

            _config.CameraZoomController.ResetZoom(true);
            _config.CameraZoomController.SetControl(true, true);

            _config.ItemSlotFactory.Initialize();
            _config.ItemSlotFactory.RunOutOfQueueSlot += () => GameOver(false);
            _config.ItemSlotFactory.HadAllTargetSlotsFull += () => GameOver(true);

            SetupAutoHideItems();
        }

        private void UpdateFramerate()
        {
            if (!_config.HighFramerate)
            {
                Application.targetFrameRate = 60;
            }
            else
            {
                Application.targetFrameRate = 120;
            }
        }

        private void SetupAutoHideItems()
        {
            for (int i = 0; i < _autoHideItems.Length; i++)
            {
                _autoHideItems[i].Initialize();
            }
        }

        private void GameOver(bool won)
        {
            GameOverScreen gameOverScreen = UIManager.Instance.GetScreen<GameOverScreen>();
            gameOverScreen.Initialize(won);
            gameOverScreen.Show();
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