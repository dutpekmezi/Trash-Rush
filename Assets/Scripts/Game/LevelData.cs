using System.Collections.Generic;
using GameLift.Levels;
using UnityEngine;

namespace TrashRush.Game
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Trash Rush/Game/Level Data")]
    public sealed class LevelData : BaseLevelData
    {
        [SerializeField] private List<SlashObjectData> _slashObjects = new();

        public IReadOnlyList<SlashObjectData> SlashObjects => _slashObjects;
    }
}
