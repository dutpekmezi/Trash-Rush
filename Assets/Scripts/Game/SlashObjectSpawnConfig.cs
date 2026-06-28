using UnityEngine;

namespace TrashRush.Game
{
    [CreateAssetMenu(
        fileName = "SlashObjectSpawnConfig",
        menuName = "Trash Rush/Game/Slash Object Spawn Config")]
    public sealed class SlashObjectSpawnConfig : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] private float _viewportX = 0.5f;
        [SerializeField] private float _spawnViewportY = 1.2f;
        [SerializeField] private float _despawnViewportY = -0.2f;
        [SerializeField] private float _worldDepth = 33.4f;
        [SerializeField] private bool _useUnscaledTime;

        public float ViewportX => Mathf.Clamp01(_viewportX);
        public float SpawnViewportY => _spawnViewportY;
        public float DespawnViewportY => _despawnViewportY;
        public float WorldDepth => _worldDepth;
        public bool UseUnscaledTime => _useUnscaledTime;
    }
}
