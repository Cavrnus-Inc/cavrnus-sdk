using TMPro;
using UnityEngine;

namespace CavrnusSdk.UI
{
	public class PropertiesDebuggerEntry : MonoBehaviour
	{
		[SerializeField] private TMP_Text PropId;
		[SerializeField] private TMP_Text PropValueStr;

		public void Setup(string propAbsId, string propCurrValueStr)
		{
			PropId.text = propAbsId;
			PropValueStr.text = propCurrValueStr;
		}
	}
}