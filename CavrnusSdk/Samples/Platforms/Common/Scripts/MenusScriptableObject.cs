using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CavrnusSdk.Common
{
    [CreateAssetMenu(fileName = "CavrnusMenus", menuName = "Cavrnus/Menus", order = 1)]
    public class MenusScriptableObject : ScriptableObject
    {
        [Serializable]
        public class MenuEntry
        {
            public string Name;
            public GameObject Menu;
        }
        public List<MenuEntry> Menus;

        public GameObject GetMenu(string menu)
        {
            return Menus.FirstOrDefault(prefab => prefab.Name.Equals(menu))?.Menu;
        }
    }
}