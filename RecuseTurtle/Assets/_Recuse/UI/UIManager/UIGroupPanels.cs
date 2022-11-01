using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIManager
{
    public class UIGroupPanels : MonoBehaviour
    {
        public enum TypeAnim
        {
            Wait,
            Parallel,
        }

        [SerializeField] public TypeAnim typeAnim;
        [SerializeField] protected UIPanel _main;
        [SerializeField] protected List<UIPanel> _uIPanels = new List<UIPanel>();
        public bool IsHideAnotherPanel = true;

        protected bool IsInGroup(UIPanel panel)
        {
            if (panel == null || _uIPanels.Count == 0)
                return false;

            return _uIPanels.Contains(panel);
        }

        public void Attach(UIPanel panel)
        {
            if (IsInGroup(panel))
                return;
            
            _uIPanels.Add(panel);
        }

        public void DeAttach(UIPanel panel)
        {
            if (IsInGroup(panel) == false)
                return;

            _uIPanels.Remove(panel);
        }

        public void Show(UIPanel panel, object param = null, bool Notify = true)
        {
            UIPanel _cPanel;
            float maxTime = 0;
            for(int i = 0; i < _uIPanels.Count; i++)
            {
                _cPanel = _uIPanels[i];
                if (!_cPanel.Equals(panel))
                {
                    if (Notify && IsHideAnotherPanel && _cPanel.Visibility.IsVisibility())
                    {
                        maxTime = Mathf.Max(_cPanel.fadeOutTime, maxTime);
                        _cPanel.Hide();
                    }
                }
            }

            if (typeAnim == TypeAnim.Wait)
            {
                DOVirtual.DelayedCall(maxTime, () =>
                {
                    if (panel.Visibility.IsVisibility() == false)
                        panel.Show(param);
                });
            }
            else
                 if (panel.Visibility.IsVisibility() == false)
                    panel.Show(param);
        }

        public void AddGroup(List<UIPanel> panels)
        {
            for(int i = 0; i < panels.Count; i++)
            {
                Attach(panels[i]);
            }
        }

        public void AddAndLoad(Type[] panels)
        {

        }

        public void SetMainPanel(UIPanel panel)
        {
            _main = panel;
        }


        public void ShowAll()
        {
            UIPanel _cPanel;
            for (int i = 0; i < _uIPanels.Count; i++)
            {
                _cPanel = _uIPanels[i];
                if (_cPanel.Visibility.IsVisibility() == false)
                    _cPanel.Show(null);
            }
        }

        public void HideAll()
        {
            UIPanel _cPanel;
            for (int i = 0; i < _uIPanels.Count; i++)
            {
                _cPanel = _uIPanels[i];
                if (_cPanel.Visibility.IsVisibility())
                    _cPanel.Hide();
            }
        }
    }
}