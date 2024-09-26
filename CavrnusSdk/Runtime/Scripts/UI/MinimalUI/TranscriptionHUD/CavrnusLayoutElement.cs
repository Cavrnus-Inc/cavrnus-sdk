using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.UI
{
    public class CavrnusLayoutElement : LayoutElement
    {
        public float maxHeight;
        public float maxWidth;

        public bool useMaxWidth;
        public bool useMaxHeight;

        bool ignoreOnGettingPreferredSize;

        public override int layoutPriority { 
            get => ignoreOnGettingPreferredSize ? -1 : base.layoutPriority; 
            set => base.layoutPriority = value; }

        public override float preferredHeight {
            get {
                if (useMaxHeight) {
                    var defaultIgnoreValue = ignoreOnGettingPreferredSize;
                    ignoreOnGettingPreferredSize = true;

                    var baseValue = LayoutUtility.GetMinHeight(transform as RectTransform);

                    ignoreOnGettingPreferredSize = defaultIgnoreValue;

                    return baseValue > maxHeight ? maxHeight : baseValue;
                }
                else
                    return base.preferredHeight;
            }
            set => base.preferredHeight = value;
        }

        public override float preferredWidth { 
            get {
                if (useMaxWidth) {
                    var defaultIgnoreValue = ignoreOnGettingPreferredSize;
                    ignoreOnGettingPreferredSize = true;

                    var baseValue = LayoutUtility.GetPreferredWidth(transform as RectTransform);

                    ignoreOnGettingPreferredSize = defaultIgnoreValue;

                    return baseValue > maxWidth ? maxWidth : baseValue;
                }
                else
                    return base.preferredWidth;
            } 
            set => base.preferredWidth = value; 
        }
    }
}