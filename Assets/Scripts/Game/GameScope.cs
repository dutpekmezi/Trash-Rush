using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TrashRush.Game
{
    public sealed class GameScope : LifetimeScope
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _cameraRig;
        [SerializeField] private Slash _slashPrefab;
        [SerializeField] private SlashConfig _slashConfig;
        [SerializeField] private HitEffectConfig _hitEffectConfig;
        [SerializeField] private HitTextConfig _hitTextConfig;
        [SerializeField] private SlashObjectSpawnConfig _slashObjectSpawnConfig;
        [SerializeField] private GameObject _decalProjectorPrefab;
        [SerializeField] private Sprite _backgroundSlashHalfSprite;
        [SerializeField] private Sprite _backgroundSlashTopHalfSprite;
        [SerializeField] private HitText _hitTextPrefab;
        [SerializeField] private RectTransform _hitTextContainer;
        [SerializeField] private LayerMask _slashHitLayerMask = ~0;
        [SerializeField, Min(2)] private int _slashHitSampleCount = 24;
        [Header("Leaf Slash Wind")]
        [SerializeField] private ParticleSystem _fallingLeaves;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_camera);
            builder.RegisterInstance(_slashConfig);
            builder.RegisterInstance(_hitTextConfig);
            builder.RegisterInstance(_slashObjectSpawnConfig);
            builder.Register<SlashManager>(Lifetime.Singleton)
                .WithParameter(_slashPrefab)
                .WithParameter(transform);
            builder.RegisterEntryPoint<CameraStreakZoomService>(Lifetime.Singleton)
                .WithParameter(_cameraRig)
                .WithParameter(_hitEffectConfig);
            builder.RegisterEntryPoint<LeafSlashWindService>(Lifetime.Singleton)
                .WithParameter(_fallingLeaves);
            builder.Register<SlashDecalSpawner>(Lifetime.Singleton)
                .WithParameter(_hitEffectConfig)
                .WithParameter(_decalProjectorPrefab)
                .WithParameter("backgroundSlashHalfSprite", _backgroundSlashHalfSprite)
                .WithParameter("backgroundSlashTopHalfSprite", _backgroundSlashTopHalfSprite)
                .WithParameter(_slashHitLayerMask)
                .WithParameter(_slashHitSampleCount);
            builder.Register<HitTextService>(Lifetime.Singleton)
                .WithParameter(_hitTextPrefab)
                .WithParameter(_hitTextContainer);
            builder.Register<SlashObjectSpawnService>(Lifetime.Singleton)
                .WithParameter(transform);
            builder.Register<SlashGameResultService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SlashObjectService>(Lifetime.Singleton).AsSelf();
            builder.RegisterEntryPoint<InputManager>(Lifetime.Singleton);
        }
    }
}
