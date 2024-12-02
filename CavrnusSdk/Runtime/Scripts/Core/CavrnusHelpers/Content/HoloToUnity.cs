using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts;
using Collab.Base.Collections;
using Collab.Holo;
using Collab.Base.Core;
using Collab.Base.Math;
using Collab.Base.ProcessTask;
using Collab.Base.Scheduler;
using UnityEngine;

namespace UnityBase.Content
{
	public class HoloToUnity
	{
		private readonly GameObject rootParent;
		private readonly IHoloComponentFactory factory;

		public HoloToUnity(GameObject parent, IHoloComponentFactory factory)
		{
			this.rootParent = parent;
			this.factory = factory;
		}

		#region Async Loading via Scheduler

		public async Task<GameObject> LoadHoloStreamAsync(Stream s, long streamLen, IUnityScheduler sched, IProcessFeedback pf)
		{
			HoloRoot loadedRoot = await sched.ExecInWorkerThreadTask(async () =>
			{
				using (pf?.Progress?.Push(
					new ProgressStep("Holo Parsing", "Reading Holo Stream", new Span(0f, .4f))))
				{
					// TODO: It would be fabulous if we could process components as they arrive here, streaming async style, but it's a deep change for another time.
					var hr = HoloIO.LoadHolo(s, streamLen);
					//pf?.Messages?.Info($"Loaded holo root '{hr.RootMetadata?.ObjectName}' with {hr.NodesById.Count} nodes. ({hr.RootMetadata.ReadHoloVersion}, {hr.RootMetadata.OverallBounds})");
					return hr;
				}
			});

			pf?.CancelToken.ThrowIfCancellationRequested();

			using (pf?.Progress?.Push(new ProgressStep("Load Holo", "Loading Holo into Unity and VRAM",
				new Span(.4f, 1f))))
			{
				var converter = new HoloToUnityConvert(factory, sched);
				try
				{
					await sched.ExecInMainThreadAsAsyncTask(async () =>
					{
						await LoadHoloRootContinuation(loadedRoot, converter, pf);
						return false;
					});

					pf?.CancelToken.ThrowIfCancellationRequested();

					return converter.CreatedRoot;
				}
				catch (OperationCanceledException oce)
				{
					converter.CancelAndDestroy();
					throw;
				}
				catch (Exception e)
				{
					pf?.Messages?.Error($"Error while converting holo to unity: {e.Message}", e);
					converter.CancelAndDestroy();
					throw;
				}
			}
		}

		public async Task LoadHoloRootContinuation(HoloRoot root, HoloToUnityConvert converter, IProcessFeedback pf)
		{
			try
			{
				using (pf?.Progress?.Push(
					new ProgressStep("Setup", "Setting up root level objects", new Span(0f, .05f))))
				{
					converter.InitializeLoading(root, rootParent);
				}

				using (pf?.Progress?.Push(new ProgressStep("Load", "Loading Holo Components", new Span(.05f, .8f))))
				{
					int componentsComplete = 0;

					Set<HoloComponentIdentifier> visited = new Set<HoloComponentIdentifier>();
					var nodesList = root.EnumerateAllChildrenDepthAndDependencyFirst(visited).ToList();
					foreach (var node in nodesList)
					{
						pf?.Messages?.Info($"@{Time.time}... HoloComponent: {node.Component.GetType().Name}, {node.Component.ComponentId} [{Rand.RandomI(0, 9999)}][{componentsComplete}]");
						await converter.LoadComponent(node.Component, pf);

						if (this.rootParent != null && HelperFunctions.NullOrDestroyed(this.rootParent))
							return;

						pf?.Progress?.UpdateCurrentStep(componentsComplete++, nodesList.Count);

						if (componentsComplete % 40 == 0)
						{
							pf?.CancelToken.ThrowIfCancellationRequested();
							await Task.Delay(1);

							if (this.rootParent != null && HelperFunctions.NullOrDestroyed(this.rootParent))
								return;
						}
					}
				}

				using (pf?.Progress?.Push(new ProgressStep("Finalize", "Building Colliders", new Span(.8f, 1f))))
				{
					var cb = converter.MakeAllColliders();
					foreach (var y in cb)
					{
						if (y is Helpers.TimingInfo ti)
						{
							if (ti.difficulty == Helpers.TimingInfo.Difficulty.Hard)
								await Task.Delay(1);

							if (this.rootParent != null && HelperFunctions.NullOrDestroyed(this.rootParent))
								return;
						}
					}
				}
			}
			catch (Exception e)
			{
				DebugOutput.Error($"Error while loading holo: {e}");
				throw e;
			}
		}

		#endregion
	}
}