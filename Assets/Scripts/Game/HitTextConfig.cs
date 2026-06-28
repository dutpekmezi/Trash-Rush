using DG.Tweening;
using UnityEngine;

namespace TrashRush.Game
{
    [CreateAssetMenu(fileName = "HitTextConfig", menuName = "Trash Rush/Game/Hit Text Config")]
    public sealed class HitTextConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _riseDistance = 180f;
        [SerializeField, Min(0.01f)] private float _moveDuration = 0.4f;
        [SerializeField, Min(0f)] private float _fadeDelay = 0.05f;
        [SerializeField, Min(0.01f)] private float _fadeDuration = 0.3f;
        [SerializeField, Range(0f, 180f)] private float _maxRotationAngle = 45f;
        [SerializeField, Min(0f)] private float _scaleIncreasePerStreak = 0.08f;
        [SerializeField, Min(1f)] private float _maxScaleMultiplier = 2.5f;
        [SerializeField] private Ease _moveEase = Ease.OutCubic;
        [SerializeField] private Ease _fadeEase = Ease.InQuad;
        [SerializeField] private bool _useUnscaledTime;

        public float RiseDistance => Mathf.Max(0f, _riseDistance);
        public float MoveDuration => Mathf.Max(0.01f, _moveDuration);
        public float FadeDelay => Mathf.Max(0f, _fadeDelay);
        public float FadeDuration => Mathf.Max(0.01f, _fadeDuration);
        public float MaxRotationAngle => Mathf.Clamp(_maxRotationAngle, 0f, 180f);
        public float ScaleIncreasePerStreak => Mathf.Max(0f, _scaleIncreasePerStreak);
        public float MaxScaleMultiplier => Mathf.Max(1f, _maxScaleMultiplier);
        public Ease MoveEase => _moveEase;
        public Ease FadeEase => _fadeEase;
        public bool UseUnscaledTime => _useUnscaledTime;

        public float GetScaleMultiplier(int streak)
        {
            var multiplier = 1f + Mathf.Max(0, streak - 1) * ScaleIncreasePerStreak;
            return Mathf.Min(multiplier, MaxScaleMultiplier);
        }
    }
}
