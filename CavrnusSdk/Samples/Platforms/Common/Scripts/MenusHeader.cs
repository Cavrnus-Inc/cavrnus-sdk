using UnityEngine;

namespace CavrnusSdk.Common
{
    public class MenusHeader : MonoBehaviour
    {
        public void CloseAllMenus() => MenuManager.Instance.CloseAllMenus();
    }
}