using DG.Tweening;
using UnityEngine;

namespace TrashRush.Game
{
    [CreateAssetMenu(fileName = "SlashConfig", menuName = "Trash Rush/Game/Slash Config")]
    public sealed class SlashConfig : ScriptableObject
    {
        [SerializeField] private float _scaleTime = 0.08f;
        [SerializeField] private float _holdTime = 0.04f;
        [SerializeField] private float _descaleTime = 0.12f;
        [SerializeField, Min(0f)] private float _minDragWorldDistance = 1f;
        [SerializeField] private bool _pulseEnabled = true;
        [SerializeField] private float _pulseScale = 1.06f;
        [SerializeField] private float _pulseTime = 0.035f;
        [SerializeField] private Ease _scaleEase = Ease.OutQuad;
        [SerializeField] private Ease _pulseEase = Ease.InOutSine;
        [SerializeField] private Ease _descaleEase = Ease.InQuad;
        [SerializeField] private bool _useUnscaledTime;
        [SerializeField, Min(0f)] private float _decalProjectorLifetime = 2f;
        [SerializeField, Min(0f)] private float _decalProjectorFadeTime = 0.35f;
        [SerializeField, Min(0f)] private float _decalProjectorMinHeight = 3f;
        [SerializeField, Min(0f)] private float _decalProjectorMaxHeight = 8f;
        [SerializeField, Min(0.01f)] private float _streakResetDelay = 1.5f;
        [Header("Background Slash")]
        [SerializeField] private bool _backgroundSlashEnabled = true;
        [SerializeField, Min(0f)] private float _backgroundSlashLifetime = 2f;
        [SerializeField, Min(0f)] private float _backgroundSlashFadeTime = 0.35f;
        [SerializeField, Min(0f)] private float _backgroundSlashMinHeight = 2.5f;
        [SerializeField, Min(0f)] private float _backgroundSlashMaxHeight = 10f;
        [SerializeField] private Vector2 _backgroundSlashScaleMultiplier = Vector2.one;
        [SerializeField] private Color _backgroundSlashColor = Color.white;
        [SerializeField] private int _backgroundSlashSortingOrderOffset = 1;
        [Header("Leaf Slash Wind")]
        [SerializeField, Min(0f)] private float _leafWindRadius = 3f;
        [SerializeField, Min(0f)] private float _leafWindVelocityImpulse = 5f;
        [SerializeField, Min(0f)] private float _leafWindMaxVelocity = 10f;

        public float ScaleTime => _scaleTime;
        public float HoldTime => _holdTime;
        public float DescaleTime => _descaleTime;
        public float MinDragWorldDistance => _minDragWorldDistance;
        public bool PulseEnabled => _pulseEnabled;
        public float PulseScale => _pulseScale;
        public float PulseTime => _pulseTime;
        public Ease ScaleEase => _scaleEase;
        public Ease PulseEase => _pulseEase;
        public Ease DescaleEase => _descaleEase;
        public bool UseUnscaledTime => _useUnscaledTime;
        public float DecalProjectorLifetime => _decalProjectorLifetime;
        public float DecalProjectorFadeTime => _decalProjectorFadeTime;
        public float DecalProjectorMinHeight => _decalProjectorMinHeight;
        public float DecalProjectorMaxHeight => _decalProjectorMaxHeight;
        public bool BackgroundSlashEnabled => _backgroundSlashEnabled;
        public float BackgroundSlashLifetime => Mathf.Max(0f, _backgroundSlashLifetime);
        public float BackgroundSlashFadeTime => Mathf.Max(0f, _backgroundSlashFadeTime);
        public float BackgroundSlashMinHeight => Mathf.Max(0f, _backgroundSlashMinHeight);
        public float BackgroundSlashMaxHeight =>
            Mathf.Max(BackgroundSlashMinHeight, _backgroundSlashMaxHeight);
        public Vector2 BackgroundSlashScaleMultiplier => new(
            Mathf.Max(0f, _backgroundSlashScaleMultiplier.x),
            Mathf.Max(0f, _backgroundSlashScaleMultiplier.y));
        public Color BackgroundSlashColor => _backgroundSlashColor;
        public int BackgroundSlashSortingOrderOffset => _backgroundSlashSortingOrderOffset;
        public float LeafWindRadius => Mathf.Max(0f, _leafWindRadius);
        public float LeafWindVelocityImpulse => Mathf.Max(0f, _leafWindVelocityImpulse);
        public float LeafWindMaxVelocity => Mathf.Max(0f, _leafWindMaxVelocity);
        public float StreakResetDelay => Mathf.Max(0.01f, _streakResetDelay);
    }
}
