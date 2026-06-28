using System;
using DG.Tweening;
using UnityEngine;
using VContainer.Unity;

namespace TrashRush.Game
{
    public sealed class CameraStreakZoomService : IStartable, IDisposable
    {
        private readonly Camera _camera;
        private readonly Transform _cameraRig;
        private readonly SlashManager _slashManager;
        private readonly SlashObjectService _slashObjectService;
        private readonly HitEffectConfig _config;

        private Tween _cameraTween;
        private float _initialZoom;
        private Vector3 _initialRigLocalPosition;
        private Vector3 _initialCameraLocalPosition;
        private bool _started;

        public CameraStreakZoomService(
            Camera camera,
            Transform cameraRig,
            SlashManager slashManager,
            SlashObjectService slashObjectService,
            HitEffectConfig config)
        {
            _camera = camera;
            _cameraRig = cameraRig;
            _slashManager = slashManager;
            _slashObjectService = slashObjectService;
            _config = config;
        }

        public void Start()
        {
            if (_camera == null || _cameraRig == null || _slashManager == null)
            {
                return;
            }

            _initialZoom = GetZoom();
            _initialRigLocalPosition = _cameraRig.localPosition;
            _initialCameraLocalPosition = _camera.transform.localPosition;
            _slashManager.StreakChanged += OnStreakChanged;
            _started = true;
        }

        public void Dispose()
        {
            if (_started && _slashManager != null)
            {
                _slashManager.StreakChanged -= OnStreakChanged;
            }

            KillCameraTween();

            if (_started && _camera != null)
            {
                SetZoom(_initialZoom);
            }

            if (_started && _cameraRig != null)
            {
                _cameraRig.localPosition = _initialRigLocalPosition;
            }

            _started = false;
        }

        private void OnStreakChanged(int streak)
        {
            if (_camera == null || _config == null || !_config.StreakZoomEnabled)
            {
                return;
            }

            if (streak <= 0)
            {
                TweenCamera(
                    _initialZoom,
                    _initialRigLocalPosition,
                    _config.StreakZoomResetDuration,
                    _config.StreakZoomResetEase);
                return;
            }

            var zoomAmount = Mathf.Min(
                streak * _config.StreakZoomAmountPerHit,
                _config.StreakZoomMaxAmount);
            var targetZoom = Mathf.Max(0.01f, _initialZoom - zoomAmount);
            var targetRigPosition = GetClampedFocusPosition(targetZoom);

            TweenCamera(
                targetZoom,
                targetRigPosition,
                _config.StreakZoomInDuration,
                _config.StreakZoomInEase);
        }

        private void TweenCamera(
            float targetZoom,
            Vector3 targetRigPosition,
            float duration,
            Ease ease)
        {
            KillCameraTween();

            _cameraTween = DOTween.Sequence()
                .SetLink(_camera.gameObject)
                .Join(DOVirtual
                    .Float(GetZoom(), targetZoom, duration, SetZoom)
                    .SetEase(ease))
                .Join(_cameraRig
                    .DOLocalMove(targetRigPosition, duration)
                    .SetEase(ease))
                .OnKill(() => _cameraTween = null);

            if (_config.UseUnscaledTime)
            {
                _cameraTween.SetUpdate(true);
            }
        }

        private Vector3 GetClampedFocusPosition(float targetZoom)
        {
            var focusTarget = _slashObjectService != null
                ? _slashObjectService.ActiveObject
                : null;

            if (!_camera.orthographic ||
                focusTarget == null ||
                focusTarget.RotationTarget == null)
            {
                return _initialRigLocalPosition;
            }

            var targetWorldPosition = focusTarget.RotationTarget.position;
            var targetInRigParent = _cameraRig.parent != null
                ? _cameraRig.parent.InverseTransformPoint(targetWorldPosition)
                : targetWorldPosition;

            var initialCenter = _initialRigLocalPosition + _initialCameraLocalPosition;
            var maxHorizontalOffset = Mathf.Max(
                0f,
                _initialZoom * _camera.aspect - targetZoom * _camera.aspect);
            var maxVerticalOffset = Mathf.Max(0f, _initialZoom - targetZoom);

            var targetCameraCenter = new Vector3(
                Mathf.Clamp(
                    targetInRigParent.x,
                    initialCenter.x - maxHorizontalOffset,
                    initialCenter.x + maxHorizontalOffset),
                Mathf.Clamp(
                    targetInRigParent.y,
                    initialCenter.y - maxVerticalOffset,
                    initialCenter.y + maxVerticalOffset),
                initialCenter.z);

            var targetRigPosition = targetCameraCenter - _initialCameraLocalPosition;
            targetRigPosition.z = _initialRigLocalPosition.z;
            return targetRigPosition;
        }

        private float GetZoom()
        {
            return _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
        }

        private void SetZoom(float value)
        {
            if (_camera.orthographic)
            {
                _camera.orthographicSize = value;
            }
            else
            {
                _camera.fieldOfView = value;
            }
        }

        private void KillCameraTween()
        {
            if (_cameraTween != null && _cameraTween.IsActive())
            {
                _cameraTween.Kill(false);
            }

            _cameraTween = null;
        }
    }
}
