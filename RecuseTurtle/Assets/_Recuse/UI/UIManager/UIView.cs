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

    public abstract class UIView : UIPanel
    {
        public virtual void CloseDialog() { }

        public virtual void OnEscapPress()
        {
            CloseDialog();
        }
    }
}