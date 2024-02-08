using TMPro;
using UnityEngine;

namespace CavrnusSdk.UI
{
    public class WidgetLocalTimeDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeText;
        
        private void Update()
        {
            timeText.text = $"{System.DateTime.Now.ToLocalTime():h:mm tt}";
        }
    }
}