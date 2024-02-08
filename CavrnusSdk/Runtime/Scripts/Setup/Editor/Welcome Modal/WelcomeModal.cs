using System;
using UnityEditor;
using UnityEngine;

namespace CavrnusSdk.Setup.Editor
{
    public class WelcomeModal : EditorWindow
    {
        private static readonly Vector2 WindowSize = new Vector2(700, 520);
        private readonly Vector2 mainButtonSize = new Vector2(500, 50);
        
        private const float Space = 10;
        private const int Padding = 20;

        private static bool shouldAutoOpen = true;
        private static WelcomeModal window;

        [MenuItem("Cavrnus/Welcome")]
        public static void Init()
        {
            ShowWindow();
        }

        private static void ShowWindow()
        {
            window = GetWindow<WelcomeModal>();
            window.CreateCenteredWindow("Welcome To Cavrnus!", WindowSize);
            window.CenterOnMainWin();

            window.ShowPopup();
        }

        [InitializeOnLoadMethod]
        private static void OnProjectLoadedInEditor()
        {
            EditorApplication.update += HandleUpdate;
        }

        private static void HandleUpdate()
        {
            shouldAutoOpen = EditorPrefs.GetBool("ShouldAutoOpen", true);

            if (shouldAutoOpen) 
                ShowWindow();
            
            EditorApplication.update -= HandleUpdate;
        }

        private void OnGUI()
        {
            ShowHeader();

            CustomEditorUtilities.AddSpace(Space);

            // Draw content inside the padded area
            GUILayout.BeginHorizontal();
            GUILayout.Space(Padding); // Left padding

            GUILayout.BeginVertical();
            
            CustomEditorUtilities.CreateHeader("Instantly add multi-user collaboration to your project with just a few clicks!");
            
            CustomEditorUtilities.AddDivider();
            CustomEditorUtilities.AddSpace(Space);

            GUILayout.EndVertical();

            CustomEditorUtilities.AddSpace(Space);
            
            GUILayout.EndHorizontal();
            
            GUILayout.BeginVertical();

            CreateLargeButton("Setup Space For Cavrnus", CavrnusSetupHelpers.SetupSceneForCavrnus);
            CreateLargeButton("Go To Web Console", ()=> Application.OpenURL("https://console.dev.cavrn.us/signin?startUrl=/"));
            CreateLargeButton("Documentation", ()=> Application.OpenURL("https://www.cavrnus.com/"));
            CreateLargeButton("Close", Close);
            
            GUILayout.EndVertical();

            // Footer area
            GUILayout.BeginArea(new Rect(0, position.height - 50, position.width, 50));
            CustomEditorUtilities.AddDivider();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace(); // Right Align

            GUILayout.BeginHorizontal();
            GUILayout.Space(Padding); // Left padding
            shouldAutoOpen = GUILayout.Toggle(shouldAutoOpen, "Show when Unity starts");
            GUILayout.Space(Padding); // Right padding
            GUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace(); // Bottom padding
            GUILayout.EndVertical();

            // End horizontal layout group
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void ShowHeader()
        {
            GUILayout.BeginVertical();
                // Draw the box

                var cacheColor = GUI.color;
                GUI.color = new Color(0.7f, 0.9f, 0.7f);
                
                var boxStyle = new GUIStyle(GUI.skin.box) {
                    alignment = TextAnchor.MiddleCenter, 
                    fontStyle = FontStyle.Bold,
                    fontSize = 30
                };

                GUILayout.Box("Welcome to Cavrnus!", boxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(150));
                
                GUI.color = cacheColor;

            GUILayout.EndVertical();
        }
        
        private void CreateLargeButton(string text, Action onClick)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            
            CustomEditorUtilities.CreateLargeButton(text, mainButtonSize,onClick);
            
            GUILayout.Space(0);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            EditorPrefs.SetBool("ShouldAutoOpen", shouldAutoOpen);
        }
        
        private void OnLostFocus()
        {
            EditorPrefs.SetBool("ShouldAutoOpen", shouldAutoOpen);
        }
    }
}