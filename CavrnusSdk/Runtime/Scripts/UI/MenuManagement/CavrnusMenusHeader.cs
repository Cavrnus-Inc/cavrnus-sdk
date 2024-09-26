using UnityEngine;

namespace CavrnusSdk.UI
{
    public class CavrnusMenusHeader : MonoBehaviour
    {
        public void CloseAllMenus() => CavrnusMenuManager.Instance.CloseAllMenus();
    }
}