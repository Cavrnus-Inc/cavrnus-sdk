using System;
using System.Collections.Generic;
using System.Linq;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;

namespace CavrnusSdk.UI
{
    public class VideoUiDropdown : RtcUiDropdownBase
    {
        private List<CavrnusVideoInputDevice> foundVideoInputs = new List<CavrnusVideoInputDevice>();
        
        private const string PLAYERPREFS_VIDEOOUTPUT = "CavrnusVideoOutput";

        private IDisposable binding;

        protected override void OnSpaceConnected()
        {
            var savedAudioInput = PlayerPrefs.GetString(PLAYERPREFS_VIDEOOUTPUT, null);
            
            binding = SpaceConnection.FetchVideoInputs(opts => {
                //Store the fetched options to look up the selection
                foundVideoInputs = opts;
                foundVideoInputs.Remove(new CavrnusVideoInputDevice("Nothing", "black"));
                foundVideoInputs.Remove(new CavrnusVideoInputDevice("Application", "Application"));
          
                //Clear the existing UI
                Dropdown.ClearOptions();

                //Build the dropdown list
                var options = new List<TMP_Dropdown.OptionData>();
                foreach (var opt in opts) {
                    if (opt.Name.ToLowerInvariant().Contains("nothing") || opt.Name.ToLowerInvariant().Contains("application"))
                        continue;
                    
                    options.Add(new TMP_Dropdown.OptionData(opt.Name));
                }
                
                Dropdown.AddOptions(options);
                Dropdown.value = 0;

                //If we have a saved selection, pick it
                if (savedAudioInput != null) {
                    var selection = foundVideoInputs.FirstOrDefault(inp => inp.Id == savedAudioInput);

                    if (selection != null) {
                        Dropdown.SetValueWithoutNotify(foundVideoInputs.ToList().IndexOf(selection));
                        SpaceConnection?.UpdateVideoInput(selection);
                    }
                }
            });
        }

        protected override void DropdownValueChanged(int inputId)
        {
            base.DropdownValueChanged(inputId);
            
            //Have we finished fetching the options?
            if (foundVideoInputs == null) {
                SpaceConnection?.SetLocalUserStreamingState(false);

                return;
            }
            
            //Save our selection so we have it on future runs
            PlayerPrefs.SetString(PLAYERPREFS_VIDEOOUTPUT, foundVideoInputs[inputId].Id);
            PlayerPrefs.Save();

            SpaceConnection?.UpdateVideoInput(foundVideoInputs[inputId]);
            SpaceConnection?.SetLocalUserStreamingState(true);
        }

        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}