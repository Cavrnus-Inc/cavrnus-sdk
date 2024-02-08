using System;
using UnityEditor;
using UnityEngine;

namespace CavrnusSdk.Setup.Editor
{
    public static class CustomEditorUtilities
    {
        public static EditorWindow CreateCenteredWindow(this EditorWindow w, string title, Vector2 size)
        {
            w.titleContent = new GUIContent(title);
            w.position = new Rect(Screen.width / 2, Screen.height / 2, size.x, size.y);
            w.maxSize = new Vector2(size.x, size.y);
            w.minSize = w.maxSize;
            w.CenterOnMainWin();

            return w;
        }

        public static void AddSpace(float amount)
        {
            GUILayout.Space(amount);
        }
        
        public static void AddDivider()
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
        }
        
        public static void CreateLargeButton(string label, Vector2 size, Action onClick, int space = 10)
        {
            var style = ButtonStyle();
            style.fontSize = 16;
            style.fontStyle = FontStyle.Bold;
            style.fixedWidth = size.x;
            style.fixedHeight = size.y;
            
            if (GUILayout.Button(label, style))
                onClick?.Invoke();
            
            GUILayout.Space(space);
        }

        public static void CreateButton(string label, Action onClick, int space = 10)
        {
            if (GUILayout.Button(label, ButtonStyle()))
                onClick?.Invoke();
            
            GUILayout.Space(space);
        }

        public static void CreateHeader(string text, int space = 10)
        {
            GUILayout.Label(text, HeadingStyle());
            GUILayout.Space(space);
        }

        public static string CreateTextFieldWithLabel(string container, string label, int space = 10, int width = 120)
        {
            GUILayout.Label(label, BodyStyle());
            GUILayout.Space(space / 4);
            var gui= EditorGUILayout.TextField(container, GUILayout.Height(20), GUILayout.Width(width));
            GUILayout.Space(space);

            return gui;
        }
        
        public static string CreateTextAreaWithLabel(string container, string label, int height = 200, int space = 10)
        {
            GUILayout.Label(label, BodyStyle());
            GUILayout.Space(space / 4);
            var gui= EditorGUILayout.TextArea(container, GUILayout.Height(height));
            GUILayout.Space(space);
            
            return gui;
        }
        
        public static void CenterOnMainWin(this EditorWindow window)
        {
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            Rect pos = window.position;
            float centerWidth = (main.width - pos.width) * 0.5f;
            float centerHeight = (main.height - pos.height) * 0.5f;
            pos.x = main.x + centerWidth;
            pos.y = main.y + centerHeight;
            window.position = pos;
        }

        public static GUIStyle BodyStyle()
        {
            var style = new GUIStyle(EditorStyles.label) {
                wordWrap = true, 
                fontSize = 14, 
                richText = true
            };

            return style;
        }
        
        public static GUIStyle TitleStyle()
        {
            var style = new GUIStyle(EditorStyles.label) {
                wordWrap = true, 
                fontSize = 28, 
                richText = true,
                fontStyle = FontStyle.Bold
            };

            return style;
        }
        
        public static GUIStyle ButtonStyle()
        {
            var skin = new GUIStyle(GUI.skin.button) {
                fontSize = 14, 
                fontStyle = FontStyle.Bold
            };

            return skin;
        }
        
        public static GUIStyle HeadingStyle()
        {
            var style = new GUIStyle(EditorStyles.label) {
                wordWrap = true, 
                fontSize = 16, 
                richText = true,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };

            return style;
        }
    }
}