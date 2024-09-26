using System.Collections.Generic;
using UnityEngine;

namespace Cavrnus.UI
{
    public class SideMenuManager : MonoBehaviour
    {
        [SerializeField] private SideMenuContainer sideMenuContainer;
        
        [Space]
        [SerializeField] private Transform buttonsContainer;
        [SerializeField] private SideMenuButton buttonPrefab;

        private int currentOpenMenuId = -1;
        
        // Created UI Elements
        private readonly List<MinimalUIManager.SideMenuData> instantiatedMenus = new List<MinimalUIManager.SideMenuData>();
        private readonly List<SideMenuButton> instantiatedButtons = new List<SideMenuButton>();

        public void Setup(List<MinimalUIManager.SideMenuData> menuData)
        {
            menuData.ForEach(data => {
                instantiatedMenus.Add(new MinimalUIManager.SideMenuData {
                    Title = data.Title, 
                    MenuIcon = data.MenuIcon, 
                    Menu = Instantiate(data.Menu)
                });
            });

            for (var i = 0; i < instantiatedMenus.Count; i++) {
                var button = Instantiate(buttonPrefab, buttonsContainer, false);
                button.Setup(i, instantiatedMenus[i]);
                button.ButtonSelected += ButtonOnButtonSelected;
                instantiatedButtons.Add(button);
                
                sideMenuContainer.AddMenuToContainer(instantiatedMenus[i]);
            }
            
            sideMenuContainer.ManuallyClosed += SideMenuContainerOnManuallyClosed;
        }

        private void SideMenuContainerOnManuallyClosed()
        {
            instantiatedButtons[currentOpenMenuId].SetState(false);
            currentOpenMenuId = -1;
        }

        private void ButtonOnButtonSelected(int menuId)
        {
            ResetButtons();

            // close current menu and deactivate button
            if (menuId == currentOpenMenuId) {
                sideMenuContainer.SetMenuContainerVisibility(false);
                sideMenuContainer.SetTargetMenuVisibility(menuId, instantiatedMenus[menuId],false);
                
                currentOpenMenuId = -1;
            }
            else {
                instantiatedButtons[menuId].SetState(true);
                sideMenuContainer.SetMenuContainerVisibility(true);
                sideMenuContainer.SetTargetMenuVisibility(menuId, instantiatedMenus[menuId],true);

                currentOpenMenuId = menuId;
            }
        }

        private void ResetButtons()
        {
            instantiatedButtons.ForEach(b => b.SetState(false));
        }

        private void OnDestroy()
        {
            foreach (var button in instantiatedButtons) {
                button.ButtonSelected -= ButtonOnButtonSelected;
            }
            
            sideMenuContainer.ManuallyClosed -= SideMenuContainerOnManuallyClosed;
        }
    }
}