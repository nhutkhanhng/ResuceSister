using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIManager
{
    public class UIViewLoader : UILoader<UIView>
    {
        protected override string Expression()
        {
            return "Views/{0}";
        }
    }
}