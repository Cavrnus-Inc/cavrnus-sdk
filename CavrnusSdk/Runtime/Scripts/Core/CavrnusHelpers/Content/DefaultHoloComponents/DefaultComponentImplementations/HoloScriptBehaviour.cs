using System;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class HoloScriptBehaviour : HoloComponent, IInteractibleComponent
	{
		[Multiline]
		public string holoScript = "";

		public string scriptVersion = "0.2";
		
		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<ScriptHoloComponent>(parent);
			Version ver;
			if (!Version.TryParse(scriptVersion, out ver))
				ver = null;
			g.Data = new ScriptData() { Script = holoScript, ScriptVersion = ver };

			g.Tags = Tags;
			return g.ComponentId;
		}
	}
}
