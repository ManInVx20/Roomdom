using System.Collections.Generic;
using DG.Tweening;
using EmeraldPowder.CameraScaler;
using UnityEngine;
using VinhLB.Utilities;

namespace VinhLB
{
    public class CameraZoomController : BaseController
    {
        public enum ZoomMode
        {
            In,
            Out
        }

        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private CameraScaler _cameraScaler;

        [Space(10)]
        [SerializeField]
        private LayerMask _blockLayer;
        [SerializeField]
        private BaseController[] _blockControllers;

        [Space(10)]
        [SerializeField]
        private float _zoomSpeed = 10f;
        [SerializeField]
        private float _zoomSmoothness = 6f;

        [Space(10)]
        [SerializeField]
        private float _startZoomSize = 1f;
        [SerializeField]
        private float _minZoomSize = 0.75f;
        [SerializeField]
        private float _maxZoomSize = 1.25f;

        private bool _canGetInput = true;
        private bool _canUpdate = true;
        private float _difference;
        private float _currentZoomSize;

        public float CurrentZoomSize => _currentZoomSize;
        public float MinZoomSize
        {
            get => _minZoomSize;
            set => _minZoomSize = value;
        }
        public float MaxZoomSize
        {
            get => _maxZoomSize;
            set => _maxZoomSize = value;
        }
        public float MinManualZoomSize => _minZoomSize * 0.75f;
        public float MaxManualZoomSize => _maxZoomSize * 1.75f;

        private void Update()
        {
            if (!IsControllable)
            {
                return;
            }

            if (IsAnyControllerInUse(_blockControllers))
            {
                return;
            }

            if (_canGetInput && VLBInput.IsAnyPointerDown(out List<VLBInput.TouchData> touchDataList))
            {
                for (int i = 0; i < touchDataList.Count; i++)
                {
                    if (VLBInput.IsPointerOnUI(touchDataList[i].index)
                        || VLBPhysics.TryCastRay(touchDataList[i].index, _camera, out _, float.PositiveInfinity,
                            _blockLayer))
                    {
                        _canGetInput = false;

                        break;
                    }
                }
            }

            if (_canGetInput)
            {
                if (VLBApplication.IsOnEditor())
                {
                    _difference = Input.mouseScrollDelta.y;
                }
                else if (Input.touchCount == 2)
                {
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                    _difference = (currentMagnitude - prevMagnitude) * 0.05f;
                }
                else
                {
                    _difference = 0;
                }

                if (!Mathf.Approximately(_difference, 0))
                {
                    InUse = true;
                }
                else
                {
                    InUse = false;

                    _difference = 0;
                }
            }
            else
            {
                if (VLBInput.IsAnyPointerUp(out _))
                {
                    _canGetInput = true;
                }
            }
        }

        private void LateUpdate()
        {
            if (!_canUpdate)
            {
                return;
            }

            float newZoomSize = _currentZoomSize + _difference * _zoomSpeed;
            _currentZoomSize = Mathf.Clamp(newZoomSize, MinManualZoomSize, MaxManualZoomSize);

            _cameraScaler.CameraZoom =
                Mathf.Lerp(_cameraScaler.CameraZoom, _currentZoomSize, _zoomSmoothness * Time.deltaTime);
        }

        public void SetControl(bool value, bool keepUpdating)
        {
            SetControl(value);

            InUse = false;

            _canUpdate = keepUpdating;

            _canGetInput = true;
        }

        public void SetBlockLayer(LayerMask layer)
        {
            _blockLayer = layer;
        }

        public Tween ResetZoom(bool immediately, float duration = 1f, System.Action onComplete = null)
        {
            _currentZoomSize = _startZoomSize;

            if (immediately)
            {
                _cameraScaler.CameraZoom = _currentZoomSize;

                onComplete?.Invoke();

                return null;
            }

            return DOTween.To(() => _cameraScaler.CameraZoom, (value) => _cameraScaler.CameraZoom = value,
                    _currentZoomSize, duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void ChangeZoom(ZoomMode mode, bool immediately)
        {
            switch (mode)
            {
                case ZoomMode.In:
                    _currentZoomSize = _maxZoomSize;
                    break;
                case ZoomMode.Out:
                    _currentZoomSize = _minZoomSize;
                    break;
            }

            if (immediately)
            {
                _cameraScaler.CameraZoom = _currentZoomSize;
            }
        }
    }
}