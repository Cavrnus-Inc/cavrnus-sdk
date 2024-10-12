using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk.UI
{
    public class CavrnusSideMenuLoader : MonoBehaviour
    {
        public List<CavrnusSideMenuData> Menus;

        private bool foundMenuManager;
        private void Update()
        {
            if (foundMenuManager) 
                return;
            
            if (CavrnusMainMenuManager.Instance != null) {
                CavrnusMainMenuManager.Instance.SideMenuManager.SetupMenus(Menus);

                foundMenuManager = true;

                enabled = false;
            }
        }
    }
}