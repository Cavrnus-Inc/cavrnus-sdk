using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers;
using UnityBase.Content;
using UnityBase.Content.DefaultHoloComponents;
using System.IO;
using CavrnusSdk.PropertySynchronizers.CommonImplementations;
using System.Threading.Tasks;
using Collab.Base.ProcessSys;

namespace CavrnusCore
{
	public class CavrnusHoloLoader : MonoBehaviour
	{
		public HoloLoadProgress ProgressPrefab;

		HoloLoadProgress progDisplay;

		private GameObject loadedHolo = null;

		// Start is called before the first frame update
		void Start()
		{
			CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn =>
			{
				spaceConn.BindStringPropertyValue(GetComponent<CavrnusPropertiesContainer>().UniqueContainerName, "ContentId", contentId => FetchHoloStream(spaceConn, contentId));
			});
		}

		private void FetchHoloStream(CavrnusSpaceConnection spaceConn, string file)
		{
			if(destroyed) //Were we removed during the previous load step?
				return;

			//We probably get this before we get the actual ContentId
			if (file == null)
				return;

			if (progDisplay != null)
			{
				Debug.LogError("Cannot change the content of a HoloLoader while it is in progress.  For now...");
				return;
			}

			progDisplay = GameObject.Instantiate(ProgressPrefab);
			progDisplay.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName = GetComponent<CavrnusPropertiesContainer>().UniqueContainerName;

			Debug.Log("fetching " + file);
			CavrnusFunctionLibrary.FetchFileById(spaceConn, file, (step, prog) => progDisplay.DisplayProgress(step, prog), async (stream, len) =>
			{
				Debug.Log("fetched " + file);

				if (destroyed) //Were we removed during the previous load step?
					return;

				await LoadHoloFile(stream, len);
			});
		}

		private async Task LoadHoloFile(Stream stream, long len)
		{
			//For Local Files
			//var fs = File.Open(file, FileMode.Open);

			gameObject.SetActive(false);

			var pf = ProcessFeedbackFactory.DelegatePerProg(ps =>
			{
				CavrnusStatics.Scheduler.ExecInMainThread(() => progDisplay.DisplayProgress(ps.currentMessage, ps.overallProgress));
			}, 0);

			HoloToUnity htu = new HoloToUnity(gameObject, new DefaultHoloComponentFactory());
			var ob = await htu.LoadHoloStreamAsync(stream, len, CavrnusStatics.Scheduler, pf);

			if(destroyed) //Were we removed during the previous load step?
				return;

			GameObject.Destroy(progDisplay.gameObject);

			SetupLoadedHolo(ob);
		}

		public void SetupLoadedHolo(GameObject ob)
		{
			ob.transform.SetParent(null);

			var cpc = ob.AddComponent<CavrnusPropertiesContainer>();
			cpc.UniqueContainerName = GetComponent<CavrnusPropertiesContainer>().UniqueContainerName;

			var syncT = ob.AddComponent<SyncLocalTransform>();
			syncT.PropertyName = "Transform";
			var syncV = ob.AddComponent<SyncVisibility>();
			syncV.PropertyName = "Visibility";

			foreach (var ps in ob.GetComponentsInChildren<ParticleSystem>())
			{
				ps.Play();
			}
			foreach (var anim in ob.GetComponentsInChildren<Animation>())
			{
				anim.Play();
			}

			loadedHolo = ob;
		}

		//For Debugging
		/*private void Update()
		{
			if(loadedHolo != null)
			{
				var ps = loadedHolo.GetComponentInChildren<ParticleSystemRenderer>();
				if(ps != null)
				{
					string str = $"RenderQueue: {ps.material.renderQueue}";
					foreach (var keyword in ps.material.enabledKeywords)
						str += $"\nKeyword: {keyword}";
					foreach (var propName in ps.material.GetPropertyNames(MaterialPropertyType.Float))
						str += $"\nFloat: {propName} - {ps.material.GetFloat(propName)}";
					foreach (var propName in ps.material.GetPropertyNames(MaterialPropertyType.Int))
						str += $"\nInt: {propName} - {ps.material.GetInt(propName)}";
					foreach (var propName in ps.material.GetPropertyNames(MaterialPropertyType.Vector))
						str += $"\nVector: {propName} - {ps.material.GetVector(propName)}";
				}
			}
		}*/

		private bool destroyed = false;
		private void OnDestroy()
		{
			destroyed = true;
			if (loadedHolo != null)
				GameObject.Destroy(loadedHolo);
			if(progDisplay != null)
				GameObject.Destroy(progDisplay.gameObject);
		}
	}
}