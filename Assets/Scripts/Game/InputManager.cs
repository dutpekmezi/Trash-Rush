using System;
using Lean.Touch;
using UnityEngine;
using VContainer.Unity;

namespace TrashRush.Game
{
    public sealed class InputManager : IStartable, IDisposable
    {
        private const float DefaultMinDragWorldDistance = 1f;

        private readonly Camera _camera;
        private readonly SlashManager _slashManager;
        private readonly SlashDecalSpawner _slashDecalSpawner;
        private readonly SlashObjectService _slashObjectService;
        private readonly HitTextService _hitTextService;
        private readonly SlashConfig _config;

        private LeanFinger _activeFinger;
        private Vector3 _startWorldPosition;
        private bool _slashPlayed;

        public InputManager(
            Camera camera,
            SlashManager slashManager,
            SlashDecalSpawner slashDecalSpawner,
            SlashObjectService slashObjectService,
            HitTextService hitTextService,
            SlashConfig config)
        {
            _camera = camera;
            _slashManager = slashManager;
            _slashDecalSpawner = slashDecalSpawner;
            _slashObjectService = slashObjectService;
            _hitTextService = hitTextService;
            _config = config;
        }

        public void Start()
        {
            LeanTouch.OnFingerDown += OnFingerDown;
            LeanTouch.OnFingerUpdate += OnFingerUpdate;
            LeanTouch.OnFingerUp += OnFingerUp;
        }

        public void Dispose()
        {
            LeanTouch.OnFingerDown -= OnFingerDown;
            LeanTouch.OnFingerUpdate -= OnFingerUpdate;
            LeanTouch.OnFingerUp -= OnFingerUp;

            _activeFinger = null;
        }

        private void OnFingerDown(LeanFinger finger)
        {
            if (!_slashObjectService.IsPlaying ||
                finger.StartedOverGui ||
                finger.IsOverGui)
            {
                return;
            }

            if (_activeFinger != null)
            {
                return;
            }

            _activeFinger = finger;
            _startWorldPosition = GetWorldPosition(finger);
            _slashPlayed = false;
        }

        private void OnFingerUpdate(LeanFinger finger)
        {
            if (_activeFinger != finger || _slashPlayed || finger.IsOverGui)
            {
                return;
            }

            var endWorldPosition = GetWorldPosition(finger);
            var dragDistance = Vector3.Distance(_startWorldPosition, endWorldPosition);

            if (dragDistance < MinDragWorldDistance)
            {
                return;
            }

            _slashManager.Play(_startWorldPosition, endWorldPosition);
            var nextStreak = _slashManager.CurrentStreak + 1;

            if (_slashDecalSpawner.TrySpawn(
                    _startWorldPosition,
                    endWorldPosition,
                    nextStreak,
                    out var slashObject))
            {
                var streak = _slashManager.RegisterHit();
                _hitTextService.Play(_startWorldPosition, endWorldPosition, streak);
                _slashObjectService.TryRegisterSlash(slashObject);
            }
            else
            {
                _slashManager.RegisterMiss();
            }

            _slashPlayed = true;
        }

        private void OnFingerUp(LeanFinger finger)
        {
            if (_activeFinger != finger)
            {
                return;
            }

            _activeFinger = null;
            _slashPlayed = false;
        }

        private Vector3 GetWorldPosition(LeanFinger finger)
        {
            var position = finger.GetWorldPosition(_slashManager.Depth - _camera.transform.position.z, _camera);
            position.z = _slashManager.Depth;

            return position;
        }

        private float MinDragWorldDistance =>
            _config != null ? Mathf.Max(0f, _config.MinDragWorldDistance) : DefaultMinDragWorldDistance;
    }
}
