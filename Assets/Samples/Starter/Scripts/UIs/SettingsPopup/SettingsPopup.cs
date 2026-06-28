using GameLift.Buttons;
using GameLift.Popup;
using GameLift.Purchasing;
using GameLift.Settings;
using TMPro;
using UnityEngine;
using VContainer;

namespace GameLift.UI.GamePopups
{
    public class SettingsPopup : PopupBase
    {
        public override string PopupId => PopupIdConst;
        public const string PopupIdConst = "settings_popup";

        [Header("Vibration Toggle")]
        [SerializeField] private BaseButton _vibrationButton;
        [SerializeField] private GameObject _vibrationOnVisual;
        [SerializeField] private GameObject _vibrationOffVisual;

        [Header("Audio Toggle")]
        [SerializeField] private BaseButton _audioButton;
        [SerializeField] private GameObject _audioOnVisual;
        [SerializeField] private GameObject _audioOffVisual;

        [Header("Music Toggle")]
        [SerializeField] private BaseButton _musicButton;
        [SerializeField] private GameObject _musicOnVisual;
        [SerializeField] private GameObject _musicOffVisual;

        [Header("Restore Purchases")]
        [SerializeField] private BaseButton _restorePurchasesButton;
        [SerializeField] private TMP_Text _restorePurchasesLabel;

        [Header("Settings Labels")]
        [SerializeField] private TMP_Text _settingsTitleLabel;
        [SerializeField] private TMP_Text _quitLabel;

        [Header("Quit")]
        [SerializeField] private BaseButton _quitButton;

        private GameSettingsService _settingsService;
        private IPurchasingService _purchasingService;

        [Inject]
        private void Construct(GameSettingsService settingsService, IPurchasingService purchasingService, IObjectResolver resolver)
        {
            _settingsService = settingsService;
            _purchasingService = purchasingService;

            resolver.Inject(_vibrationButton);
            resolver.Inject(_audioButton);
            resolver.Inject(_musicButton);
            resolver.Inject(_restorePurchasesButton);

            _quitButton.gameObject.SetActive(false);

            RefreshAllVisuals();
            WireButtons();
        }

        private void WireButtons()
        {
            _vibrationButton.Button.onClick.AddListener(OnVibrationClicked);
            _audioButton.Button.onClick.AddListener(OnAudioClicked);
            _musicButton.Button.onClick.AddListener(OnMusicClicked);
            _restorePurchasesButton.Button.onClick.AddListener(OnRestorePurchasesClicked);

        }

        private void OnVibrationClicked()
        {
            _settingsService.ToggleVibration();
            RefreshVisual(_vibrationOnVisual, _vibrationOffVisual, _settingsService.IsVibrationEnabled);
        }

        private void OnAudioClicked()
        {
            _settingsService.ToggleAudio();
            RefreshVisual(_audioOnVisual, _audioOffVisual, _settingsService.IsAudioEnabled);
        }

        private void OnMusicClicked()
        {
            _settingsService.ToggleMusic();
            RefreshVisual(_musicOnVisual, _musicOffVisual, _settingsService.IsMusicEnabled);
        }

        private void OnRestorePurchasesClicked()
        {
            _purchasingService.RestorePurchases();
        }

        private void RefreshAllVisuals()
        {
            RefreshVisual(_vibrationOnVisual, _vibrationOffVisual, _settingsService.IsVibrationEnabled);
            RefreshVisual(_audioOnVisual, _audioOffVisual, _settingsService.IsAudioEnabled);
            RefreshVisual(_musicOnVisual, _musicOffVisual, _settingsService.IsMusicEnabled);
        }

        private static void RefreshVisual(GameObject onVisual, GameObject offVisual, bool enabled)
        {
            if (onVisual != null) onVisual.SetActive(enabled);
            if (offVisual != null) offVisual.SetActive(!enabled);
        }
    }
}
