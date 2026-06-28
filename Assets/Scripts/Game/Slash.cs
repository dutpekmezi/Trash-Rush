using System;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace TrashRush.Game
{
    public sealed class Slash : MonoBehaviour
    {
        [SerializeField] private Transform _mesh;
        [SerializeField] private SlashConfig _config;

        private Vector3 _fullScale;
        private Vector3 _targetScale;
        private Sequence _sequence;
        private Sequence _pulseSequence;

        public float Depth => transform.position.z;

        [Inject]
        public void Construct(SlashConfig config)
        {
            _config = config;
        }

        private void Awake()
        {
            if (_mesh == null)
            {
                return;
            }

            _fullScale = _mesh.localScale;
            _targetScale = _fullScale;
            _mesh.localScale = Vector3.zero;
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
            _pulseSequence?.Kill();
        }

        public void Play(Vector3 position)
        {
            Play(position, Quaternion.identity, _fullScale);
        }

        public void Play(Vector3 startPosition, Vector3 endPosition)
        {
            Play(startPosition, endPosition, null);
        }

        public void Play(Vector3 startPosition, Vector3 endPosition, Action<Slash> completed)
        {
            var delta = endPosition - startPosition;
            delta.z = 0f;

            var length = delta.magnitude;

            if (length <= Mathf.Epsilon)
            {
                completed?.Invoke(this);
                return;
            }

            var position = (startPosition + endPosition) * 0.5f;
            position.z = Depth;

            var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;

            Play(position, Quaternion.Euler(0f, 0f, angle), _fullScale, completed);
        }

        private void Play(Vector3 position, Quaternion rotation, Vector3 targetScale, Action<Slash> completed = null)
        {
            if (_config == null || _mesh == null)
            {
                completed?.Invoke(this);
                return;
            }

            transform.SetPositionAndRotation(position, rotation);
            _targetScale = targetScale;
            _sequence?.Kill();
            _pulseSequence?.Kill();
            _mesh.localScale = Vector3.zero;

            _sequence = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_mesh.DOScale(_targetScale, _config.ScaleTime).SetEase(_config.ScaleEase))
                .AppendCallback(StartPulse)
                .AppendInterval(_config.HoldTime)
                .AppendCallback(StopPulse)
                .Append(_mesh.DOScale(Vector3.zero, _config.DescaleTime).SetEase(_config.DescaleEase))
                .AppendCallback(() => completed?.Invoke(this));

            if (_config.UseUnscaledTime)
            {
                _sequence.SetUpdate(true);
            }
        }

        private void StartPulse()
        {
            if (!_config.PulseEnabled || _config.PulseTime <= 0f || _config.HoldTime <= 0f)
            {
                return;
            }

            _pulseSequence = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_mesh.DOScale(_targetScale * _config.PulseScale, _config.PulseTime).SetEase(_config.PulseEase))
                .Append(_mesh.DOScale(_targetScale, _config.PulseTime).SetEase(_config.PulseEase))
                .SetLoops(-1);

            if (_config.UseUnscaledTime)
            {
                _pulseSequence.SetUpdate(true);
            }
        }

        private void StopPulse()
        {
            _pulseSequence?.Kill();
            _pulseSequence = null;
            _mesh.localScale = _targetScale;
        }
    }
}
