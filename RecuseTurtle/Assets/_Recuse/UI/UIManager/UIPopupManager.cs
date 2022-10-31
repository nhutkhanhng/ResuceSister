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
    public class UIPopupManager : MonoBehaviour
    {
        protected Dictionary<string, UIPopup> m_popups = new Dictionary<string, UIPopup>();
        [SerializeField] public Transform PopupHolder;

        [SerializeField] protected List<string> essentials = new List<string>();
        [SerializeField] protected List<string> defaultInit = new List<string>();
        public bool Initialized = false;

        protected UIPopup GetPopup(string name)
        {
            if (!m_popups.ContainsKey(name)) return null;
            return m_popups[name];
        }

        protected T GetPopup<T>() where T : UIPopup
        {
            foreach (var kvp in m_popups)
            {
                if (kvp.Value.GetType().IsEquivalentTo(typeof(T)))
                {
                    return kvp.Value as T;
                }
            }

            return null;
        }
        public void Start()
        {
            ToolBox.Set<UIPopupManager>(this);

            Initialized = false;

            InitStarted().Forget();
        }

        public async UniTask InitStarted()
        {
            var _load = LoadEssentials();
            await _load;

            EndLoaded().Forget();
            await UniTask.NextFrame();
            Initialized = true;
        }


        protected async UniTask EndLoaded()
        {
            defaultInit.ForEach(x => LoadPopup(x).Forget());
        }

        protected UniTask LoadEssentials()
        {
            List<UniTask> allView = new List<UniTask>();

            essentials.ForEach(x => allView.Add(LoadPopup(x)));

            return UniTask.WhenAll(allView);
        }

        public void ShowPopup<T>(object param = null, UIManager.UIPanel.OnShowed onShow = null, UIManager.UIPanel.OnHidden onHide = null, string PopupNameSpec = "") where T : UIPopup
        {
            // var _popup = _ShowPopup<T>(param, onShow, onHide);
            T _popup = null;
            UniTask.Action(async () => _popup = await _ShowPopup<T>(param, onShow, onHide)).Invoke();            
        }

        public async UniTask<T> _ShowPopup<T>(object param = null, UIManager.UIPanel.OnShowed onShow = null, UIManager.UIPanel.OnHidden onHide = null, string PopupNameSpec = "") where T : UIPopup
        {
            T popup = GetPopup<T>();

            if (popup == null)
            {
                // UniTask.Action(async () => popup = await LoadPopup<T>()).Invoke();
                popup = await LoadPopup<T>();
            }

            if (popup != null)
            {
                popup.onShow = onShow;
                popup.onHidden = onHide;

                popup.transform.SetAsLastSibling();
                popup.gameObject.SetActive(true);

                popup.Show(param);
            }

            return popup as T;
        }

        protected void AddPopup(string PopupName, UIPopup instance)
        {
            if (m_popups.ContainsKey(PopupName)) m_popups[PopupName] = instance;
            else m_popups.Add(PopupName, instance);
        }

        public async UniTask<T> LoadPopup<T>() where T : UIPopup
        {
            try
            {
                var popupAsset = Resources.LoadAsync<T>(string.Format($"Popups/Popup - {(typeof(T).ToString()).Replace("Popup","")}"));
                await UniTask.WaitUntil(() => popupAsset.isDone == true);

                UIPopup popup = null;
                popup = await __AddPopup<UIPopup>(popupAsset);
                // UniTask.Action(async () => popup = await __AddPopup<UIPopup>(popupAsset)).Invoke();
                return popup as T;
            }
            catch
            {
                Debug.LogError("find not found " + typeof(T).ToString());
                return null;
            }
        }

        protected async UniTask<T> __AddPopup<T>(ResourceRequest _ResourceRequested) where T : UIPopup
        {
            var _popupAsset = ((T)_ResourceRequested.asset);
            _popupAsset.gameObject.SetActive(false);

            var popup = UnityEngine.Object.Instantiate((T)_ResourceRequested.asset, PopupHolder ?? this.transform);
            popup.gameObject.SetActive(false);
            popup.transform.SetAsFirstSibling();
            await popup._Load();


            AddPopup(_ResourceRequested.asset.name, popup);

            return popup;
        }
        public async UniTask<UIPopup> LoadPopup(string PopupName)
        {
            try
            {
                var popupAsset = Resources.LoadAsync<UIPopup>($"Popups/Popup - {PopupName}");

                await popupAsset;
                // ((UIPopup)viewPrefab.asset).gameObject.SetActive(false);
                UIPopup popup = null;
                popup = await __AddPopup<UIPopup>(popupAsset);
                //    Instantiate((UIPopup)viewPrefab.asset, PopupHolder ?? this.transform);
                //popup.transform.SetAsFirstSibling();
                // await popup._Load();


                //AddPopup(PopupName, popup);
                return popup;
            }
            catch
            {
                Debug.LogError(PopupName + " Popup faile ");
                return null;
            }
        }



        public async UniTask HideAllPopup(string[] exceptView = null, bool instantAction = false)
        {
            var tasks = new List<UniTask>();
            foreach (var key in m_popups.Keys)
            {
                if (exceptView != null && exceptView.Contains(key)) continue;
                tasks.Add(HidePopup(key, instantAction));
            }

            await UniTask.WhenAll(tasks);
        }


        public async UniTask HidePopup(string name, bool instantAction = false)
        {
            var poup = GetPopup(name);
            if (poup != null)
            {
                poup.Hide();
                await UniTask.WaitUntil(() => poup.Visibility == VisibilityState.NotVisible);
                poup.gameObject.SetActive(false);                
            }
        }



        private void RemovePopup(string _poupName)
        {
            if (!m_popups.ContainsKey(_poupName)) return;
            Destroy(m_popups[_poupName].gameObject);
            m_popups.Remove(_poupName);
        }
    }
}