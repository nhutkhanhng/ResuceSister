using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace UIManager
{
    
    public abstract class UIDepth : MonoBehaviour
    {
        public int Depth
        {
            get
            {
                return transform.GetSiblingIndex();
            }

            set
            {
                transform.SetSiblingIndex(value);
            }
        }

        public void BringForward()
        {
            transform.SetSiblingIndex(Depth + 1);
        }

        public void BringBackward()
        {
            transform.SetSiblingIndex(Depth - 1);
        }

        public void MoveFront()
        {
            transform.SetAsLastSibling();
        }

        public void MoveBack()
        {
            transform.SetAsFirstSibling();
        }
    }

    public enum VisibilityState
    {
        /// <summary> Is visible </summary>
        Visible = 0,

        /// <summary> Is NOT visible </summary>
        NotVisible = 1,

        /// <summary> Is playing the HIDE animation (in transition exit view) </summary>
        Hiding = 2,

        /// <summary> Is playing the SHOW animation (in transition enter view) </summary>
        Showing = 3
    }

    public abstract class UIPanel : MonoBehaviour
    {
        public object Parameter { get; protected set; }
        [SerializeField] public float fadeInTime = .1f, fadeOutTime = .1f;
        [SerializeField] public CanvasGroup _CanvasGroup { get; protected set; }

        /// <summary> Internal variable that keeps track of this UIView's visibility state (Visible, NotVisible, Hiding or Showing) </summary>
        private VisibilityState m_visibility = VisibilityState.NotVisible;
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

        public virtual async UniTask Initialize() { }
        public virtual async UniTask _Load()
        {
            await UniTask.WaitForEndOfFrame();
        }

        public void Show(object param)
        {
            
            if (m_visibility != VisibilityState.Visible)
            {
                GetComponent<GraphicRaycaster>().enabled = false;
                this.Parameter = param;
                Shown();
                onShow?.Invoke(this);

                DOVirtual.DelayedCall(this.fadeInTime, () =>
                {
                    m_visibility = VisibilityState.Visible;
                    GetComponent<GraphicRaycaster>().enabled = true;
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
                GetComponent<GraphicRaycaster>().enabled = false;
                OnHide();
                DOVirtual.DelayedCall(fadeOutTime, () =>
                {
                    this.Visibility = VisibilityState.NotVisible;
                    this.gameObject.SetActive(false);
                });
            }
        }
        public void OnHide()
        {
            onHidden?.Invoke(this);
            Hidden();
        }

        public virtual void Hidden() { }
    }
    public abstract class UIView : UIPanel
    {
        public virtual void CloseDialog() { }

        public virtual void OnEscapPress()
        {
            CloseDialog();
        }
    }
}