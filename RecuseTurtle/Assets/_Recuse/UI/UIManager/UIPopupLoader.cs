using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIManager
{
    public class UIPopupLoader : UILoader<UIPopup>
    {
        protected override string Expression()
        {
            return "Popups/{0}";
        }
    }
}