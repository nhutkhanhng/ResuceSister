using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;
using DG.Tweening;
using NaughtyAttributes;
using ToolKit;

namespace UIManager
{
    public abstract class UILoader<uiBase> : MonoBehaviour where uiBase : UIPanel
    {
        [SerializeField] protected bool Initialized = false;
        [SerializeField] protected Transform transHolder;
        protected Dictionary<string, uiBase> m_uiElements = new Dictionary<string, uiBase>();

        [SerializeField] protected List<string> Essentials = new List<string>();
        [SerializeField] protected List<string> Default = new List<string>();

        // Load from resource
        protected abstract string Expression();

        public void Show<T>(object param = null) where T : uiBase
        {
            T _popup = null;
            UniTask.Action(async () => _popup = await AsyncShow<T>(param)).Invoke();
        }

        public async UniTask<uiBase> AsyncShow(string uiName, object param = null)
        {
            uiBase _view = await GetOrLoad(uiName);
            if (_view == null)
            {
                return null;
            }

            _ExectorShow(_view, param);

            return _view;
        }
        public async UniTask<T> AsyncShow<T>(object param = null) where T : uiBase
        {
            T _view = await GetOrLoad<T>() as T;
            if (_view == null)
            {
                return null;
            }

            _ExectorShow(_view, param);

            return _view as T;
        }

        protected virtual void _ExectorShow(uiBase uiElement, object param)
        {
            uiElement.transform.SetAsLastSibling();
            uiElement.gameObject.SetActive(true);

            uiElement.Show(param);
        }

        public async UniTask Hide(uiBase ui)
        {
            if (ui != null)
            {
                ui.Hide();
                await UniTask.WaitUntil(() => ui.Visibility == VisibilityState.NotVisible);
                ui.gameObject.SetActive(false);
            }
        }

        public void Despawn(uiBase ui)
        {
            ui.gameObject.SetActive(false);
            ui.transform.SetParent(transHolder ?? this.transform);
        }

        // =====================================================================================

        protected async UniTask<T> Load<T>() where T : uiBase
        {
            try
            {
                var popupAsset = Resources.LoadAsync<T>(string.Format(Expression(), (typeof(T).ToString())));
                await UniTask.WaitUntil(() => popupAsset.isDone == true);

                T _view = null;
                // UniTask.Action(async () => _view = await __AddView<UIView>(popupAsset)).Invoke();
                _view = await __InstanceView(popupAsset) as T;
                return _view;
            }
            catch
            {
                Debug.LogError("find not found " + typeof(uiBase).ToString());
                return null;
            }
        }
        protected async UniTask<uiBase> Load(string uiName)
        {
            try
            {
                var viewAsset = Resources.LoadAsync<uiBase>(string.Format(Expression(), uiName));
                // string.Format(Expression, viewName));

                await UniTask.WaitUntil(() => viewAsset.isDone == true);
                uiBase _uiView;
                // UniTask.Action(async () => _uiView = await __AddView<UIView>(viewAsset)).Invoke();
                _uiView = await __InstanceView(viewAsset);

                return _uiView;
            }
            catch
            {
                Debug.LogError(string.Format(Expression(), uiName) + " Load asset fail");
                return null;
            }

            return null;
        }

        protected async UniTask<uiBase> __InstanceView(ResourceRequest _ResourceRequested)
        {
            var _popupAsset = ((uiBase)_ResourceRequested.asset);
            var popup = UnityEngine.Object.Instantiate((uiBase)_ResourceRequested.asset, transHolder ?? this.transform);

            popup.gameObject.SetActive(false);
            popup.transform.name = popup.transform.name.Replace("(Clone)", string.Empty);
            popup.transform.SetAsFirstSibling();

            await popup.Initialize();


            Add(_ResourceRequested.asset.name, popup);

            return popup;
        }

        public async UniTask<T> GetOrLoad<T>() where T : uiBase
        {
            T _view = Get<T>() as T;
            if (_view)
            {
                // AddView(viewName, _uiView);                
            }
            else
            {
                try
                {
                    _view = await Load<T>() as T;
                    await _view.Initialize();

                    Add(_view.GetType().ToString(), _view);


                    if (_view)
                        _view.gameObject.SetActive(false);

                    return _view;
                    // _uiView.Hide();
                }
                catch { Debug.LogError(typeof(T).ToString()); }
            }

            return _view;
        }
        public async UniTask<uiBase> GetOrLoad(string uiName)
        {
            var _uiView = Get(uiName);
            if (_uiView)
            {
                // AddView(viewName, _uiView);                
            }
            else
            {
                try
                {
                    _uiView = await Load(uiName);
                    await _uiView.Initialize();

                    Add(uiName, _uiView);


                    if (_uiView)
                        _uiView.gameObject.SetActive(false);

                    return _uiView;
                    // _uiView.Hide();
                }
                catch { }
            }

            return _uiView;
        }

        protected T Get<T>() where T : uiBase
        {
            foreach (var v in m_uiElements.Values)
            {
                if (v is T)
                {
                    return v as T;
                }
            }

            return null;
        }
        protected uiBase Get(string name)
        {
            if (!m_uiElements.ContainsKey(name)) return null;
            return m_uiElements[name];
        }

        protected void Remove(string key)
        {
            if (!m_uiElements.ContainsKey(key)) return;
            Destroy(m_uiElements[key].gameObject);
            m_uiElements.Remove(key);
        }
        protected void Add(string key, uiBase view)
        {
            if (m_uiElements.ContainsKey(key)) m_uiElements[key] = view;
            else m_uiElements.Add(key, view);
        }

        // =====================================================================================


        protected UniTask LoadUIElentals(List<string> uiNames)
        {
            List<UniTask> allView = new List<UniTask>();

            uiNames.ForEach(x => allView.Add(GetOrLoad(x)));

            return UniTask.WhenAll(allView);
        }

        public async UniTask Initialize()
        {
            Initialized = false;
            
            await LoadUIElentals(this.Essentials);
            LoadUIElentals(this.Default).Forget();

            Initialized = true;
        }
    }




    public class UIPanelLoader : UILoader<UIPanel>
    {
        protected override string Expression()
        {
            return "Panles/{0}";
        }
    }


}