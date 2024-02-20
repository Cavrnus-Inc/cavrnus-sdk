using TMPro;
using UnityEngine;

public class HoloLoadProgress : MonoBehaviour
{
    public TMP_Text progText;

    public void DisplayProgress(string step, float progress)
    {
        progText.SetText($"{(int)(progress*100)}%");
	}
}
