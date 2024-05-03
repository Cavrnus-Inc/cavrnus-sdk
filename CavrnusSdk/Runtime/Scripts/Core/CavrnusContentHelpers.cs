using System;
using CavrnusSdk.API;
using Collab.Proxy.Comm.RestApi.ObjectMetaTypes;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Collab.Base.ProcessSys;
using Collab.Base.ProcessTask;
using Collab.Base.Scheduler;
using UnityEditor;

namespace CavrnusCore
{
	internal static class CavrnusContentHelpers
	{
		internal static async void FetchFileById(string id, Action<string, float> progress, Func<Stream, long, Task> onStreamLoaded)
		{
			var ndo = await CavrnusStatics.Notify.ObjectsSystem.StartListeningSpecificAsync(id);

			//TODO: What is this???
			if (!ndo.Assets.TryGetValue(ObjectAssetIdentifier.SimpleCanonical, out var asset))
				return;

			var pf = ProcessFeedbackFactory.DelegatePerProg(ps =>
			{
				CavrnusStatics.Scheduler.ExecInMainThread(() => progress(ps.currentMessage, ps.overallProgress));
			}, 0);

			await CavrnusStatics.ContentManager.GetAndDecryptObject<bool>(ndo.ToUoiDeprecateMe(), asset, async (uoi2, stream, len, pf2) =>
			{
				await onStreamLoaded(stream, len);
				return true;
			}, pf);
		}

		internal static async void FetchAllUploadedContent(Action<List<CavrnusRemoteContent>> onCurrentContentArrived)
		{
			await CavrnusStatics.Notify.ObjectsSystem.StartListeningAllAsync();

			List<CavrnusRemoteContent> content = new List<CavrnusRemoteContent>();

			foreach (var ndo in CavrnusStatics.Notify.ObjectsSystem.ObjectsInfo.Values)
				content.Add(new CavrnusRemoteContent(ndo));

			CavrnusStatics.Scheduler.ExecInMainThread(() => onCurrentContentArrived(content));
		}

		internal static async void UploadContent(string localFilePath, Dictionary<string, string> tags, Action<CavrnusRemoteContent> onUploadComplete)
		{
			var uploadProcess = new ProcessTask<string>("Upload new Content");

			uploadProcess.Setup(async (pf) =>
			{
				var id = await CavrnusStatics.ContentManager.UploadPotentiallyNewObject(localFilePath, CavrnusStatics.Notify.UsersSystem.ConnectedUser.Value.Id, ObjectUsageEnumRest.Object, pf, tags);
				return id;
			}, new ProgressStep("Upload File", $"Uploading {localFilePath}."));

			var uploadTask = uploadProcess.GenerateHandle();
			await uploadProcess.Execute();

			var contentId = await uploadTask.Task;

			var ndo = await CavrnusStatics.Notify.ObjectsSystem.StartListeningSpecificAsync(contentId);

			CavrnusStatics.Scheduler.ExecInMainThread(() => onUploadComplete(new CavrnusRemoteContent(ndo)));
		}
	}
}