using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;
using DG.Tweening;

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

        [SerializeField] protected UIPopupLoader _popupManager;
        [SerializeField] public UIPopupLoader popupManager => _popupManager;
        [SerializeField] protected UIPanelLoader _panelManager;


        public async void Start()
        {
            ToolBox.Set<UIController>(this);

            _viewManager    = _viewManager    ?? ToolBox.Get<UIViewLoader>();
            _popupManager   = _popupManager   ?? ToolBox.Get<UIPopupLoader>();
            _panelManager   = _panelManager   ?? ToolBox.Get<UIPanelLoader>();

            await _panelManager.Initialize();

            await _viewManager.Initialize();
            await _popupManager.Initialize();
            // var _view = await AsyncViewShow<ViewHome>();
        }

        public void ShowPopup<T>(object param, System.Action<T> result = null) where T : UIPopup
        {
            _ShowPopup<T>(param, result).Forget();
        }

        public async UniTask<T> _ShowPopup<T>( object param = null, System.Action<T> result = null) where T : UIPopup
        {
            T _view = await popupManager.AsyncShow<T>(param);

            result?.Invoke(_view);

            return _view;
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

        }
        protected async UniTask<UIView> _ShowView(string viewName, object param = null)
        {
            UIView _view = await viewManager.AsyncShow(viewName, param);

            return _view;
        }
        protected async UniTask<T> AsyncViewShow<T>(object param = null) where T : UIView
        {
            T _view = await viewManager.AsyncShow<T>(param);

            return _view;
        }

        public async UniTask<UIView> AsyncViewShow(string uiName, System.Action<UIView> result = null)
        {
            UIView _view = await viewManager.AsyncShow(uiName);
            result?.Invoke(_view);
            return _view;
        }

        public UIView LoadViewByName(string uiName)
        {
            return viewManager.Get(uiName);
        }

        public async UniTask<UIView> LoadView(string uiName, System.Action<UIView> result = null)
        {
            UIView _view = await viewManager.GetOrLoad(uiName);
            result?.Invoke(_view);
            return _view;
        }

        public void Hide(UIView view)
        {
            viewManager.Hide(view).Forget();
        }
        public void Hide(UIPopup popup) { popupManager.Hide(popup).Forget(); }
        // public void Hide<T>() where T : UIPopup { popupManager.Hide<T>().Forget(); }

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
