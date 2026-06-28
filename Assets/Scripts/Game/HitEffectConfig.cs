using DG.Tweening;
using UnityEngine;

namespace TrashRush.Game
{
    [CreateAssetMenu(fileName = "HitEffectConfig", menuName = "Trash Rush/Game/Hit Effect Config")]
    public sealed class HitEffectConfig : ScriptableObject
    {
        [SerializeField] private bool _rotationEnabled = true;
        [SerializeField, Min(0f)] private float _rotationDuration = 0.2f;
        [SerializeField, Min(0f)] private float _minRotationAngle = 4f;
        [SerializeField, Min(0f)] private float _maxRotationAngle = 12f;
        [SerializeField, Min(0f)] private float _rotationStrengthIncreasePerStreak = 0.08f;
        [SerializeField, Min(1f)] private float _maxRotationStrengthMultiplier = 2.5f;
        [SerializeField] private Ease _rotationEase = Ease.OutSine;
        [SerializeField] private bool _screenShakeEnabled = true;
        [SerializeField, Min(0f)] private float _screenShakeDuration = 0.12f;
        [SerializeField, Min(0f)] private float _screenShakeBaseStrength = 0.06f;
        [SerializeField, Min(0f)] private float _screenShakeStrengthPerStreak = 0.02f;
        [SerializeField, Min(0f)] private float _screenShakeMaxStrength = 0.3f;
        [SerializeField, Min(1)] private int _screenShakeVibrato = 10;
        [SerializeField, Range(0f, 180f)] private float _screenShakeRandomness = 90f;
        [SerializeField] private bool _streakZoomEnabled = true;
        [SerializeField, Min(0f)] private float _streakZoomAmountPerHit = 0.08f;
        [SerializeField, Min(0f)] private float _streakZoomMaxAmount = 1.2f;
        [SerializeField, Min(0.01f)] private float _streakZoomInDuration = 0.1f;
        [SerializeField, Min(0.01f)] private float _streakZoomResetDuration = 0.8f;
        [SerializeField] private Ease _streakZoomInEase = Ease.OutQuad;
        [SerializeField] private Ease _streakZoomResetEase = Ease.OutSine;
        [SerializeField] private bool _useUnscaledTime;

        public bool RotationEnabled => _rotationEnabled;
        public float RotationDuration => Mathf.Max(0f, _rotationDuration);
        public float MinRotationAngle => Mathf.Max(0f, _minRotationAngle);
        public float MaxRotationAngle => Mathf.Max(MinRotationAngle, _maxRotationAngle);
        public float RotationStrengthIncreasePerStreak =>
            Mathf.Max(0f, _rotationStrengthIncreasePerStreak);
        public float MaxRotationStrengthMultiplier =>
            Mathf.Max(1f, _maxRotationStrengthMultiplier);
        public Ease RotationEase => _rotationEase;
        public bool ScreenShakeEnabled => _screenShakeEnabled;
        public float ScreenShakeDuration => Mathf.Max(0f, _screenShakeDuration);
        public float ScreenShakeBaseStrength => Mathf.Max(0f, _screenShakeBaseStrength);
        public float ScreenShakeStrengthPerStreak =>
            Mathf.Max(0f, _screenShakeStrengthPerStreak);
        public float ScreenShakeMaxStrength =>
            Mathf.Max(ScreenShakeBaseStrength, _screenShakeMaxStrength);
        public int ScreenShakeVibrato => Mathf.Max(1, _screenShakeVibrato);
        public float ScreenShakeRandomness => Mathf.Clamp(_screenShakeRandomness, 0f, 180f);
        public bool StreakZoomEnabled => _streakZoomEnabled;
        public float StreakZoomAmountPerHit => Mathf.Max(0f, _streakZoomAmountPerHit);
        public float StreakZoomMaxAmount => Mathf.Max(0f, _streakZoomMaxAmount);
        public float StreakZoomInDuration => Mathf.Max(0.01f, _streakZoomInDuration);
        public float StreakZoomResetDuration => Mathf.Max(0.01f, _streakZoomResetDuration);
        public Ease StreakZoomInEase => _streakZoomInEase;
        public Ease StreakZoomResetEase => _streakZoomResetEase;
        public bool UseUnscaledTime => _useUnscaledTime;

        public float GetRotationStrengthMultiplier(int streak)
        {
            var multiplier =
                1f + Mathf.Max(0, streak - 1) * RotationStrengthIncreasePerStreak;
            return Mathf.Min(multiplier, MaxRotationStrengthMultiplier);
        }

        public float GetScreenShakeStrength(int streak)
        {
            var strength =
                ScreenShakeBaseStrength +
                Mathf.Max(0, streak - 1) * ScreenShakeStrengthPerStreak;
            return Mathf.Min(strength, ScreenShakeMaxStrength);
        }
    }
}
