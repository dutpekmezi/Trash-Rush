using UnityEngine;

namespace TrashRush.Game
{
    public sealed class HitTextService
    {
        private readonly Camera _camera;
        private readonly HitTextConfig _config;
        private readonly HitText _prefab;
        private readonly RectTransform _parent;
        private readonly Canvas _canvas;

        public HitTextService(
            Camera camera,
            HitTextConfig config,
            HitText prefab,
            RectTransform parent)
        {
            _camera = camera;
            _config = config;
            _prefab = prefab;
            _parent = parent;
            _canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;
        }

        public void Play(
            Vector3 startWorldPosition,
            Vector3 endWorldPosition,
            int streak)
        {
            if (_camera == null || _config == null || _prefab == null || _parent == null)
            {
                return;
            }

            var delta = endWorldPosition - startWorldPosition;
            delta.z = 0f;

            if (delta.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var screenPosition = _camera.WorldToScreenPoint(startWorldPosition);
            var canvasCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera != null ? _canvas.worldCamera : _camera
                : null;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parent,
                    screenPosition,
                    canvasCamera,
                    out var anchoredPosition))
            {
                return;
            }

            var hitText = Object.Instantiate(_prefab, _parent, false);
            var zRotation = GetHitTextZRotation(delta, _config.MaxRotationAngle);
            hitText.Play(anchoredPosition, zRotation, streak, _config);
        }

        private static float GetHitTextZRotation(Vector3 delta, float maxRotationAngle)
        {
            if (delta.x < 0f ||
                (Mathf.Abs(delta.x) <= Mathf.Epsilon && delta.y < 0f))
            {
                delta = -delta;
            }

            var lineAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            return Mathf.Clamp(lineAngle, -maxRotationAngle, maxRotationAngle);
        }
    }
}
