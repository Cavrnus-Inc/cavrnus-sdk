using UnityEngine;
using UnityEngine.SceneManagement;

namespace CavrnusSdk.UI
{
	public class SwitchSceneAndSpace : MonoBehaviour
	{
		public string SceneToLoad;

		void Start()
		{
			gameObject.SetActive(false);
			CavrnusSpaceJoinEvent.OnAnySpaceConnection(OnSpaceConnection);
		}

		CavrnusSpaceConnection spaceConn;

		private void OnSpaceConnection(CavrnusSpaceConnection spaceConn)
		{
			this.spaceConn = spaceConn;

			//IMPORTANT: We can't exit the current space until we have finished entering it.
			gameObject.SetActive(true);
		}

		public void SwitchSceneAndCavrnusSpace()
		{
			if (string.IsNullOrEmpty(SceneToLoad)) {
				Debug.LogError("Specify a scene in the prefab that you want to switch to!");
				return;
			}

			//Make sure to disconnect from your existing Cavrnus Space
			CavrnusSpaceJoinEvent.ExitCurrentSpace();

			//Then simply load up the new scene.
			//It can connect to its own Cavrnus Space by itself
			SceneManager.LoadScene(SceneToLoad);
		}
	}
}