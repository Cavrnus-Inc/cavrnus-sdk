using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class HoloMetaSettings : MonoBehaviour
	{
		public bool ImportOffsetDisabled = false;
		public bool RotationDisabled = false;
		public float ImportHeightOffsetValue = 0;
		public bool ExportDisabled = false;
		public bool ScaleDisabled = false;
		public bool HideDisabled = false;
		public bool SnappingDisabled = false;
		public bool AsAvatarTransparencyDisabled = false;
		public float AsAvatarTransparencyDistanceMultiplier = 1f;
		public bool IgnoreAllCutting = false;
		public bool IsRiggedAvatar = false;
		public bool IsReadyPlayerMe = false;

		public void AssignToMetadata(MetadataHoloStreamComponent m)
		{
			if (ImportOffsetDisabled)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.ImportOffsetDisabled, "true"));
			if (RotationDisabled)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.RotationDisabled, "true"));
			m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.ImportHeightOffsetValue, ImportHeightOffsetValue.ToString()));
			if (ExportDisabled)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.ExportDisabled, "true"));
			if (ScaleDisabled)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.ScaleDisabled, "true"));
			if (HideDisabled)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.HideDisabled, "true"));
			if (SnappingDisabled)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.SnappingDisabled, "true"));
			if (AsAvatarTransparencyDisabled)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.AsAvatarTransparencyDiabled, "true"));
			if (AsAvatarTransparencyDistanceMultiplier != 1.0f)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.AsAvatarTransparencyDistanceMultiplier, AsAvatarTransparencyDistanceMultiplier.ToString()));
			if (IgnoreAllCutting)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.IgnoreAllCutting, "true"));
			if (IsRiggedAvatar)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.IsRiggedAvatar, "true"));
			if (IsReadyPlayerMe)
				m.Metadata.Add(new MetadataHoloStreamComponent.MetadataEntry(HoloMeta.IsReadyPlayerMe, "true"));
		}
	}
}