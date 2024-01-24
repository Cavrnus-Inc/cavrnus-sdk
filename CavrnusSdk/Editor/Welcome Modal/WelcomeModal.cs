using System;
using UnityEditor;
using UnityEngine;

namespace CavrnusSdk.Editor
{
    public class WelcomeModal : EditorWindow
    {
        private static GUIStyle linkStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle introStyle;
        private static GUIStyle headingStyle;
        private static GUIStyle bodyStyle;
        private static GUIStyle buttonStyle;

        private static bool initialized;

        private static readonly Vector2 WindowSize = new Vector2(700, 620);
        
        private const float Space = 10;
        private static readonly int Padding = 20;
        
        private EditorGUIContent styleContent;
        private bool toggleValue;

        [MenuItem("Cavrnus/Welcome")]
        public static void Init()
        {
            var w = GetWindow<WelcomeModal>();
            w.titleContent = new GUIContent("Welcome!");
            w.position = new Rect(Screen.width / 2, Screen.height / 2, WindowSize.x, WindowSize.y);
            w.maxSize = new Vector2(WindowSize.x, WindowSize.y);
            w.minSize = w.maxSize;
            w.CenterOnMainWin();
            
            InitStyle();
            w.ShowPopup();
        }
        
        private void OnGUI()
        {
            if (styleContent == null) return;
            
            ShowHeader();
            
            // Draw some previous GUI layout items
            // Add padding after the previous items
            GUILayout.Space(15);

            // Draw content inside the padded area
            GUILayout.BeginHorizontal();
            GUILayout.Space(Padding); // Left padding

            GUILayout.BeginVertical();
            ShowBody();
            AddDivider();
            GUILayout.Space(Space);

            GUILayout.EndVertical();

            GUILayout.Space(Padding); // Right padding
            
            GUILayout.EndHorizontal();
            
            GUILayout.BeginVertical();
            CreateButton("Setup Space For Cavrnus", new Vector2(500, 50), () => {
                Debug.Log("Setup space");
                CavrnusSetupHelpers.SetupSpaceForCavrnus();
            });
            
            GUILayout.Space(8);
            CreateButton("Go To Web Console", new Vector2(500, 50),() => {
                Application.OpenURL("https://console.dev.cavrn.us/signin?startUrl=/");
            });
            
            GUILayout.Space(8);
            CreateButton("Documentation", new Vector2(500, 50),() => {
                Application.OpenURL("https://www.cavrnus.com/");
            });
            
            GUILayout.Space(8);
            CreateButton("Close", new Vector2(150, 50), () => {
                Close();
            });
            
            GUILayout.EndVertical();

            // Separator line

            // Footer area
            GUILayout.BeginArea(new Rect(0, position.height - 50, position.width, 50));
            AddDivider();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace(); // Right Align

            GUILayout.BeginHorizontal();
            GUILayout.Space(Padding); // Left padding
            toggleValue = GUILayout.Toggle(toggleValue, "Don't show on startup");
            GUILayout.Space(Padding); // Right padding
            GUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace(); // Bottom padding
            GUILayout.EndVertical();

            // End horizontal layout group
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

        }

        private static void AddDivider() { GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3)); }

        private static void CreateButton(string text, Vector2 size, Action onClick)
        {
            var bs = new GUIStyle(GUI.skin.button) {
                fontSize = 16, 
                fontStyle = FontStyle.Bold
            };

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(text, bs, GUILayout.Width(size.x), GUILayout.Height(size.y)))
                onClick?.Invoke();

            GUILayout.FlexibleSpace(); // Add flexible space on the right
            GUILayout.EndHorizontal();
        }
        
        private void ShowBody()
        {
            if (styleContent.Title != "") {
                GUILayout.BeginVertical();
                GUILayout.Label(styleContent.Title, titleStyle);
                GUILayout.Space(Space);
                GUILayout.EndVertical();
            }
            
            if (styleContent.Intro != "") {
                GUILayout.BeginVertical();
                GUILayout.Label(styleContent.Intro, introStyle);
                GUILayout.Space(Space);
                GUILayout.EndVertical();
            }
            
            foreach (var section in styleContent.Sections)
            {
                GUILayout.BeginVertical();

                if (!string.IsNullOrEmpty(section.Heading))
                    GUILayout.Label(section.Heading, headingStyle);
                GUILayout.EndVertical();
                
                GUILayout.Space(Space);
                GUILayout.BeginVertical();

                if (!string.IsNullOrEmpty(section.Text))
                    GUILayout.Label(section.Text, bodyStyle);
                GUILayout.EndVertical();

                GUILayout.Space(Space);
                GUILayout.BeginVertical();
                if (!string.IsNullOrEmpty(section.LinkText))
                {
                    if (LinkLabel(new GUIContent(section.LinkText)))
                        Application.OpenURL(section.URL);
                }
                GUILayout.EndVertical();

                GUILayout.Space(Space);
            }
        }

        private void CreateGUI()
        {
            InitStyle();
            
            // get corresponding SO asset
            styleContent = SelectContent();
            
            // var label = new Label("Select an object and click the Randomize! button");
            // rootVisualElement.Add(label);
            //
            // var randomizeButton = new Button();
            // randomizeButton.text = "Randomize!";
            // randomizeButton.clicked += () => {
            //     Debug.Log("Clicked!");
            // };
            //
            // rootVisualElement.Add(randomizeButton);
        }

        private void ShowHeader()
        {
            GUILayout.BeginVertical();
            {
                if (styleContent.Banner != null)
                {
                    GUILayout.Box(styleContent.Banner, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                }
            }
            GUILayout.EndVertical();
        }

        private static EditorGUIContent SelectContent()
        {
            var ids = AssetDatabase.FindAssets("WelcomeContent t:EditorGUIContent");
            if (ids.Length == 1)
            {
                var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

                Selection.objects = new UnityEngine.Object[] { readmeObject };

                return (EditorGUIContent)readmeObject;
            }
            else
            {
                Debug.Log("Couldn't find a readme");
                return null;
            }
        }

        private static void InitStyle()
        {
            if (initialized)
                return;
            
            bodyStyle = new GUIStyle(EditorStyles.label);
            bodyStyle.wordWrap = true;
            bodyStyle.fontSize = 14;
            bodyStyle.richText = true;

            titleStyle = new GUIStyle(bodyStyle);
            titleStyle.fontSize = 28;
            titleStyle.fontStyle = FontStyle.Bold;
            
            introStyle = new GUIStyle(bodyStyle);
            introStyle.fontStyle = FontStyle.Normal;

            headingStyle = new GUIStyle(bodyStyle);
            headingStyle.fontStyle = FontStyle.Bold;
            headingStyle.fontSize = 18;

            linkStyle = new GUIStyle(bodyStyle);
            linkStyle.wordWrap = false;

            // Match selection color which works nicely for both light and dark skins
            linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
            linkStyle.stretchWidth = false;

            buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.fontStyle = FontStyle.Bold;

            initialized = true;
        }
        
        private static bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, linkStyle, options);

            Handles.BeginGUI();
            Handles.color = linkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            return GUI.Button(position, label, linkStyle);
        }
    }
}