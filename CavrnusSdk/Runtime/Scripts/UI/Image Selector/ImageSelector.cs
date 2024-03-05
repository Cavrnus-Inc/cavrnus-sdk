using System;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers.CommonImplementations;
using Collab.Base.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageSelector : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private SyncImageSprite imageSyncer;
    [SerializeField] private Button button;

    private void Start()
    {
        button.interactable = false;
        
        CavrnusFunctionLibrary.AwaitAnySpaceConnection(csc => {
            button.onClick.AddListener(SelectRandomImage);
            button.interactable = true;
        });
    }

    public void SelectRandomImage()
    {
        image.sprite = imageSyncer.SpriteLookup.TakeRandom().Value;
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(SelectRandomImage);
    }
}