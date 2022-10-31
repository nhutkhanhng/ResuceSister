using UnityEngine;

namespace UIManager
{
    using DG.Tweening;
    using Redcode.Extensions;
    using TMPro;
    using UnityEngine.UI;

    public class UIViewLoading : UIView
    {
        [SerializeField] private Image iconLevel;

        [SerializeField] private CanvasGroup Background;
        [SerializeField] private Image progressImage;
        [SerializeField] private TextMeshProUGUI progressTitle;
        [SerializeField] private string URLPrivacyPolicy;

        public float CurrentProgress => progressImage.fillAmount;
        protected override void Shown()
        {
            iconLevel.color = iconLevel.color.WithA(0);

            Background.DOFade(1, Mathf.Min(.15f, this.fadeInTime));
        }
        public void ShowProgress(float progress)
        {
            if(progressImage)
                progressImage.fillAmount = progress;
            progressTitle.text = (progress * 100).ToString("F2") + "%";

            if (iconLevel)
                iconLevel.color = iconLevel.color.WithA(progress);
        }

        public override void Hidden()
        {
            base.Hidden();
            Background.DOFade(0, Mathf.Max(.1f, this.fadeOutTime - .1f));
        }

        public void OnClickPrivacy()
        {
            Application.OpenURL(URLPrivacyPolicy);
        }
    }
}