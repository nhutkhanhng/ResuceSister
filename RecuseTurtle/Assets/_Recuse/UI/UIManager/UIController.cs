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
    }
    public class UIController : MonoBehaviour
    {
        [SerializeField] protected UIViewManager _viewManager;
        public UIViewManager viewManager => _viewManager;

        [SerializeField] protected UIPopupManager _popupManager;
        [SerializeField] public UIPopupManager popupManager => _popupManager;

        protected Stack<UIPanel> _panles = new Stack<UIPanel>();


        public async void Start()
        {
            ToolBox.Set<UIController>(this);

            _viewManager = _viewManager ?? ToolBox.Get<UIViewManager>();
            _popupManager = _popupManager ?? ToolBox.Get<UIPopupManager>();

            _panles = new Stack<UIPanel>();


            await _viewManager.Initialize();
            await _popupManager.Initialize();


            ShowView("ViewHome");
        }


        public void ShowView(string viewName, object param = null)
        {
            _ShowView(viewName, param).Forget();
        }
        public void ShowView<T>(object param = null) where T : UIView
        {
            _ShowView<T>(param).Forget();
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
            UIView _view = await viewManager._AsyncShowView(viewName, param);
            _panles.Push(_view);

            return _view;
        }
        protected async UniTask<T> _ShowView<T>(object param = null) where T : UIView
        {
            T _view = await viewManager._AsyncShowView<T>(param);

            _panles.Push(_view);

            return _view;
        }
    }
}
