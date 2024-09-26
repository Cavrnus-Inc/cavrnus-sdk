﻿using System;
using System.Collections;
using System.Collections.Generic;
using CavrnusCore;
using CavrnusSdk.API;
using CavrnusSdk.UI;
using Collab.Base.Collections;
using Collab.LiveRoomSystem.LiveObjectManagement.ObjectTypeManagers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Cavrnus.Chat
{
    public class ChatMenuEntry : MonoBehaviour
    {
        [Header("Chat Metadata References")]
        [SerializeField] private TextMeshProUGUI creatorName;
        [SerializeField] private TextMeshProUGUI creationTime;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private Image profilePicImage;
        
        [Header("Chat Visuals")]
        [SerializeField] private Image chatBubbleBackground;
        [SerializeField] private Color localUserColor;
        
        [Header("Chat Hover")]
        [SerializeField] private List<CanvasGroup> rootCanvasGroup;
        [SerializeField] private List<CanvasGroup> extraButtonsCanvasGroup;
        
        [Header("Layout Components")]
        [SerializeField] private HorizontalOrVerticalLayoutGroup rootLayoutElement;
        [SerializeField] private HorizontalOrVerticalLayoutGroup innerLayoutElement;
        [SerializeField] private HorizontalOrVerticalLayoutGroup chatContentLayoutElement;
        [SerializeField] private HorizontalOrVerticalLayoutGroup extrasContentLayoutElement;
        [SerializeField] private HorizontalOrVerticalLayoutGroup metadataContainer;
        
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        
        private IChatViewModel chat;
        private Action<IChatViewModel> onDelete;
        
        public void Setup(CavrnusSpaceConnection spaceConn, IChatViewModel chat, Action<IChatViewModel> onDelete)
        {
            this.chat = chat;
            this.onDelete = onDelete;

            disposables.Add(chat.CurrentLanguageTranslatedText.Bind(msg => message.text = msg.text ?? msg.liveSource));
            disposables.Add(chat.CreateTime.Bind(msg => creationTime.text = UnityBase.HelperFunctions.ToPrettyDay(msg.ToLocalTime())));
			disposables.Add(chat.CreatorName.Bind(msg => creatorName.text = msg));
			disposables.Add(chat.CreatorProfilePicUrl.Bind(profilePicUrl =>
			{
                CavrnusStatics.Scheduler.ExecCoRoutine(CavrnusShortcutsLibrary.LoadProfilePic(profilePicUrl, pic =>
                {
					if (profilePicImage == null)
					{
						return;
					}

					profilePicImage.sprite = pic;
					if (pic != null)
						profilePicImage.GetComponent<AspectRatioFitter>().aspectRatio =
						(float)pic.texture.width / (float)pic.texture.height;
				}));
			}));

			extraButtonsCanvasGroup.ForEach(cg => cg.alpha = 0f);
		}
        
        public void RemoveChatButtonClick()
        {
            onDelete?.Invoke(chat);
        }

        private void OnDestroy()
        {
            disposables.ForEach(d => d.Dispose());
        }
    }
}