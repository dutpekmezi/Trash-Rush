using DG.Tweening;
using UnityEngine;

namespace TrashRush.Game
{
    [CreateAssetMenu(fileName = "FloatingConfig", menuName = "Trash Rush/Game/Floating Config")]
    public sealed class FloatingConfig : ScriptableObject
    {
        [SerializeField] private bool _rotationEnabled = true;
        [SerializeField, Min(0.01f)] private float _minRotationDuration = 4f;
        [SerializeField, Min(0.01f)] private float _maxRotationDuration = 7f;
        [SerializeField, Min(0f)] private float _minRotationAngle = 45f;
        [SerializeField, Min(0f)] private float _maxRotationAngle = 145f;
        [SerializeField, Min(0f)] private float _maxStartDelay = 0.45f;
        [SerializeField] private Vector3 _axisWeights = Vector3.one;
        [SerializeField] private Ease _rotationEase = Ease.InOutSine;
        [SerializeField] private bool _useUnscaledTime;

        public bool RotationEnabled => _rotationEnabled;
        public float MinRotationDuration => Mathf.Max(0.01f, _minRotationDuration);
        public float MaxRotationDuration => Mathf.Max(MinRotationDuration, _maxRotationDuration);
        public float MinRotationAngle => Mathf.Max(0f, _minRotationAngle);
        public float MaxRotationAngle => Mathf.Max(MinRotationAngle, _maxRotationAngle);
        public float MaxStartDelay => Mathf.Max(0f, _maxStartDelay);
        public Vector3 AxisWeights => new Vector3(
            Mathf.Max(0f, _axisWeights.x),
            Mathf.Max(0f, _axisWeights.y),
            Mathf.Max(0f, _axisWeights.z));
        public Ease RotationEase => _rotationEase;
        public bool UseUnscaledTime => _useUnscaledTime;
    }
}
