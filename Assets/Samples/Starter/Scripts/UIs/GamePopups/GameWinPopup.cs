using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameLift.Buttons;
using GameLift.Currency;
using GameLift.ObjectFlowAnimator;
using GameLift.Popup;
using GameLift.Scene;
using GameLift.UI.Currency;
using TMPro;
using UnityEngine;
using VContainer;

namespace GameLift.UI.GamePopups
{
    public class GameWinPopup : PopupBase
    {
        [SerializeField] private BaseButton _claimButton;
        private ICurrencyService _currencyService;
        private IUIFlowAnimator _uiFlowAnimator;
        private ISceneService _sceneService;

        private const string WinPopupFlyGoldKey = "win_popup_fly_gold";

        public override string PopupId => PopupIdConst;
        public const string PopupIdConst = "game_win_popup";


        [Inject]
        private void Construct(ICurrencyService currencyService, IUIFlowAnimator uIFlowAnimator, IObjectResolver resolver, ISceneService sceneService)
        {
            _currencyService = currencyService;
            _uiFlowAnimator = uIFlowAnimator;
            _sceneService = sceneService;

            resolver.Inject(_claimButton);
            _claimButton.Button.onClick.AddListener(OnClaimButtonClicked);
        }

        private void OnClaimButtonClicked()
        {
            _claimButton.Button.interactable = false;
            _ = AnimateRewards();
        }

        private async UniTask AnimateRewards()
        {
            if (TryFlyGold(40))
            {
                await UniTask.WaitUntil(() => !_uiFlowAnimator.IsPlaying(WinPopupFlyGoldKey));
            }

            Disappear();
            await _sceneService.LoadScene(SceneKeys.GameScene);
        }

        private bool TryFlyGold(int gold)
        {
            var currencyBars = FindObjectsByType<CurrencyBar>(FindObjectsSortMode.None);
            var currencyBar = currencyBars.FirstOrDefault(s => s.isActiveAndEnabled == true && s.CurrencyId == CurrencyIds.Gold);

            if (currencyBar == null)
            {
                return false;
            }

            float amountPerParticleCount = 1;

            float particleReceiveCount = gold;

            var currencyConfig = _currencyService.GetCurrencyConfig(CurrencyIds.Gold);

            if (currencyConfig == null)
            {
                return false;
            }

            _currencyService.ModifyCurrency(CurrencyIds.Gold, gold, addFakeDecrease: true);

            _uiFlowAnimator.AddNewDestinationAction(
                WinPopupFlyGoldKey,
                startScreenPos: _claimButton.transform.position,
                endScreenPos: currencyBar.transform.position,
                sprite: currencyConfig.currencySprite,
                particleCount: (int)particleReceiveCount,
                onReceivedItem: () =>
                {
                    _currencyService.AddFakeCurrency(currencyConfig.currencyId, amountPerParticleCount);
                    if (currencyBar != null)
                        currencyBar.Jump();
                },
                onCompleted: () =>
                {
                    _currencyService.ClearFakeCurreny(CurrencyIds.Gold);
                }
            );

            return true;
        }
    }
}
