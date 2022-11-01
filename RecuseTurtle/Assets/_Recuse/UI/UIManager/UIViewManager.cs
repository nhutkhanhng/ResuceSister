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
    public class UIViewManager : MonoBehaviour
    {
        public Camera UICam;
        const string POPUP_BASE_MESSAGE = "Views/";
        const string Expression = "Views/{0}";
        protected Canvas _canvas;
        public Canvas MasterCanvas { get; private set; }

        protected Dictionary<string, UIView> m_views = new Dictionary<string, UIView>();

        [SerializeField] public Transform ViewHolder;


        [SerializeField] protected List<string> essentials = new List<string>();
        [SerializeField] protected List<string> defaultInit = new List<string>();

        public bool Initialized = false;

        public async UniTask Initialize()
        {
            Initialized = false;
            MasterCanvas = GetComponent<Canvas>();
            await LoadAllViews();

            Initialized = true;
        }
        protected async UniTask EndLoaded()
        {
            defaultInit.ForEach(x => GetOrLoadView(x).Forget());
        }

        protected UniTask LoadEssentials()
        {
            List<UniTask> allView = new List<UniTask>();

            essentials.ForEach(x => allView.Add(GetOrLoadView(x)));

            return UniTask.WhenAll(allView);
        }
        public async UniTask LoadAllViews()
        {
            var _load = LoadEssentials();
            await _load;

            EndLoaded().Forget();
        }

        public async UniTask<UIView> ShowView(string viewName)
        {
            var _view = await GetOrLoadView(viewName);

             _view.Show(null);
            return _view;
        }

        protected async UniTask<T> GetOrLoadView<T>() where T : UIView
        {
            T _view = GetView<T>();
            if (_view)
            {
                // AddView(viewName, _uiView);                
            }
            else
            {
                try
                {
                    _view = await LoadView<T>();
                    await _view._Load();

                    AddView(_view.GetType().ToString(), _view);


                    if (_view)
                        _view.gameObject.SetActive(false);

                    return _view;
                    // _uiView.Hide();
                }
                catch { Debug.LogError(typeof(T).ToString()); }
            }

            return _view;
        }
        protected async UniTask<UIView> GetOrLoadView(string viewName)
        {
            var _uiView = GetView(viewName);
            if (_uiView)
            {
                // AddView(viewName, _uiView);                
            }
            else
            {
                try
                {
                    _uiView = await LoadView(viewName);
                    await _uiView._Load();

                    AddView(viewName, _uiView);


                    if (_uiView)
                        _uiView.gameObject.SetActive(false);

                    return _uiView;
                    // _uiView.Hide();
                }
                catch {  }
            }

            return _uiView;
        }

        protected async UniTask<T> __InstanceView<T>(ResourceRequest _ResourceRequested) where T : UIView
        {
            var _popupAsset = ((T)_ResourceRequested.asset);

            _popupAsset.gameObject.SetActive(false);

            var popup = UnityEngine.Object.Instantiate((T)_ResourceRequested.asset, ViewHolder ?? this.transform);

            popup.transform.name = popup.transform.name.Replace("(Clone)", string.Empty);
            popup.transform.SetAsFirstSibling();

            await popup.Initialize();


            AddView(_ResourceRequested.asset.name, popup);

            return popup;
        }
        protected async UniTask<T> LoadView<T>() where T : UIView
        {
            try
            {
                var popupAsset = Resources.LoadAsync<T>(string.Format(Expression, (typeof(T).ToString())));
                await UniTask.WaitUntil(() => popupAsset.isDone == true);

                T _view = null;
                // UniTask.Action(async () => _view = await __AddView<UIView>(popupAsset)).Invoke();
                _view = await __InstanceView<T>(popupAsset);
                return _view as T;
            }
            catch
            {
                Debug.LogError("find not found " + typeof(T).ToString());
                return null;
            }
        }
        protected async UniTask<UIView> LoadView(string viewName)
        {
            try
            {
                var viewAsset = Resources.LoadAsync<UIView>($"Views/{viewName}");
                    // string.Format(Expression, viewName));

                await UniTask.WaitUntil(() => viewAsset.isDone == true);
                UIView _uiView;
                // UniTask.Action(async () => _uiView = await __AddView<UIView>(viewAsset)).Invoke();
                _uiView = await __InstanceView<UIView>(viewAsset);

                return _uiView;
            }
            catch
            {
                Debug.LogError(string.Format(Expression, viewName)  + " Load asset fail");
                return null;
            }

            return null;
        }
        public async UniTaskVoid ShowLoadingForProcess(UniTask process)
        {
            var  _view = await _AsyncShowView<UIViewLoading>();

            await process;

            await UniTask.Delay(TimeSpan.FromSeconds(1f), ignoreTimeScale: false);
            await HideView(_view);
        }


        public void ShowView<T>(object param = null) where T : UIView
        {
            T _popup = null;
            UniTask.Action(async () => _popup = await _AsyncShowView<T>(param)).Invoke();
        }
        public async UniTask<UIView> _AsyncShowView(string viewName, object param = null)
        {
            UIView _view = await GetOrLoadView(viewName);
            if (_view == null)
            {
                return null;
            }

            _view.transform.SetAsLastSibling();
            _view.gameObject.SetActive(true);

            _view.Show(param);
            return _view;
        }

        public async UniTask<T> _AsyncShowView<T>(object param = null) where T : UIView
        {
            T _view = await GetOrLoadView<T>();
            if (_view == null)
            {
                return null;
            }

            _view.transform.SetAsLastSibling();
            _view.gameObject.SetActive(true);

            _view.Show(param);
            return _view as T;
        }
        //VIEW
        public UIView ShowView(string viewName, bool instantAction = false, object param = null)
        {
            var view = GetView(viewName);
                            
            if (view != null)
            {
                if (!view.gameObject.activeSelf)
                {
                    view.gameObject.SetActive(true);
                }
                view.gameObject.SetActive(true);
                view.transform.SetAsLastSibling();
                view.Show(param);
            }
            else
            {
                Debug.LogError(viewName);
            }

            return view;
        }


        //public async UniTaskVoid ShowLoadingForProcess(UniTask process, System.Action callback = null)
        //{
        //    var sceneTask = UniTask.Create(async () =>
        //    {
        //        await AssetBundleManager.Instance.LoadScene(SCENE_NAME.Home, LoadSceneMode.Additive);
        //    });
        //    ShowLoadingForProcess(sceneTask).Forget();

        //    var progress =
        //       new Progress<float>(((ViewLoading)UIManager.Instance.GetView(VIEW_NAME.Loading)).ShowProgress);
        //    await UniTask.WhenAll(AssetBundleManager.Instance.UnloadScene(SCENE_NAME.Home),
        //        AssetBundleManager.Instance.LoadScene(SCENE_NAME.TestScene, LoadSceneMode.Additive, progress));

        //    ((UIViewLoading)ShowView(VIEW_NAME.Loading, true)).ShowProgress(process);
        //    await process;
        //    HideView(VIEW_NAME.Loading, true);

        //    callback?.Invoke();
        //}
        public async UniTask HideView(UIView view, bool instantAction = false)
        {
            if (view != null)
            {
                view.Hide();
                await UniTask.WaitUntil(() => view.Visibility == VisibilityState.NotVisible);
                view.gameObject.SetActive(false);
                // RemoveView(name);
            }
        }
        public async UniTask HideView(string name, bool instantAction = false)
        {
            var view = GetView(name);
            if (view != null)
            {
                view.Hide();
                await UniTask.WaitUntil(() => view.Visibility == VisibilityState.NotVisible);
                view.gameObject.SetActive(false);
                // RemoveView(name);
            }
        }

        public async UniTask HideAllView(Type[] exceptView)
        {
            var tasks = new List<UniTask>();
            foreach (var _value in m_views.Values)
            {
                if (exceptView != null && exceptView.Contains(_value.GetType())) continue;
                tasks.Add(HideView(_value));
            }

            await UniTask.WhenAll(tasks);
        }
        public async UniTask HideAllView(string[] exceptView = null, bool instantAction = false)
        {
            var tasks = new List<UniTask>();
            foreach (var key in m_views.Keys)
            {
                if (exceptView != null && exceptView.Contains(key)) continue;
                tasks.Add(HideView(key, instantAction));
            }

            await UniTask.WhenAll(tasks);
        }

        public UIView GetView(string name)
        {
            if (!m_views.ContainsKey(name)) return null;
            return m_views[name];
        }

        public T GetView<T>() where T : UIView
        {
            foreach(var v in m_views.Values)
            {
                if (v is T)
                {
                    return v as T;
                }
            }

            return null;
        }
        protected void AddView(string key, UIView view)
        {
            if (m_views.ContainsKey(key)) m_views[key] = view;
            else m_views.Add(key, view);
        }

        protected void RemoveView(string key)
        {
            if (!m_views.ContainsKey(key)) return;
            Destroy(m_views[key].gameObject);
            m_views.Remove(key);
        }
    }
}