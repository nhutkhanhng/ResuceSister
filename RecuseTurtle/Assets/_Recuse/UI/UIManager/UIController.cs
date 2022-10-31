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
    public class UIController : MonoBehaviour
    {
        [SerializeField] public UIViewManager viewManager { get; protected set; }
        [SerializeField] public UIPopupManager popupManager { get; protected set; }


        protected Stack<UIPanel> _panles = new Stack<UIPanel>();


        public async void Start()
        {
            ToolBox.Set<UIController>(this);

            viewManager = viewManager ?? ToolBox.Get<UIViewManager>();
            popupManager = popupManager ?? ToolBox.Get<UIPopupManager>();

            _panles = new Stack<UIPanel>();
        }


        public void ShowView<T>(object param = null) where T : UIView
        {
            _ShowView<T>(param).Forget();
        }

        protected async UniTask<T> _ShowView<T>(object param = null) where T : UIView
        {
            T _view = await viewManager._AsyncShowView<T>(param);

            _panles.Push(_view);

            return _view;
        }
    }
}
