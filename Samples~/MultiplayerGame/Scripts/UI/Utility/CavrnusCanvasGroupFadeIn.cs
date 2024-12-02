using System.Collections.Generic;
using CavrnusSdk.UI;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusCanvasGroupFadeIn : MonoBehaviour
    {
        [SerializeField] private float duration = 0.2f;
        
        private CanvasGroup cg;
        private void Awake()
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            gameObject.DoFade(new List<CanvasGroup> {cg}, duration, true);
        }
    }
}