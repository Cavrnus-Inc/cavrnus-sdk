using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleSetRandomColor : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
		//Clickity Clackity
		//Detect if the user clicked me.  Just uses Unity stuff
		if (Input.GetMouseButtonDown(0))
		{
			UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 100))
			{
				if (hit.collider != null && hit.collider.transform == transform)
				{
					SwitchColor();
				}
			}
		}
	}

	private void SwitchColor()
	{
		//Create some new value.  Can come from UI or whatever you want
		var newColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

		GetComponent<MeshRenderer>().material.color = newColor;
	}
}
