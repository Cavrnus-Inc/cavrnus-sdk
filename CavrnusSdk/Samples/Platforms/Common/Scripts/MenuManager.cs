using System;
using System.Collections.Generic;
using System.Linq;
using Collab.Base.Collections;
using UnityEngine;

namespace CavrnusSdk.Common
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance;

        [SerializeField] private MenusScriptableObject menus;
        
        private class MenuInformation
        {
            public GameObject MenuPrefab = null;
            public GameObject MenuInstance = null;
            public ISetting<bool> VisSetting = new Setting<bool>();
        }

        private readonly Dictionary<string, MenuInformation> knownMenus = new Dictionary<string, MenuInformation>();
        private readonly Dictionary<string, MenuInformation> openMenus = new Dictionary<string, MenuInformation>();

        private void Awake()
        {
            Instance = this;
        }

        public IReadonlySetting<bool> GetMenuSetting(string menu) => LazyMenuInfoLookup(menu).VisSetting;

        private MenuInformation LazyMenuInfoLookup(string menu)
        {
            MenuInformation menuInfo;
            
            if (!knownMenus.ContainsKey(menu)) {
                var mi =  new MenuInformation {
                    MenuPrefab = menus.GetMenu(menu),
                    VisSetting = new Setting<bool>(false),
                    MenuInstance = null
                };
                
                knownMenus.Add(menu,mi);

                menuInfo = mi;
            }
            else
                menuInfo = knownMenus.FirstOrDefault(m => m.Key == menu).Value;

            return menuInfo;
        }
        
        public void OpenMenu(string menu, Transform menuContainer, Action<GameObject> onCreated = null)
        {
            CloseAllMenus();

            var menuInfo = LazyMenuInfoLookup(menu);
            var foundMenu = menuInfo.MenuPrefab;
            var menuInstance = Instantiate(foundMenu, menuContainer);
            menuInstance.gameObject.SetActive(true);

            menuInfo.MenuInstance = menuInstance;
            menuInfo.VisSetting.Value = true;
            
            openMenus.Add(menu, menuInfo);
            onCreated?.Invoke(menuInstance);
        }

        public void CloseAllMenus()
        {
            foreach (var menu in openMenus) {
                menu.Value.VisSetting.Value = false;
                Destroy(menu.Value.MenuInstance);
            }
            
            openMenus.Clear();
        }

        public void CloseMenu(string menu)
        {
            var foundMenu = openMenus.FirstOrDefault(m => m.Key == menu);
            foundMenu.Value.VisSetting.Value = false;
            Destroy(foundMenu.Value.MenuInstance);

            openMenus.Remove(menu);
        }
        
        public void ToggleMenu(string menu, Transform menuContainer, Action<GameObject> onCreated = null)
        {
            if (openMenus.ContainsKey(menu))
                CloseMenu(menu);
            else
                OpenMenu(menu, menuContainer, onCreated);
        }
    }
}