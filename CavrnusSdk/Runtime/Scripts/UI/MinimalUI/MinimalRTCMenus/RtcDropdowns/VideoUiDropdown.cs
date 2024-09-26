using System.Collections.Generic;
using CavrnusSdk.API;
using TMPro;

namespace CavrnusSdk.UI
{
    public class VideoUiDropdown : RtcUiDropdownBase
    {
        private List<CavrnusVideoInputDevice> foundVideoInputs = new List<CavrnusVideoInputDevice>();

        protected override void OnSpaceConnected()
        {
            CavrnusFunctionLibrary.FetchVideoInputs(opts => {
                //Store the fetched options to look up the selection
                foundVideoInputs = opts;

                //Clear the existing UI
                Dropdown.ClearOptions();

                //Build the dropdown list
                var options = new List<TMP_Dropdown.OptionData>();
                foreach (var opt in opts) {
                    if (opt.Name.ToLowerInvariant().Contains("nothing")) {
                        options.Add(new TMP_Dropdown.OptionData("None"));
                    }
                    else {
                        options.Add(new TMP_Dropdown.OptionData(opt.Name));
                    }
                }

                //Assign dropdown list to UI
                Dropdown.AddOptions(options);
                Dropdown.value = 0;
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

            CavrnusFunctionLibrary.UpdateVideoInput(foundVideoInputs[inputId]);
            SpaceConnection?.SetLocalUserStreamingState(inputId != 0);
        }
    }
}