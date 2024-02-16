using System;
using CavrnusSdk.API;
using Collab.Proxy.Comm.RestApi.ObjectMetaTypes;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Collab.Base.ProcessSys;

namespace CavrnusCore
{
	internal static class CavrnusContentHelpers
	{
		internal static async void FetchFileById(CavrnusSpaceConnection spaceConn, string id, Action<string, float> progress, Func<Stream, long, Task> onStreamLoaded)
		{
			var ndo = await CavrnusStatics.Notify.ObjectsSystem.StartListeningSpecificAsync(id);

			//TODO: What is this???
			if (!ndo.Assets.TryGetValue(ObjectAssetIdentifier.SimpleCanonical, out var asset))
				return;

			var pf = ProcessFeedbackFactory.DelegatePerProg(ps =>
			{
				CavrnusStatics.Scheduler.ExecInMainThread(() => progress(ps.currentMessage, ps.overallProgress));
			}, 0);

			await spaceConn.RoomSystem.ObjectContext.ServerContentManager.GetAndDecryptObject<bool>(ndo.ToUoiDeprecateMe(), asset, async (uoi2, stream, len, pf2) =>
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

			onCurrentContentArrived(content);
		}
	}
}