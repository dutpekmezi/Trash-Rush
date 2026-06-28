using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TrashRush.Game
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class HitText : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _label;

        private Sequence _sequence;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnDestroy()
        {
            _sequence?.Kill(false);
        }

        private void OnValidate()
        {
            EnsureReferences();
        }

        public void Play(
            Vector2 anchoredPosition,
            float zRotation,
            int streak,
            HitTextConfig config)
        {
            EnsureReferences();

            if (_rectTransform == null ||
                _canvasGroup == null ||
                _label == null ||
                config == null)
            {
                Destroy(gameObject);
                return;
            }

            _sequence?.Kill(false);
            _rectTransform.anchoredPosition = anchoredPosition;
            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
            _rectTransform.localScale = Vector3.one * config.GetScaleMultiplier(streak);
            _label.text = $"{Mathf.Max(1, streak)}!";
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            var targetPosition = anchoredPosition + Vector2.up * config.RiseDistance;
            var fadeTween = DOVirtual
                .Float(1f, 0f, config.FadeDuration, value => _canvasGroup.alpha = value)
                .SetEase(config.FadeEase);

            _sequence = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_rectTransform
                    .DOAnchorPos(targetPosition, config.MoveDuration)
                    .SetEase(config.MoveEase))
                .Insert(config.FadeDelay, fadeTween)
                .OnComplete(() => Destroy(gameObject));

            if (config.UseUnscaledTime)
            {
                _sequence.SetUpdate(true);
            }
        }

        private void EnsureReferences()
        {
            if (_rectTransform == null)
            {
                _rectTransform = transform as RectTransform;
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_label == null)
            {
                _label = GetComponent<TMP_Text>();
            }
        }
    }
}
