using System;
using UnityEngine;

namespace CavrnusSdk.UI
{
    [Serializable]
    public class CavrnusSideMenuData
    {
        public string Title;
        public GameObject Menu;
        public Sprite MenuIcon;
            
        public override bool Equals(object obj)
        {
            if (obj is CavrnusSideMenuData otherMenuData)
                return Title == otherMenuData.Title;
                
            return false;
        }

        public override int GetHashCode()
        {
            return Title != null ? Title.GetHashCode() : 0;
        }
    }
}