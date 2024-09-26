using System;
using TMPro;
using UnityEngine.EventSystems;

namespace CavrnusSdk.UI
{
    public class CavrnusInputField : TMP_InputField
    {
        public static Action<TMP_InputField> Selected;
        public static Action<TMP_InputField> DeSelected;

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            Selected?.Invoke(this);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            if (eventData == null) return;
            DeSelected?.Invoke(this);

            base.OnDeselect(eventData);
        }
    }
}