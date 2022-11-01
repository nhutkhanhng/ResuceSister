using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UIManager;
using Cysharp.Threading.Tasks;

public class ViewHome : UIView
{
    [SerializeField] protected UIPanel left, right;
    [SerializeField] UIGroupPanels _groupPanels;

    public override UniTask Initialize()
    {
        _groupPanels.SetMainPanel(this);
        _groupPanels.AddGroup(new List<UIPanel>() { left, right });

        return base.Initialize();
    }

    protected override void Shown()
    {
        _groupPanels.ShowAll();
        _groupPanels.IsHideAnotherPanel = true;
    }


    public void OnClickShowLeft()
    {
        _groupPanels.Show(left);
    }

    public void OnClickRight()
    {
        _groupPanels.Show(right);
    }

    public override void Hidden()
    {
        base.Hidden();
    }
}
