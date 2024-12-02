using System;
using System.Collections.Generic;
using System.Linq;
using Collab.Base.Collections;
using Collab.Base.Graphics;
using Collab.Holo;
using Collab.Holo.HoloComponents;
using Collab.Proxy.Prop.JournalInterop;
using UnityBase.Content;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class HoloTagObjectLookup
{
	public string Id;
	public UnityEngine.Object Object;

	public HoloTagObjectLookup(string id, Object obj)
	{
		Id = id;
		Object = obj;
	}
}

[Serializable]
public class ComponentLookup
{
	public List<HoloTagObjectLookup> Components = new List<HoloTagObjectLookup>();
	private Dictionary<string, List<Object>> keyToObjects = null;
	private Dictionary<Object, List<string>> objectToKeys = null;

	public List<Object> GetByKey(string id)
	{
		if(keyToObjects == null)
			BakeDictionaries();

		if (id == null)
			return new List<Object>();

		if (keyToObjects.ContainsKey(id))
			return keyToObjects[id];

		return new List<Object>();
	}

	//TODO: This doesn't work until after holo load, but sometimes we wanna do it before the load is done.  Fix!!!
	public Object GetFirstByKey(string id)
	{
		return GetByKey(id).FirstOrDefault();
	}

	public List<string> GetByObject(Object obj)
	{
		if(objectToKeys == null)
			BakeDictionaries();

		if (obj == null)
			return new List<string>();

		if (objectToKeys.ContainsKey(obj))
			return objectToKeys[obj];

		return new List<string>();
	}

	public string GetFirstByObject(Object obj)
	{
		return GetByObject(obj).FirstOrDefault();
	}

	public void RebakeDictionaries()
	{
		BakeDictionaries();
	}

	private void BakeDictionaries()
	{
		keyToObjects = new Dictionary<string, List<Object>>();
		objectToKeys = new Dictionary<Object, List<string>>();
		foreach (HoloTagObjectLookup component in Components)
		{
			if (component.Id != null && component.Object != null)
			{
				if (!keyToObjects.ContainsKey(component.Id) || keyToObjects[component.Id] == null)
					keyToObjects[component.Id] = new List<Object>();
				keyToObjects[component.Id].Add(component.Object);
				if (!objectToKeys.ContainsKey(component.Object) || objectToKeys[component.Object] == null)
					objectToKeys[component.Object] = new List<string>();
				objectToKeys[component.Object].Add(component.Id);
			}
		}
	}
}

public class HoloComponentAccessor : MonoBehaviour
{
	public ComponentLookup TaggedComponents = new ComponentLookup();
	public ComponentLookup IdComponents = new ComponentLookup();
	public ComponentLookup NamedComponents = new ComponentLookup();

	public List<Pair<string, IPropertyBoundManager>> propManagers = new List<Pair<string, IPropertyBoundManager>>();

	public HoloRoot TmpHoloRoot;

	public Object LookupByIdThenNameThenTag(string id)
	{
		return IdComponents.GetFirstByKey(id) ?? NamedComponents.GetFirstByKey(id) ?? TaggedComponents.GetFirstByKey(id);
	}

	public IEnumerable<Tuple<string,T>> ComponentIdsByType<T>()
	{
		foreach (var holoTagObjectLookup in IdComponents.Components)
		{
			if (holoTagObjectLookup.Object is T ast)
			{
				yield return Tuple.Create(holoTagObjectLookup.Id, ast);
			}
		}
	}
}

// Why? Because this way we can store the relevant texture metadata after a load, without keeping
// the raw byte data along with it. Ideally this would be unified into one type, contained with the 
// TextureAssetHoloStreamComponent.
public class TextureAssetMetadata
{
	public HoloComponentIdentifier ComponentId;
	public string Name;
	public TextureAssetHoloStreamComponent.ImageHoloPartFormatType ImageFormat;
	public string ImageFileName;
	public bool TextureAvailable;
	public Image2D.ImageTextureTypeEnum TextureCategory;
	public TextureAssetHoloStreamComponent.ImageFilteringEnum TextureFilter;
	public List<String> Tags;

	public static TextureAssetMetadata FromTextureAsset(TextureAssetHoloStreamComponent t)
	{
		return new TextureAssetMetadata()
		{
			ComponentId	= t.ComponentId,
			Name = t.Name,
			ImageFormat = t.ImageFormat,
			ImageFileName = t.ImageFileName,
			TextureAvailable = t.TextureAvailable,
			TextureCategory = t.TextureCategory,
			TextureFilter = t.TextureFilter,
			Tags = t.Tags
		};

	}
}

public class MaterialAssetMetadata
{
	public HoloComponentIdentifier ComponentId;
	public string Name;
	public GenericMaterialAssetHoloStreamComponent MaterialComponent = null;
	public List<string> Tags;

	public static MaterialAssetMetadata FromMaterialAsset(GenericMaterialAssetHoloStreamComponent m)
	{
		return new MaterialAssetMetadata()
		{
			MaterialComponent = m,
			ComponentId = m.ComponentId,
			Name = m.Name,
			Tags = m.Tags
		};

	}
}
