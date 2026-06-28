using System;
using UnityEngine;
using VContainer.Unity;

namespace TrashRush.Game
{
    public sealed class LeafSlashWindService : IStartable, IDisposable
    {
        private readonly SlashManager _slashManager;
        private readonly ParticleSystem _fallingLeaves;
        private readonly SlashConfig _config;

        private ParticleSystem.Particle[] _particles;
        private bool _started;

        public LeafSlashWindService(
            SlashManager slashManager,
            ParticleSystem fallingLeaves,
            SlashConfig config)
        {
            _slashManager = slashManager;
            _fallingLeaves = fallingLeaves;
            _config = config;
        }

        public void Start()
        {
            if (_slashManager == null || _fallingLeaves == null)
            {
                return;
            }

            _slashManager.SlashPlayed += OnSlashPlayed;
            _started = true;
        }

        public void Dispose()
        {
            if (_started && _slashManager != null)
            {
                _slashManager.SlashPlayed -= OnSlashPlayed;
            }

            _started = false;
            _particles = null;
        }

        private void OnSlashPlayed(Vector3 startPosition, Vector3 endPosition)
        {
            if (_fallingLeaves == null ||
                _config == null ||
                _config.LeafWindRadius <= Mathf.Epsilon ||
                _config.LeafWindVelocityImpulse <= Mathf.Epsilon)
            {
                return;
            }

            var slashDelta = endPosition - startPosition;
            slashDelta.z = 0f;
            var slashLengthSquared = slashDelta.sqrMagnitude;

            if (slashLengthSquared <= Mathf.Epsilon)
            {
                return;
            }

            EnsureParticleBuffer();
            var particleCount = _fallingLeaves.GetParticles(_particles);

            if (particleCount == 0)
            {
                return;
            }

            var main = _fallingLeaves.main;
            var simulationTransform = GetSimulationTransform(main);
            var slashDirection = slashDelta.normalized;
            var radius = _config.LeafWindRadius;
            var radiusSquared = radius * radius;
            var changed = false;

            for (var i = 0; i < particleCount; i++)
            {
                var particle = _particles[i];
                var worldPosition = simulationTransform != null
                    ? simulationTransform.TransformPoint(particle.position)
                    : particle.position;

                var toParticle = worldPosition - startPosition;
                toParticle.z = 0f;
                var segmentProgress = Mathf.Clamp01(
                    Vector3.Dot(toParticle, slashDelta) / slashLengthSquared);
                var closestPoint = startPosition + slashDelta * segmentProgress;
                var offset = worldPosition - closestPoint;
                offset.z = 0f;
                var distanceSquared = offset.sqrMagnitude;

                if (distanceSquared > radiusSquared)
                {
                    continue;
                }

                var distance = Mathf.Sqrt(distanceSquared);
                var falloff = 1f - distance / radius;
                falloff = falloff * falloff * (3f - 2f * falloff);

                var worldVelocity = simulationTransform != null
                    ? simulationTransform.TransformVector(particle.velocity)
                    : particle.velocity;
                worldVelocity += slashDirection * (_config.LeafWindVelocityImpulse * falloff);
                ClampPlanarVelocity(ref worldVelocity);

                particle.velocity = simulationTransform != null
                    ? simulationTransform.InverseTransformVector(worldVelocity)
                    : worldVelocity;
                _particles[i] = particle;
                changed = true;
            }

            if (changed)
            {
                _fallingLeaves.SetParticles(_particles, particleCount);
            }
        }

        private void EnsureParticleBuffer()
        {
            var requiredSize = Mathf.Max(1, _fallingLeaves.main.maxParticles);

            if (_particles == null || _particles.Length < requiredSize)
            {
                _particles = new ParticleSystem.Particle[requiredSize];
            }
        }

        private Transform GetSimulationTransform(ParticleSystem.MainModule main)
        {
            switch (main.simulationSpace)
            {
                case ParticleSystemSimulationSpace.Local:
                    return _fallingLeaves.transform;
                case ParticleSystemSimulationSpace.Custom:
                    return main.customSimulationSpace != null
                        ? main.customSimulationSpace
                        : _fallingLeaves.transform;
                default:
                    return null;
            }
        }

        private void ClampPlanarVelocity(ref Vector3 velocity)
        {
            var maxVelocity = _config.LeafWindMaxVelocity;

            if (maxVelocity <= Mathf.Epsilon)
            {
                return;
            }

            var planarVelocity = new Vector2(velocity.x, velocity.y);

            if (planarVelocity.sqrMagnitude <= maxVelocity * maxVelocity)
            {
                return;
            }

            planarVelocity = planarVelocity.normalized * maxVelocity;
            velocity.x = planarVelocity.x;
            velocity.y = planarVelocity.y;
        }
    }
}
