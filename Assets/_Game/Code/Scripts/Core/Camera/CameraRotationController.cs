using System.Collections;
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
        [SerializeField]
        private float _rotateAroundDelay = 3f;
        [SerializeField]
        private float _rotateAroundAngle = 30f;

        private bool _canGetInput = true;
        private bool _canUpdate = true;
        private float _pitch;
        private float _yaw;
        private Coroutine _rotateAroundCoroutine;

        public float StartAngleY
        {
            get => _startAngleY;
            set => _startAngleY = value;
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
                    SetInUse(true);

                    Vector2 deltaPosition = VLBInput.GetPointerDeltaPosition();

                    _pitch += -deltaPosition.y * _rotationSpeed;
                    _yaw += deltaPosition.x * _rotationSpeed;

                    _pitch = Mathf.Clamp(_pitch, _bottomAngleLimit, _topAngleLimit);
                    _yaw = GetClampedYaw(_yaw);
                }
                else
                {
                    SetInUse(false);
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

            Quaternion targetRotation = Quaternion.Euler(GetTargetEulerAngles(_pitch, _yaw));
            transform.localRotation = 
                Quaternion.Lerp(transform.localRotation, targetRotation, _smoothSpeed * Time.deltaTime);
        }

        public override void SetInUse(bool value)
        {
            base.SetInUse(value);

            if (!value)
            {
                StartRotateAround();
            }
            else
            {
                StopRotateAround();
            }
        }

        public void SetControl(bool value, bool keepUpdating)
        {
            if (IsControllable == value)
            {
                return;
            }

            SetControl(value);

            if (!value)
            {
                SetInUse(false);
            }

            _canUpdate = keepUpdating;

            _canGetInput = true;
        }

        public void SetBlockLayer(LayerMask layer)
        {
            _blockLayer = layer;
        }

        public void StartRotateAround()
        {
            if (_rotateAroundCoroutine != null)
            {
                return;
            }

            _rotateAroundCoroutine = StartCoroutine(RotateAroundCoroutine(_rotateAroundDelay));

            return;

            IEnumerator RotateAroundCoroutine(float delay)
            {
                yield return new WaitForSeconds(delay);

                while (true)
                {
                    _yaw += _rotateAroundAngle * Time.deltaTime;
                    _yaw = GetClampedYaw(_yaw);

                    yield return null;
                }
            }
        }

        public void StopRotateAround()
        {
            if (_rotateAroundCoroutine == null)
            {
                return;
            }

            StopCoroutine(_rotateAroundCoroutine);

            _rotateAroundCoroutine = null;
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

        private float GetClampedYaw(float value)
        {
            while (value < 0)
            {
                value += 360f;
            }

            while (value >= 360f)
            {
                value -= 360f;
            }

            return value;
        }
    }
}