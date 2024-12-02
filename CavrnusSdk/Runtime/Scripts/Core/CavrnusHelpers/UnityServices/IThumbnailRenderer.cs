using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityBase.UnityServices
{
	public interface IThumbnailRenderer
	{
		Texture RenderMaterialThumbnail(Material mat);
		Texture RenderObjectThumbnail(GameObject ob);
	}
}
