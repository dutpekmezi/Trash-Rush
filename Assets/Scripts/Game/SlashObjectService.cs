using System;
using GameLift.Levels;
using UnityEngine;
using VContainer.Unity;

namespace TrashRush.Game
{
    public sealed class SlashObjectService : IStartable, IDisposable
    {
        private readonly LevelService<BaseLevelData> _levelService;
        private readonly SlashObjectSpawnService _spawnService;
        private readonly SlashGameResultService _resultService;

        private SlashObject _activeObject;
        private LevelData _currentLevelData;
        private int _currentObjectIndex;
        private bool _disposed;
        private bool _isPlaying;

        public event Action<LevelData> LevelStarted;
        public event Action<SlashObject, int, int> SlashProgressChanged;
        public event Action<bool> LevelEnded;

        public bool IsPlaying => _isPlaying && !_disposed;
        public SlashObject ActiveObject => _activeObject;

        public SlashObjectService(
            LevelService<BaseLevelData> levelService,
            SlashObjectSpawnService spawnService,
            SlashGameResultService resultService)
        {
            _levelService = levelService;
            _spawnService = spawnService;
            _resultService = resultService;
        }

        public void Start()
        {
            LoadLevel();
        }

        public void Dispose()
        {
            _disposed = true;
            _isPlaying = false;
            _spawnService.Dispose();
            _activeObject = null;
            _currentLevelData = null;
            _currentObjectIndex = 0;
        }

        public bool TryRegisterSlash(SlashObject slashObject)
        {
            if (!IsPlaying ||
                slashObject == null ||
                slashObject != _activeObject ||
                !slashObject.TryRegisterSlash(out var completed))
            {
                return false;
            }

            SlashProgressChanged?.Invoke(
                slashObject,
                slashObject.CurrentSlash,
                slashObject.TotalSlash);

            if (completed)
            {
                CompleteActiveObject();
            }

            return true;
        }

        private async void LoadLevel()
        {
            try
            {
                var levelData = await _levelService.LoadNextLevelData();

                if (_disposed)
                {
                    return;
                }

                if (!(levelData is LevelData gameLevelData))
                {
                    Debug.LogError(
                        "[Trash Rush] Current level data must be a LevelData asset.");
                    return;
                }

                if (gameLevelData.SlashObjects == null ||
                    gameLevelData.SlashObjects.Count == 0)
                {
                    Debug.LogError("[Trash Rush] LevelData must contain at least one SlashObjectData.");
                    return;
                }

                _currentLevelData = gameLevelData;
                _currentObjectIndex = 0;
                _isPlaying = true;
                _levelService.OnLevelStarted();
                LevelStarted?.Invoke(gameLevelData);
                SpawnCurrentObject();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void CompleteActiveObject()
        {
            if (!IsPlaying || _activeObject == null)
            {
                return;
            }

            var completedObject = _activeObject;
            _activeObject = null;
            _spawnService.Stop(completedObject);
            completedObject.Explode();

            _currentObjectIndex++;
            SpawnCurrentObject();
        }

        private void SpawnCurrentObject()
        {
            if (!IsPlaying || _currentLevelData == null)
            {
                return;
            }

            while (_currentObjectIndex < _currentLevelData.SlashObjects.Count &&
                   _currentLevelData.SlashObjects[_currentObjectIndex] == null)
            {
                Debug.LogWarning(
                    $"[Trash Rush] Skipping null SlashObjectData at index {_currentObjectIndex}.");
                _currentObjectIndex++;
            }

            if (_currentObjectIndex >= _currentLevelData.SlashObjects.Count)
            {
                CompleteLevel();
                return;
            }

            var slashObjectData = _currentLevelData.SlashObjects[_currentObjectIndex];
            _activeObject = _spawnService.Spawn(slashObjectData, OnObjectReachedBottom);

            if (_activeObject == null)
            {
                FailLevel();
                return;
            }

            SlashProgressChanged?.Invoke(_activeObject, 0, _activeObject.TotalSlash);
        }

        private void CompleteLevel()
        {
            if (!IsPlaying)
            {
                return;
            }

            _isPlaying = false;
            _currentLevelData = null;
            LevelEnded?.Invoke(true);
            _resultService.ShowWin();
        }

        private void OnObjectReachedBottom(SlashObject escapedObject)
        {
            if (!IsPlaying || escapedObject == null || escapedObject != _activeObject)
            {
                return;
            }

            _isPlaying = false;
            _activeObject = null;
            _spawnService.Stop(escapedObject);
            escapedObject.MarkEscaped();
            UnityEngine.Object.Destroy(escapedObject.gameObject);

            _currentLevelData = null;
            LevelEnded?.Invoke(false);
            _resultService.ShowLose();
        }

        private void FailLevel()
        {
            if (!IsPlaying)
            {
                return;
            }

            _isPlaying = false;
            _activeObject = null;
            _currentLevelData = null;
            LevelEnded?.Invoke(false);
            _resultService.ShowLose();
        }
    }
}
