using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Collab.Holo;
using UnityBase.Content.DefaultHoloComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityBase.Content
{
	public class IncludeInExportComponent : HoloComponent
	{
		public Object[] includeInExport = Array.Empty<Object>();

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo conversions)
		{
			foreach (var includeThis in includeInExport)
			{
				conversions.SearchForOrConvertComponent(includeThis, this.gameObject, t, parent);
			}

			return HoloComponentIdentifier.Nothing;
		}
	}
}