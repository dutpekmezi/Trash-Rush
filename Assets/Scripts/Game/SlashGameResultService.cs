using GameLift.Levels;
using GameLift.Popup;
using GameLift.UI.GamePopups;

namespace TrashRush.Game
{
    public sealed class SlashGameResultService
    {
        private readonly LevelService<BaseLevelData> _levelService;
        private readonly IPopupService _popupService;

        private bool _resultShown;

        public SlashGameResultService(
            LevelService<BaseLevelData> levelService,
            IPopupService popupService)
        {
            _levelService = levelService;
            _popupService = popupService;
        }

        public void ShowWin()
        {
            if (_resultShown)
            {
                return;
            }

            _resultShown = true;
            _levelService.OnLevelCompleted(true);
            _popupService.Create<GameWinPopup>(true);
        }

        public void ShowLose()
        {
            if (_resultShown)
            {
                return;
            }

            _resultShown = true;
            _levelService.OnLevelCompleted(false);
            _popupService.Create<GameLosePopup>(true);
        }
    }
}
