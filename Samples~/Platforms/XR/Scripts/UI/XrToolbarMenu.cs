using System;
using System.Collections.Generic;
using CavrnusSdk.Common;
using CavrnusSdk.XR.Widgets;
using Collab.Base.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.XR.UI
{
    public class XrToolbarMenu : MonoBehaviour
    {
        [SerializeField] private Transform menusContainer;
        [SerializeField] private Transform toolBarContainer;
        
        [Space] 
        [SerializeField] private Image spacePickerMenuOpen;
        [SerializeField] private Image settingsMenuOpen;
        [SerializeField] private Image usersMenuOpen;
        [SerializeField] private Image colorPickerMenuOpen;

        [Space]
        [SerializeField] private WidgetUserProfileImage widgetUserProfileImageWidget;
        [SerializeField] private WidgetUserMic widgetUserMicWidget;
        
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        
        public void Setup()
        {
            // Toolbar is hidden by default
            CavrnusSpaceJoinEvent.OnAnySpaceConnection(OnSpaceConnection);

            // Set initial vis if in space or not
            var isInSpace = CavrnusSpaceJoinEvent.CurrentCavrnusSpace != null;
            toolBarContainer.gameObject.SetActive(isInSpace);
            
            disposables.Add(MenuManager.Instance.GetMenuSetting("UsersMenu").Bind(vis => usersMenuOpen.gameObject.SetActive(vis)));
            disposables.Add(MenuManager.Instance.GetMenuSetting("SettingsMenu").Bind(vis => settingsMenuOpen.gameObject.SetActive(vis)));
            disposables.Add(MenuManager.Instance.GetMenuSetting("SpacePickerMenu").Bind(vis => spacePickerMenuOpen.gameObject.SetActive(vis)));
            disposables.Add(MenuManager.Instance.GetMenuSetting("ColorPickerMenu").Bind(vis => colorPickerMenuOpen.gameObject.SetActive(vis)));
        }

        public void OpenSpacePicker()
        {
            //TODO: FIX!
            /*
            MenuManager.Instance.ToggleMenu("SpacePickerMenu", menusContainer, async go => {
                var foundSpaces = await CavrnusHelpers.GetAllAvailableSpaces();
                go.GetComponent<SpacePicker>().Setup(foundSpaces);
            });
            */
        }
        
        public void OpenColorPicker() => MenuManager.Instance.ToggleMenu("ColorPickerMenu", menusContainer);
        public void OpenSettings() => MenuManager.Instance.ToggleMenu("SettingsMenu", menusContainer);
        
        public void OpenUsersMenu() => MenuManager.Instance.ToggleMenu("UsersMenu", menusContainer);
        
        public void ExitSpace() => CavrnusSpaceJoinEvent.ExitCurrentSpace();
        
        private void OnSpaceConnection(CavrnusSpaceConnection csc)
        {
            toolBarContainer.gameObject.SetActive(true);

            // Setup widget components
            var cu = new CavrnusUser(csc.RoomSystem.Comm.LocalCommUser.Value, csc.RoomSystem);
			widgetUserMicWidget.Setup(cu);
			widgetUserProfileImageWidget.Setup(cu);
		}
        
        private void OnDestroy()
        {
            foreach (var d in disposables)
                d.Dispose();
        }
    }
}