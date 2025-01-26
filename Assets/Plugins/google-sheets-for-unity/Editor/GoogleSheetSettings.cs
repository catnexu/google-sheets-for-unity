using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace catnexu.googlesheetsforunity.Editor
{
    [FilePath("Assets/Plugins/google-sheets-for-unity/" + nameof(GoogleSheetSettings) + ".asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class GoogleSheetSettings : ScriptableSingleton<GoogleSheetSettings>
    {
        [SerializeField] private string _googleApplicationName = string.Empty;
        [SerializeField] private string _userName = string.Empty;
        [SerializeField] private string _clientId = string.Empty;
        [SerializeField] private string _projectId = string.Empty;
        [SerializeField] private string _clientSecret = string.Empty;
        [SerializeField] private GoogleSheetTable[] _tables = Array.Empty<GoogleSheetTable>();

        public string ApplicationName => _googleApplicationName;
        public string User => _userName;

        public string Credentials =>
            string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_projectId) || string.IsNullOrEmpty(_clientSecret)
                ? string.Empty
                : "{\"installed\":{\"client_id\":\"" + _clientId + "\"," +
                "\"project_id\":\"" + _projectId + "\"," +
                "\"auth_uri\":\"https://accounts.google.com/o/oauth2/auth\"," +
                "\"token_uri\":\"https://oauth2.googleapis.com/token\"," +
                "\"auth_provider_x509_cert_url\":\"https://www.googleapis.com/oauth2/v1/certs\"," +
                "\"client_secret\":\"" + _clientSecret + "\"," +
                "\"redirect_uris\":[\"urn:ietf:wg:oauth:2.0:oob\"," +
                "\"http://localhost\"]}}";

        public bool TryGetTableId(string tableName, out string tableId)
        {
            tableId = string.Empty;
            for (var i = _tables.Length - 1; i >= 0; i--)
            {
                GoogleSheetTable table = _tables[i];
                if (table.Name == tableName)
                {
                    tableId = table.Id;
                    return true;
                }
            }

            return false;
        }

        public void SaveExternal()
        {
            Save(true);
        }
    }

    [CustomEditor(typeof(GoogleSheetSettings), false)]
    internal sealed class GoogleSheetSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GoogleSheetSettings data = target as GoogleSheetSettings;
            base.OnInspectorGUI();
            if (!data)
                return;
            if (GUILayout.Button("Save"))
            {
                data.SaveExternal();
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
            }
        }
    }

    internal sealed class GoogleSheetSettingsWindow : EditorWindow
    {
        private SerializedObject _data;

        [MenuItem("Tools/Utils/Google Sheets/Settings", priority = 1)]
        private static void InitWindow()
        {
            GoogleSheetSettingsWindow window = (GoogleSheetSettingsWindow) EditorWindow.GetWindow(typeof(GoogleSheetSettingsWindow));
            window.titleContent = new GUIContent(PlayerSettings.productName + " GoogleSheet settings");
            window.minSize = new Vector2(550, 300);
            window.Show();
        }

        private void CreateGUI()
        {
            ScrollView scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scroll.Add(new InspectorElement(GoogleSheetSettings.instance));
            rootVisualElement.Add(scroll);
        }
        
        private void OnDestroy()
        {
            DestroyImmediate(GoogleSheetSettings.instance);
        }
    }
}