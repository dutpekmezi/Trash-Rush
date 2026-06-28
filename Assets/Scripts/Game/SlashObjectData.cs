using DG.Tweening;
using UnityEngine;

namespace TrashRush.Game
{
    [CreateAssetMenu(fileName = "SlashObjectData", menuName = "Trash Rush/Game/Slash Object Data")]
    public sealed class SlashObjectData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _id;
        [SerializeField] private SlashObject _prefab;
        [SerializeField] private Texture2D _slashTexture;
        [SerializeField, Min(1)] private int _totalSlash = 1;
        [Header("Fall")]
        [SerializeField, Min(0.01f)] private float _fallDuration = 9f;
        [SerializeField] private Ease _fallEase = Ease.Linear;
        [Header("Cell Fracture")]
        public GameObject CellFractured;
        [SerializeField, Min(0f)] private float _cellFractureForce = 5f;
        [SerializeField, Min(0f)] private float _cellFractureDestroyDelay = 2f;
        [SerializeField, Min(0f)] private float _cellFracturedDestroyDelay = 2f;

        public string Name => _displayName;
        public string DisplayName => Name;
        public string Id => _id;
        public SlashObject Prefab => _prefab;
        public Texture2D SlashTexture => _slashTexture;
        public int TotalSlash => Mathf.Max(1, _totalSlash);
        public float FallDuration => Mathf.Max(0.01f, _fallDuration);
        public Ease FallEase => _fallEase;
        public float CellFractureForce => Mathf.Max(0f, _cellFractureForce);
        public float CellFractureDestroyDelay => Mathf.Max(0f, _cellFractureDestroyDelay);
        public float CellFracturedDestroyDelay => Mathf.Max(0f, _cellFracturedDestroyDelay);
    }
}
