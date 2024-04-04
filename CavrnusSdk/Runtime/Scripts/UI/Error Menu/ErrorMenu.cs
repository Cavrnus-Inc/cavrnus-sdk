using TMPro;
using UnityEngine;

namespace CavrnusSdk.UI
{
    public class ErrorMenu : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tmProTitle;
        [SerializeField] private TextMeshProUGUI tmProMessage;
        
        public void Setup(string title, string message)
        {
            tmProTitle.text = title;
            tmProMessage.text = message;
        }
    }
}
