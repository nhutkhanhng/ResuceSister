using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using Redcode.Extensions;

namespace UIManager
{
    public class UIFloatingText : SingletonMono<UIFloatingText>
    {
        [SerializeField] protected Text Text;

        [SerializeField] protected float yLocal;
        [SerializeField] protected float duration;
        [SerializeField] Ease easeType;

        private void Awake()
        {
            Text.transform.localScale = Vector3.zero;
            Text.transform.gameObject.SetActive(false);
            defaultFontColor = Text.color;
        }

        protected Color defaultFontColor;

        public void Show(string announcement, Color fontColor, System.Action callback = null, float delayToHide = 2f)
        {
            var _newText = GameObject.Instantiate<Text>(Text, Text.transform.parent);
            _newText.color = fontColor;
            Show(_newText, announcement, yLocal, callback, delayToHide);
        }

        public void ShowRevert(string announcement, Color fontColor, System.Action callback = null, float delayToHide = 2f)
        {
            var _newText = GameObject.Instantiate<Text>(Text, Text.transform.parent);
            _newText.color = fontColor;
            _newText.transform.localPosition = _newText.transform.localPosition.WithY
                (Screen.height / 2f - yLocal);

            Show(_newText, announcement, (Screen.height / 2f - yLocal) - 30f, callback, delayToHide);
        }
        protected Sequence _sequence;
        protected void Show(Text _txtComp, string announcement,
            float yTarget,
            System.Action callback = null, 
            float delayToHide = 2f)
        {
            if (_txtComp)
            {
                _txtComp.text = announcement;
                // _txtComp.color = defaultFontColor;
                _txtComp.transform.DOScale(1f, Mathf.Min(1f, duration)).SetEase(Ease.OutBounce);

                //if (_sequence != null && _sequence.IsPlaying())
                //{
                //    _txtComp.transform.gameObject.SetActive(false);
                //    _sequence.Kill();
                //}

                _sequence = DOTween.Sequence();
                _txtComp.transform.gameObject.SetActive(true);
                _sequence.Append(_txtComp.transform.DOLocalMoveY(yTarget, duration).SetEase(easeType));
                _sequence.Join(
                        _txtComp.transform.DOScale(0f, 1f).SetEase(Ease.OutBack)
                        .SetDelay(duration * .8f)
                    );

                _sequence.OnComplete(() => { callback?.Invoke();
                    Destroy(_txtComp.gameObject);});
                _sequence.Play();
            }
        }

    }
}