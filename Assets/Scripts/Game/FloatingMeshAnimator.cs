using DG.Tweening;
using UnityEngine;

namespace TrashRush.Game
{
    public sealed class FloatingMeshAnimator : MonoBehaviour
    {
        [SerializeField] private Transform _mesh;
        [SerializeField] private FloatingConfig _floatingConfig;

        private Tween _rotationTween;
        private Quaternion _startLocalRotation;
        private bool _hasStartLocalRotation;

        private void Awake()
        {
            if (_mesh == null)
            {
                _mesh = transform;
            }

            CaptureStartRotation();
        }

        private void OnEnable()
        {
            CaptureStartRotation();
            StartRotation();
        }

        private void OnDisable()
        {
            StopRotation(true);
        }

        private void OnDestroy()
        {
            StopRotation(false);
        }

        private void OnValidate()
        {
            if (_mesh == null)
            {
                _mesh = transform;
            }
        }

        public void Stop()
        {
            StopRotation(false);
        }

        private void StartRotation()
        {
            if (_mesh == null ||
                _floatingConfig == null ||
                !_floatingConfig.RotationEnabled)
            {
                return;
            }

            QueueNextRotation(Random.Range(0f, _floatingConfig.MaxStartDelay));
        }

        private void QueueNextRotation(float delay)
        {
            if (!isActiveAndEnabled ||
                _mesh == null ||
                _floatingConfig == null ||
                !_floatingConfig.RotationEnabled)
            {
                return;
            }

            var duration = Random.Range(_floatingConfig.MinRotationDuration, _floatingConfig.MaxRotationDuration);
            var rotationOffset = GetRandomRotationOffset();

            var sequence = DOTween.Sequence()
                .SetLink(_mesh.gameObject);

            if (delay > 0f)
            {
                sequence.AppendInterval(delay);
            }

            sequence
                .Append(_mesh
                    .DOBlendableLocalRotateBy(rotationOffset, duration, RotateMode.LocalAxisAdd)
                    .SetEase(_floatingConfig.RotationEase))
                .OnComplete(() => QueueNextRotation(0f));

            if (_floatingConfig.UseUnscaledTime)
            {
                sequence.SetUpdate(true);
            }

            _rotationTween = sequence;
        }

        private Vector3 GetRandomRotationOffset()
        {
            var axisWeights = _floatingConfig.AxisWeights;
            var axis = Vector3.Scale(Random.onUnitSphere, axisWeights);

            if (axis.sqrMagnitude <= Mathf.Epsilon)
            {
                axis = Random.onUnitSphere;
            }

            var angle = Random.Range(_floatingConfig.MinRotationAngle, _floatingConfig.MaxRotationAngle);
            return axis.normalized * angle;
        }

        private void CaptureStartRotation()
        {
            if (_mesh == null)
            {
                return;
            }

            _startLocalRotation = _mesh.localRotation;
            _hasStartLocalRotation = true;
        }

        private void StopRotation(bool restoreRotation)
        {
            if (_rotationTween != null && _rotationTween.IsActive())
            {
                _rotationTween.Kill(false);
            }

            _rotationTween = null;

            if (restoreRotation && _mesh != null && _hasStartLocalRotation)
            {
                _mesh.localRotation = _startLocalRotation;
            }
        }
    }
}
