using System;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class InGameView : MonoBehaviour
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
            _cameraZoomController = cameraZoomController;

            _zoomSlider.minValue = _cameraZoomController.MinManualZoomSize;
            _zoomSlider.maxValue = _cameraZoomController.MaxManualZoomSize;

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