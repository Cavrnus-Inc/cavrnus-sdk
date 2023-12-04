using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CavrnusSdk
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public class SampleJson : MonoBehaviour
	{
		[Header("The text component you want to update")]
		public TMP_Text TextComponent;

		public string PropertyName = "JsonData";

		[Serializable]
		public class JsonSampleStructure
		{
			[SerializeField] public List<int> Data = new List<int>();
		}

		private void Start() { CavrnusSpaceJoinEvent.OnAnySpaceConnection(OnSpaceConnection); }

		private CavrnusSpaceConnection spaceConn;

		private void OnSpaceConnection(CavrnusSpaceConnection spaceConn)
		{
			this.spaceConn = spaceConn;
			CavrnusPropertyHelpers.BindToStringProperty(
				spaceConn, GetComponent<CavrnusPropertiesContainer>().UniqueContainerPath, PropertyName, DisplayJsonData);
		}

		public void DisplayJsonData(string ob)
		{
			JsonSampleStructure data = JsonUtility.FromJson<JsonSampleStructure>(ob);
			if (data == null) data = new JsonSampleStructure();

			string res = "JsonHash: ";
			foreach (var item in data.Data) { res += item.ToString() + ", "; }

			if (res.EndsWith(", ")) res.Substring(0, res.Length - 2);

			TextComponent.text = res;
		}

		void Update()
		{
			if (spaceConn == null) return;

			//Clickity Clackity
			//Detect if the user clicked me.  Just uses Unity stuff
			if (Input.GetMouseButtonDown(0)) {
				UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 100)) {
					if (hit.collider != null && hit.collider.transform == transform) { PostUpdate(); }
				}
			}
		}

		private void PostUpdate()
		{
			JsonSampleStructure dataToSend = new JsonSampleStructure();

			int numItems = UnityEngine.Random.Range(1, 6);

			for (int i = 0; i < numItems; i++) { dataToSend.Data.Add(UnityEngine.Random.Range(0, 99)); }

			CavrnusPropertyHelpers.UpdateStringProperty(
				spaceConn, GetComponent<CavrnusPropertiesContainer>().UniqueContainerPath, PropertyName,
				JsonUtility.ToJson(dataToSend));
		}
	}
}