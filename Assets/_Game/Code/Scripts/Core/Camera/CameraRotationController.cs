using DG.Tweening;
using UnityEngine;
using VinhLB.Utilities;

namespace VinhLB
{
    public class CameraRotationController : BaseController
    {
        [SerializeField]
        private Camera _camera;

        [Space(10)]
        [SerializeField]
        private LayerMask _blockLayer;
        [SerializeField]
        private BaseController[] _blockControllers;

        [Space(10)]
        [SerializeField]
        private float _rotationSpeed = 100f;
        [SerializeField]
        private float _smoothSpeed = 0.1f;

        [Space(10)]
        [SerializeField]
        private float _startAngleX = 30f;
        [SerializeField]
        private float _topAngleLimit = 45f;
        [SerializeField]
        private float _bottomAngleLimit = 15f;

        [Space(10)]
        [SerializeField]
        private float _startAngleY = 45f;
        // [SerializeField]
        // private float _leftAngleLimit = 105f;
        // [SerializeField]
        // private float _rightAngleLimit = -15f;

        private bool _canGetInput = true;
        private bool _canUpdate = true;
        private float _pitch;
        private float _yaw;

        public float StartAngleY
        {
            get => _startAngleY;
            set => _startAngleY = value;
        }

        private void Awake()
        {
            ResetRotation(true);
        }

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

            if (_canGetInput && VLBInput.IsPointerDown())
            {
                if (VLBInput.IsPointerOnUI()
                    || VLBPhysics.TryCastRay(_camera, out _, float.PositiveInfinity, _blockLayer))
                {
                    _canGetInput = false;
                }
            }

            if (_canGetInput)
            {
                if (VLBInput.IsPointerActive() && (VLBApplication.IsOnEditor() || Input.touchCount == 1))
                {
                    Vector2 deltaPosition = VLBInput.GetPointerDeltaPosition();
                    if (Vector2.Distance(deltaPosition, Vector2.zero) < 0.1f)
                    {
                        InUse = false;
                    }
                    else
                    {
                        InUse = true;

                        _pitch += -deltaPosition.y * _rotationSpeed * Time.deltaTime;
                        _yaw += deltaPosition.x * _rotationSpeed * Time.deltaTime;

                        _pitch = Mathf.Clamp(_pitch, _bottomAngleLimit, _topAngleLimit);

                        while (_yaw < 0)
                        {
                            _yaw += 360f;
                        }

                        while (_yaw >= 360f)
                        {
                            _yaw -= 360f;
                        }

                        // if (_leftAngleLimit > _rightAngleLimit)
                        // {
                        //     _yaw = Mathf.Clamp(_yaw, _rightAngleLimit, _leftAngleLimit);
                        // }
                        // else
                        // {
                        //     _yaw = Mathf.Clamp(_yaw, _leftAngleLimit, _rightAngleLimit);
                        // }
                    }
                }
                else
                {
                    InUse = false;
                }
            }
            else
            {
                if (VLBInput.IsPointerUp())
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

            Vector3 targetEulerAngles = GetTargetEulerAngles(_pitch, _yaw);
            transform.localRotation =
                Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetEulerAngles), _smoothSpeed);
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

        public Tween ResetRotation(bool immediately, float duration = 1f, System.Action onComplete = null)
        {
            _pitch = _startAngleX;
            _yaw = _startAngleY;
            Vector3 targetEulerAngles = GetTargetEulerAngles(_pitch, _yaw);

            if (immediately)
            {
                transform.localRotation = Quaternion.Euler(targetEulerAngles);

                onComplete?.Invoke();

                return null;
            }

            return RotateToTargetEulerAngles(targetEulerAngles, duration, Ease.InBack, onComplete);
        }

        // public Tween RotateToNearestLimit(float leftOffset = 0, float rightOffset = 0, float duration = 1f,
        //     Ease ease = Ease.OutBack, System.Action onComplete = null)
        // {
        //     float distanceToLeftLimit = Mathf.Abs(_yaw - _leftAngleLimit);
        //     float distanceToRightLimit = Mathf.Abs(_yaw - _rightAngleLimit);
        //     float targetAngleLimit = distanceToRightLimit < distanceToLeftLimit
        //         ? _rightAngleLimit - rightOffset
        //         : _leftAngleLimit + leftOffset;
        //     _yaw = targetAngleLimit;
        //     Vector3 targetEulerAngles = GetTargetEulerAngles(_yaw);
        //
        //     return RotateToTargetEulerAngles(targetEulerAngles, duration, ease, onComplete);
        // }
        //
        // public Tween RotateToFarthestLimit(float leftOffset = 0, float rightOffset = 0, float duration = 1f,
        //     Ease ease = Ease.Linear, System.Action onComplete = null)
        // {
        //     float distanceToLeftLimit = Mathf.Abs(_yaw - _leftAngleLimit);
        //     float distanceToRightLimit = Mathf.Abs(_yaw - _rightAngleLimit);
        //     float targetAngleLimit = distanceToRightLimit > distanceToLeftLimit
        //         ? _rightAngleLimit - rightOffset
        //         : _leftAngleLimit + leftOffset;
        //     _yaw = targetAngleLimit;
        //     Vector3 targetEulerAngles = GetTargetEulerAngles(_yaw);
        //
        //     return RotateToTargetEulerAngles(targetEulerAngles, duration, ease, onComplete);
        // }

        public Tween RotateToTargetEulerAngles(Vector3 targetEulerAngles, float duration, Ease ease,
            System.Action onComplete)
        {
            return transform.DOLocalRotate(targetEulerAngles, duration)
                .SetEase(ease)
                .OnComplete(() => onComplete?.Invoke());
        }

        private Vector3 GetTargetEulerAngles(float angleX, float angleY)
        {
            return new Vector3(angleX, angleY, 0);
        }
    }
}