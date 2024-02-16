using System.Linq;
using UnityEngine;
using CavrnusSdk.API;
using CavrnusCore;

public class SampleHoloObjectCreation : MonoBehaviour
{
	public string FileNameToSearchFor;

	CavrnusSpaceConnection spaceConn;
	CavrnusRemoteContent contentToUse;

	void Start() 
	{
		CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => 
		{
			CavrnusFunctionLibrary.FetchAllUploadedContent(content =>
			{
				contentToUse = content.FirstOrDefault(c => 
				{
					if (c.FileType != Collab.Proxy.Comm.LiveTypes.ObjectCategoryEnum.Holo)
						return false;
					return c.Name.ToLowerInvariant().Equals(FileNameToSearchFor.ToLowerInvariant()) || c.FileName.ToLowerInvariant().Contains(FileNameToSearchFor.ToLowerInvariant());
				}
				);

				if (contentToUse == null)
				{
					contentToUse = content.FirstOrDefault(c => 
					{
						if (c.FileType != Collab.Proxy.Comm.LiveTypes.ObjectCategoryEnum.Holo)
							return false;
						return c.Name.ToLowerInvariant().Contains(FileNameToSearchFor.ToLowerInvariant());
					});
				}

				if (contentToUse == null)
				{
					Debug.LogError($"No content found containing string {FileNameToSearchFor}");
					return;
				}

				Debug.Log($"Found holo to spawn.  Will spawn {contentToUse.Name} based on search string {FileNameToSearchFor}");

				spaceConn = sc;
			});
		}); 
	}

	// Update is called once per frame
	void Update()
	{
		if (spaceConn == null) return;

		//Clickity Clackity
		//Detect if the user clicked me.  Just uses Unity stuff
		if (Input.GetMouseButtonDown(0))
		{
			UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 100))
			{
				if (hit.collider != null && hit.collider.transform == transform) { CreateObject(); }
			}
		}
	}

	private void CreateObject()
	{
		var randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f));
		var pos = transform.position + randomOffset;

		PostSpawnObjectWithUniqueId(spaceConn, "HoloLoader", new CavrnusTransformData(pos, transform.localEulerAngles, Vector3.one));
	}

	// Temporarily moved out of CavrnusHelpers to avoid missing dependency when using package samples.
	private string PostSpawnObjectWithUniqueId(CavrnusSpaceConnection spaceConn, string uniqueId, CavrnusTransformData pos = null)
	{
		string newContainerName = spaceConn.SpawnObject(uniqueId);

		spaceConn.PostStringPropertyUpdate(newContainerName, "ContentId", contentToUse.Id);

		if (pos != null)
			spaceConn.PostTransformPropertyUpdate(newContainerName, "Transform", pos);

		return newContainerName;
	}
}