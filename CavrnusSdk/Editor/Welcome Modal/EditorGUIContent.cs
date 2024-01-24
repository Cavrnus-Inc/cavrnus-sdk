using System;
using UnityEngine;

namespace CavrnusSdk.Editor
{
    public class EditorGUIContent : ScriptableObject
    {
        public Texture2D Banner;
        public string Title;
        public string Intro;
        public Section[] Sections;

        public bool LoadedLayout;

        [Serializable]
        public class Section
        {
            public string Heading, Button, Text, LinkText, URL;
        }
    }
}