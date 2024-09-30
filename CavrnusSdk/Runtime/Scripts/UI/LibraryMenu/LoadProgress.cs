using TMPro;
using UnityEngine;

namespace CavrnusSdk.UI
{
    public class LoadProgress : MonoBehaviour
    {
        public TMP_Text progText;

        public void DisplayProgress(string step, float progress)
        {
            progText.SetText($"{(int)(progress*100)}%");
	    }
    }
}