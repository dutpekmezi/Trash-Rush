using GameLift.Buttons;
using GameLift.Scene;
using VContainer;

namespace GameLift.UI.MainMenu
{
    public class PlayButton : BaseButton
    {
        private ISceneService _sceneService;

        [Inject]
        private void Construct(ISceneService sceneService)
        {
            _sceneService = sceneService;
        }

        public override void BaseOnClick()
        {
            base.BaseOnClick();
            _ = _sceneService.LoadScene(SceneKeys.GameScene);
        }
    }
}
