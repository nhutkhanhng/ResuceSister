using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;
using DG.Tweening;
using ToolKit;

namespace UIManager
{
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject)
    where T : Component
        {
#if UNITY_2019_2_OR_NEWER
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
            }
#else
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
#endif

            return component;
        }

        public static void ResetLocalTransform(this Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }

    }
    public class UIController : MonoBehaviour
    {
        [SerializeField] protected UIViewLoader _viewManager;
        public UIViewLoader viewManager => _viewManager;

        [SerializeField] protected UIPopupManager _popupManager;
        [SerializeField] public UIPopupManager popupManager => _popupManager;
        [SerializeField] protected UIPanelLoader _panelManager;

        protected Stack<UIPanel> _panles = new Stack<UIPanel>();


        public async void Start()
        {
            ToolBox.Set<UIController>(this);

            _viewManager    = _viewManager    ?? ToolBox.Get<UIViewLoader>();
            _popupManager   = _popupManager   ?? ToolBox.Get<UIPopupManager>();
            _panelManager   = _panelManager   ?? ToolBox.Get<UIPanelLoader>();

            _panles = new Stack<UIPanel>();

            await _panelManager.Initialize();

            await _viewManager.Initialize();
            await _popupManager.Initialize();


            var _view = await AsyncViewShow<ViewHome>();
        }


        public void ShowView(string viewName, object param = null)
        {
            _ShowView(viewName, param).Forget();
        }
        public void ShowView<T>(object param = null) where T : UIView
        {
            AsyncViewShow<T>(param).Forget();
        }

        protected void SetupPreviousViews()
        {
            UIPanel _prev = _panles.Peek();
            if (_prev is UIView)
            {
                var _canvasGroup = _prev.gameObject.GetOrAddComponent<CanvasGroup>();
            }
        }
        protected async UniTask<UIView> _ShowView(string viewName, object param = null)
        {
            UIView _view = await viewManager.AsyncShow(viewName, param);
            _panles.Push(_view);

            return _view;
        }
        protected async UniTask<T> AsyncViewShow<T>(object param = null) where T : UIView
        {
            T _view = await viewManager.AsyncShow<T>(param);

            _panles.Push(_view);

            return _view;
        }

        public async UniTask<UIView> AsyncViewShow(string uiName)
        {
            UIView _view = await viewManager.AsyncShow(uiName);

            _panles.Push(_view);

            return _view;
        }

        public async UniTaskVoid AttachPanelToPanel(UIPanel mainPanel, UIPanel subPanel)
        {
            var _group = mainPanel.gameObject.GetOrAddComponent<UIGroupPanels>();
            _group.Attach(subPanel);

            subPanel.transform.SetParent(mainPanel.transform);
            subPanel.gameObject.SetActive(true);

            subPanel.transform.ResetLocalTransform();

            subPanel.Show();
        }
    }
}
