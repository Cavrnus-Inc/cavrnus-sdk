using System;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk
{
    [Serializable]
    public class CavrnusSpawnablePrefabsLookup : ScriptableObject
    {
        public List<CavrnusSpawnablePrefab> SpawnablePrefabs;
    }
}