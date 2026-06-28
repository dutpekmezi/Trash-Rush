using Cysharp.Threading.Tasks;
using GameLift.Levels;

namespace GameLift.Scene
{
    public class SceneFlowController
    {
        private readonly ISceneService _sceneService;
        private readonly SceneServiceSettings _settings;

        public SceneFlowController(SceneServiceSettings settings, LevelList levelList, LevelService<BaseLevelData> levelService, ISceneService sceneService)
        {
            _sceneService = sceneService;
            _settings = settings;
        }

        public async UniTask LoadFirstScene()
        {
            if (_settings.DefaultSceneConfig != null)
            {
                await _sceneService.LoadScene(_settings.DefaultSceneConfig.SceneKey);
            }
        }

    }
}
