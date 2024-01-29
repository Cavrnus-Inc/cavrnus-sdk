using TMPro;
using UnityEngine;

namespace CavrnusSdk
{
	public class SampleSetExclamationPoints : MonoBehaviour
	{
		[Header("The text component you want to update")]
		public TMP_Text TextComponent;

		void Update()
		{
			//Clickity Clackity
			//Detect if the user clicked me.  Just uses Unity stuff
			if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
			{
				UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 100))
				{
					if (hit.collider != null && hit.collider.transform == transform)
					{
						PostStringUpdateToJournal();
					}
				}
			}
		}

		private void PostStringUpdateToJournal()
		{
			string prefixString = "Left/Right Click me to Add/Remove Exclamation Points";

			string currStr = TextComponent.text;

			int bangCount = currStr.Length - prefixString.Length;

			if (Input.GetMouseButton(0))
				bangCount = bangCount + 1;
			else
				bangCount = Mathf.Max(0, bangCount - 1);

			var finalString = prefixString;
			for (int i = 0; i < bangCount; i++)
				finalString += "!";

			TextComponent.text = finalString;
		}
	}
}