using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class InGameScreen : UIScreen
    {
        [SerializeField]
        private Slider _zoomSlider;

        private CameraZoomController _cameraZoomController;

        private void OnDestroy()
        {
            _cameraZoomController.CurrentZoomSizeChanged -= CameraZoomController_CurrentZoomSizeChanged;

            _zoomSlider.onValueChanged.RemoveListener(ZoomSlider_onValueChanged);
        }

        public void Initialize(CameraZoomController cameraZoomController)
        {
            Initialize();

            _cameraZoomController = cameraZoomController;

            _zoomSlider.minValue = _cameraZoomController.MinZoomSize;
            _zoomSlider.maxValue = _cameraZoomController.MaxZoomSize;

            _zoomSlider.onValueChanged.AddListener(ZoomSlider_onValueChanged);

            _cameraZoomController.CurrentZoomSizeChanged += CameraZoomController_CurrentZoomSizeChanged;
        }

        private void CameraZoomController_CurrentZoomSizeChanged(float value)
        {
            _zoomSlider.SetValueWithoutNotify(value);
        }

        private void ZoomSlider_onValueChanged(float value)
        {
            _cameraZoomController.SetCurrentZoomSize(value, false);
        }
    }
}