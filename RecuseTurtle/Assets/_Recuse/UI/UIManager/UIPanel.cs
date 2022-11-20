using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace UIManager
{
    public enum VisibilityState
    {
        /// <summary> Is visible </summary>
        Visible = 0,

        /// <summary> Is NOT visible </summary>
        NotVisible = 1,

        /// <summary> Is playing the HIDE animation (in transition exit view) </summary>
        Hiding = 2,

        /// <summary> Is playing the SHOW animation (in transition enter view) </summary>
        Showing = 3,
    }

    public static class VisibilityExtensions
    {
        public static bool IsVisibility(this VisibilityState state) => state == VisibilityState.Showing || state == VisibilityState.Visible;
    }
    public class UIPanel : MonoBehaviour
    {
        protected UIController uiController => ToolBox.Get<UIController>();
        public object Parameter { get; protected set; }
        [SerializeField] public float fadeInTime = .01f, fadeOutTime = .01f;
        [SerializeField] public CanvasGroup _CanvasGroup { get; protected set; }

        /// <summary> Internal variable that keeps track of this UIView's visibility state (Visible, NotVisible, Hiding or Showing) </summary>
        [ReadOnly] [SerializeField] protected VisibilityState m_visibility = VisibilityState.NotVisible;
        public VisibilityState Visibility
        {
            get { return m_visibility; }
            set
            {
                m_visibility = value;
            }
        }
        public static T AddOrGet<T>(Transform mono) where T : Component
        {
            var component = mono.GetComponent<T>();
            if (component == null)
                return mono.gameObject.AddComponent<T>();

            return component;
        }


        public delegate void OnShowed(UIPanel view);
        public OnShowed onShow = null;

        public delegate void OnHidden(UIPanel view);
        public OnHidden onHidden;

        public virtual async UniTask Initialize() { await UniTask.Yield(); }
        public virtual async UniTask _Load()
        {
            await UniTask.WaitForEndOfFrame();
        }

        public void Show(object param = null)
        {

            if (m_visibility != VisibilityState.Visible)
            {
                var _raycast = GetComponent<GraphicRaycaster>();
                if (_raycast)
                    _raycast.enabled = false;
                this.gameObject.SetActive(true);

                this.Parameter = param;
                Shown();
                onShow?.Invoke(this);
                // this.transform.DOScale(1, fadeInTime);
                DOVirtual.DelayedCall(this.fadeInTime, () =>
                {
                    m_visibility = VisibilityState.Visible;
                    if (_raycast)
                        _raycast.enabled = true;
                });
            }
        }
        public delegate void OnCompleted(UIPanel view);
        public OnCompleted _onCompleted;


        public virtual void Completed()
        {
            _onCompleted?.Invoke(this);
        }
        protected virtual void Shown() { }
        public void CanvasAlPha(float alphaValue)
        {
            _CanvasGroup.alpha = alphaValue;
        }

        public void Hide()
        {
            if (this.Visibility != VisibilityState.NotVisible)
            {
                var _raycast = GetComponent<GraphicRaycaster>();
                if (_raycast)
                    _raycast.enabled = false;
                
                OnHide();
                // this.transform.DOScale(0, fadeOutTime);
                DOVirtual.DelayedCall(fadeOutTime, () =>
                {
                    this.Visibility = VisibilityState.NotVisible;
                    this.gameObject.SetActive(false);
                });
            }
        }
        protected void OnHide()
        {
            onHidden?.Invoke(this);
            Hidden();
        }

        public virtual void Hidden() { }

        protected async UniTask WaitToCall(UniTask task, System.Action callback)
        {
            await task;
            await UniTask.Yield();

            callback?.Invoke();
        }
    }
}