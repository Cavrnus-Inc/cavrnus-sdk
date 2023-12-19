using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Collab.Base.Graphics;
using Collab.RtcCommon;
using FM.LiveSwitch;
using FM.LiveSwitch.Unity;
using UnityEngine;

public class CompositeVideoSourceUnity : VideoSource
{
	private VideoSource currentInternal;

	public Dictionary<string, Tuple<SourceInput, Func<VideoSource>>> inputs = null;

	public Texture CurrentTexture
	{
		get
		{
#if BUILD_SUPPORT_WEBCAMTEXTURE
			if (currentInternal is WebCamDirectTextureSource w)
				return w.View;
#endif
			return Texture2D.blackTexture;
		}
	}

	public CompositeVideoSourceUnity(VideoFormat outputFormat) : base(outputFormat)
	{
		SynchronizationSource = Utility.GenerateSynchronizationSource();
		OutputSynchronizable = false;
	}

	public override Future<SourceInput[]> GetInputs()
	{
		Promise<SourceInput[]> p = new Promise<SourceInput[]>();

		inputs = new Dictionary<string, Tuple<SourceInput, Func<VideoSource>>>();

		inputs.Add(RtcInputSource.BlankVideoSourceId, Tuple.Create(
			new SourceInput(RtcInputSource.BlankVideoSourceId, "Nothing"),
			(Func<VideoSource>) (() => new NullVideoSource(OutputFormat))
		));

		inputs.Add("Application", Tuple.Create(
			new SourceInput("Application", "Application"),
			(Func<VideoSource>) (() => new ImagesVideoSource())));

#if BUILD_SUPPORT_WEBCAMTEXTURE
		foreach (var camInput in WebCamDirectTextureSource.GetCameraInputs())
		{
			inputs.Add(camInput.Id, 
				Tuple.Create(
					new SourceInput(camInput.Id, $"Camera: '{camInput.Name}'"),
					(Func<VideoSource>) (() =>
					{
						var cs = new WebCamDirectTextureSource(new VideoConfig(640, 480, RtcRegistrySettings.GetCameraFramerate() ?? 30)); // Use res closest to this, and with lowest optional framerate. We're intentionally cutting down on bandwidth usage.
						cs.Input = camInput;
						return cs;
					})
				));

		}
#endif

		/*	for (var si = 0; si < System.Windows.Forms.Screen.AllScreens.Length; si++)
			{
				var screen = System.Windows.Forms.Screen.AllScreens[si];
				inputs.Add(screen.DeviceName, Tuple.Create(
					new SourceInput(screen.DeviceName, $"Screen: {si} {(screen.Primary ? "(Primary)" : "")}"),
					(Func<VideoSource>) (() =>
					{
						var s = new ScreenSource(RtcRegistrySettings.GetScreenFramerate() ?? 3);
						s.Input = new SourceInput(screen.DeviceName, screen.DeviceName);
						return s;
					})));

			}

			var wins = WindowsUser32Helpers.GetVisibleWindows();
			foreach (var win in wins)
			{
				inputs.Add(win.HWnd.ToInt64().ToString(), Tuple.Create(
					new SourceInput(win.HWnd.ToInt64().ToString(), $"Window: '{win.Title}'"),
					(Func<VideoSource>) (() =>
					{
						var s = new WindowVideoSource(win, RtcRegistrySettings.GetWindowFramerate() ?? 5);
						return s;
					})));
			}
		*/
		p.Resolve(inputs.Select(i => i.Value.Item1).ToArray());

		return p;
	}

	protected override Future<object> DoStart()
	{
		// Determine which type of sub-source to initialize
		if (inputs == null)
			GetInputs().WaitForPromise();

		string inputId = Input?.Id ?? RtcInputSource.BlankVideoSourceId;

		if (!inputs.TryGetValue(inputId, out var foundInput) || foundInput == null)
		{
			Promise<object> p = new Promise<object>();
			p.Resolve(null);
			return p;
		}

		currentInternal = foundInput.Item2();
		currentInternal.OnRaiseFrame += (f) =>
		{
			var f2 = new VideoFrame(f.Buffer);
			f2.SystemTimestamp = ManagedStopwatch.GetTimestamp();
			RaiseFrame(f);
		};
		return currentInternal.Start();
	}

	protected override Future<object> DoStop()
	{
		if (currentInternal == null)
		{
			Promise<object> p = new Promise<object>();
			p.Resolve(null);
			return p;
		}

		var tmp = currentInternal;
		currentInternal.OnRaiseFrame -= RaiseFrame;
		currentInternal = null;
		return tmp.Stop();
	}

	public override string Label => "Composite Video Options";

	private Texture2D textureSource = null;
	public void UpdateTextureSource(Texture2D texSource)
	{
		textureSource = texSource;
		if (currentInternal is Texture2DSource ts)
		{
			ts.Texture2D = texSource;
		}
	}

	public void ReceiveImagesVideoSourceImage(IImage2D image)
	{
		(currentInternal as ImagesVideoSource)?.ImageProvided(image);
	}
}