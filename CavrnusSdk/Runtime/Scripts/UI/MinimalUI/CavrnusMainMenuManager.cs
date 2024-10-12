﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public class CavrnusMainMenuManager : MonoBehaviour
    {
        public static CavrnusMainMenuManager Instance{ get; private set; }
        public SideMenuManager SideMenuManager => sideMenuManager;

        [Header("SideMenuContainer")]
        [SerializeField] private SideMenuManager sideMenuManager;
        [SerializeField] private List<CavrnusSideMenuData> sideMenus = new ();

        [Header("FocusMode")]
        [SerializeField] private FocusModeManager focusModeManager;

        [Header("TranscriptionHUD")]
        [SerializeField] private CavrnusTranscriptionHUD transcriptionHUD;
        [SerializeField] private Toggle transcriptionVisToggle;
        
        [Header("MaximizedUserManager")]
        [SerializeField] public MaximizedUserManager MaximizedUserManager;

        private RtcUiDropdownBase[] dropdowns;

        private void Start()
        {
            Instance = this;
            sideMenuManager.SetupMenus(sideMenus);

            dropdowns = GetComponentsInChildren<RtcUiDropdownBase>(true);
            if (dropdowns.Length > 0) {
                foreach (var d in dropdowns) {
                    d.Setup();
                }
            }
            
            MaximizedUserManager.OnVisChanged += MaximizedUserManagerOnOnVisChanged;
            
            transcriptionHUD.OnTranscriptionPropertyEnabled += TranscriptionEnabled;
            transcriptionVisToggle.onValueChanged.AddListener(TranscriptionToggleValChanged);
        }

        private void TranscriptionToggleValChanged(bool val)
        {
            transcriptionHUD.SetVis(val);
        }

        private void TranscriptionEnabled(bool val)         
        {
            transcriptionVisToggle.gameObject.SetActive(val);
        }

        private void MaximizedUserManagerOnOnVisChanged(bool vis)
        {
            focusModeManager.SetState(vis);
        }

        private void OnDestroy()
        {
            MaximizedUserManager.OnVisChanged -= MaximizedUserManagerOnOnVisChanged;
            
            transcriptionHUD.OnTranscriptionPropertyEnabled -= TranscriptionEnabled;
            transcriptionVisToggle.onValueChanged.RemoveListener(TranscriptionToggleValChanged);
        }
    }
}