using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CavrnusSdk.Setup.Editor
{
    public class WelcomeModal : EditorWindow
    {
        private static readonly Vector2 WindowSize = new Vector2(600, 500);
        private readonly Vector2 mainButtonSize = new Vector2(270, 40);
        private readonly Vector2 smallerButtonSize = new Vector2(140, 27);
        
        private const float Space = 10;
        private const int Padding = 10;

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
            window.CreateCenteredWindow("Cavrnus Spatial Connector", WindowSize);
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

            if (shouldAutoOpen && !Application.isPlaying) 
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
            
            CustomEditorUtilities.CreateHeader("Start adding collaboration to your project.");
            
            CustomEditorUtilities.AddSpace(Space);

            GUILayout.EndVertical();

            CustomEditorUtilities.AddSpace(Space);
            
            GUILayout.EndHorizontal();
            
            GUILayout.BeginVertical();

            CreateLargeButton("Set up your Space", mainButtonSize, CavrnusSetupHelpers.SetupSceneForCavrnus);
            CreateLargeButton("Web Console",  mainButtonSize,()=> Application.OpenURL("https://console.dev.cavrn.us/signin?startUrl=/"));
            
            CustomEditorUtilities.AddSpace(5);

            CustomEditorUtilities.CreateLabel("Need help with getting started?", 12, false, TextAnchor.MiddleCenter);
            CustomEditorUtilities.AddSpace(5);

            CreateLargeButton("Visit Documentation",  smallerButtonSize,()=> Application.OpenURL("https://www.cavrnus.com/"));

            GUILayout.EndVertical();

            // Footer area

            GUILayout.BeginArea(new Rect(0, position.height - 50, position.width, 50));
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            CustomEditorUtilities.AddDivider();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Space(18); // Left padding
            CustomEditorUtilities.CreateButton("Dismiss", new Vector2(70, 27), Close);
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            
            GUILayout.FlexibleSpace(); // this pushes and right aligns next block

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace(); // Right Align
            GUILayout.BeginHorizontal();
            shouldAutoOpen = GUILayout.Toggle(shouldAutoOpen, "Show when Unity starts");
            GUILayout.Space(18); // Right padding
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace(); // Bottom padding
            GUILayout.EndVertical();

            // End horizontal layout group
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            var textColor = Color.black;
            CustomEditorUtilities.CreateLabelAbsolutePos("Cavrnus Spatial Connector", new Rect(15, 10, 200, 20), textColor, 11, true);
            CustomEditorUtilities.CreateLabelAbsolutePos($"Version {GetPackageVersion()}", new Rect(WindowSize.x - 115, 10, 100, 20), textColor, 11, true, TextAnchor.MiddleRight);
        }

        private void ShowHeader()
        {
            GUILayout.BeginVertical();
                // Draw the box

                var cacheColor = GUI.color;
                
                var boxStyle = new GUIStyle(GUI.skin.box) {
                    alignment = TextAnchor.MiddleCenter, 
                    fontStyle = FontStyle.Bold,
                    fontSize = 30,
                };

                var assetsImg = CustomEditorUtilities.LoadTextureFromFile(CustomEditorUtilities.GetPathWithRoot("CavrnusSdk/Runtime/Scripts/Setup/Editor/Welcome Modal/cav-logo.png"));
                GUILayout.Box(assetsImg, boxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(200));
                
                GUI.color = cacheColor;

            GUILayout.EndVertical();
        }
        
        private void CreateLargeButton(string text, Vector2 size, Action onClick)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            
            CustomEditorUtilities.CreateLargeButton(text, size,0, onClick);
            
            GUILayout.Space(10);
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

        private static string GetPackageVersion()
        {
            var result = "";
            var devPath = "Assets/package.json";
            var path = CustomEditorUtilities.GetPathWithRoot("package.json");
            if (File.Exists(devPath)) {
                var data = File.ReadAllText(devPath);
                var jData = JObject.Parse(data);
            
                if (jData["version"] != null)
                    result = jData["version"].ToString();
                else
                    Debug.LogError($"Cannot find version Json entry in {devPath} file!"); 
            }
            else {
                Debug.LogError($"Cannot find package path at {devPath}: ");
            }

            return result;
        }
    }
}