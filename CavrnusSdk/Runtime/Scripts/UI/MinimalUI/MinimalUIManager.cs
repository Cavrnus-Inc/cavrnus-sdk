using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.UI
{
    public class MinimalUIManager : MonoBehaviour
    {
        [Serializable]
        public class SideMenuData
        {
            public string Title;
            public GameObject Menu;
            public Sprite MenuIcon;
            
            public override bool Equals(object obj)
            {
                if (obj is SideMenuData otherMenuData)
                    return Title == otherMenuData.Title;
                
                return false;
            }

            public override int GetHashCode()
            {
                return Title != null ? Title.GetHashCode() : 0;
            }
        }

        public static MinimalUIManager Instance{ get; private set; }
        
        [Header("SideMenuContainer")]
        [SerializeField] private SideMenuManager sideMenuManager;
        [SerializeField] private List<SideMenuData> sideMenus = new ();

        [Header("FocusMode")]
        [SerializeField] private FocusModeManager focusModeManager;

        [Header("TranscriptionHUD")]
        [SerializeField] private CavrnusTranscriptionHUD transcriptionHUD;
        [SerializeField] private Toggle transcriptionVisToggle;
        
        [Header("MaximizedUserManager")]
        [SerializeField] public MaximizedUserManager maximizedUserManager;
        
        // Add UI show/hide bindings

        private void Awake()
        {
            Instance = this;
            sideMenuManager.Setup(sideMenus);
            
            maximizedUserManager.OnVisChanged += MaximizedUserManagerOnOnVisChanged;
            
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
            maximizedUserManager.OnVisChanged -= MaximizedUserManagerOnOnVisChanged;
            
            transcriptionHUD.OnTranscriptionPropertyEnabled -= TranscriptionEnabled;
            transcriptionVisToggle.onValueChanged.RemoveListener(TranscriptionToggleValChanged);
        }
    }
}