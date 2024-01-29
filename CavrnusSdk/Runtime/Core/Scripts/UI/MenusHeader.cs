using UnityEngine;

namespace CavrnusSdk.UI
{
    public class MenusHeader : MonoBehaviour
    {
        public void CloseAllMenus() => MenuManager.Instance.CloseAllMenus();
    }
}