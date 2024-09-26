using System.IO;
using System.Threading.Tasks;
using CavrnusCore;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers;
using Collab.Base.ProcessSys;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public class ImageLoader : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private LoadProgress progressPrefab;

        LoadProgress progDisplay;
        void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn =>
            {
                spaceConn.BindStringPropertyValue(GetComponent<CavrnusPropertiesContainer>().UniqueContainerName, "ContentId", FetchImage);
            });
        }

        private void FetchImage(string file)
        {
            if(destroyed) //Were we removed during the previous load step?
                return;

            //We probably get this before we get the actual ContentId
            if (file == null)
                return;

            progDisplay = Instantiate(progressPrefab);
            progDisplay.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName = GetComponent<CavrnusPropertiesContainer>().UniqueContainerName;

            Debug.Log("fetching " + file);
            CavrnusFunctionLibrary.FetchFileById(file, (step, prog) => progDisplay.DisplayProgress(step, prog), async (stream, len) =>
            {
                Debug.Log("fetched " + file);

                if (destroyed) //Were we removed during the previous load step?
                    return;

                await LoadImage(stream, len);
            });
        }

        private async Task LoadImage(Stream stream, long len)
        {
            ProcessFeedbackFactory.DelegatePerProg(ps =>
            {
                CavrnusStatics.Scheduler.ExecInMainThread(() => progDisplay.DisplayProgress(ps.currentMessage, ps.overallProgress));
            }, 0);
            
            var texture = new Texture2D(1, 1);
            var imageData = new byte[len];
            await stream.ReadAsync(imageData, 0, (int)len);
        
            texture.LoadImage(imageData);
            rawImage.texture = texture;
            
            Destroy(progDisplay.gameObject);
        }

        private bool destroyed = false;
        private void OnDestroy()
        {
            destroyed = true;
        }
    }
}