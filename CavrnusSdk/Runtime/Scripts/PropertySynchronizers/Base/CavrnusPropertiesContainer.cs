using System.Collections.Generic;
using UnityEngine;
using System;

namespace CavrnusSdk.PropertySynchronizers
{
	public class CavrnusPropertiesContainer : MonoBehaviour
	{
		[Header("The ID of the Property Container this value lives in.\nNote that two scripts referencing the same Container ID will get/set the same value.")]
		[SerializeField]
		public string UniqueContainerName;

        private void Reset() 
		{
			UniqueContainerName = GetGameObjectPath(gameObject); 
		}

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(UniqueContainerName))
                throw new System.Exception($"A Unique Container Name has not been assigned on object {GetGameObjectPath(gameObject)}");
        }

        private static string GetGameObjectPath(GameObject obj)
		{
			string res = "";
            res = res.Insert(0, "/"+obj.name);
			while (obj.transform.parent != null)
			{
				if(obj.transform.parent.GetComponent<CavrnusPropertiesContainer>() != null)
				{
                    res = res.Insert(0, "/" + obj.transform.parent.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName);
                    return res.Substring(1);
                }
				else
				{
                    obj = obj.transform.parent.gameObject;
                    res = res.Insert(0, "/" + obj.name);
                }				
			}

			//Chop off the leading "/"
			return res.Substring(1);
		}
	}
}