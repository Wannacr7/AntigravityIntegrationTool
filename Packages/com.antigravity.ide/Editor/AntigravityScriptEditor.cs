using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Antigravity.Editor
{
    [InitializeOnLoad]
    public class AntigravityScriptEditor : IExternalCodeEditor
    {
        private const string k_AntigravityCustomPathKey = "Antigravity_CustomEditorPath";
        private readonly ProjectGeneration m_ProjectGeneration;

        static AntigravityScriptEditor()
        {
            CodeEditor.Register(new AntigravityScriptEditor());
        }

        public AntigravityScriptEditor()
        {
            m_ProjectGeneration = new ProjectGeneration(Directory.GetCurrentDirectory());
        }

        public CodeEditor.Installation[] Installations
        {
            get
            {
                var installations = AntigravityDiscovery.GetInstallations().ToList();

                string customPath = EditorPrefs.GetString(k_AntigravityCustomPathKey, string.Empty);
                if (!string.IsNullOrEmpty(customPath) && File.Exists(customPath))
                {
                    if (!installations.Any(i => string.Equals(i.Path, customPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        installations.Insert(0, new CodeEditor.Installation
                        {
                            Name = "Antigravity IDE (Custom)",
                            Path = customPath
                        });
                    }
                }

                return installations.ToArray();
            }
        }

        public System.Type[] CustomDataTypes => System.Array.Empty<System.Type>();

        public void Initialize(string editorInstallationPath)
        {
            if (!string.IsNullOrEmpty(editorInstallationPath) && File.Exists(editorInstallationPath))
            {
                EditorPrefs.SetString(k_AntigravityCustomPathKey, editorInstallationPath);
            }
            SyncAll();
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            if (AntigravityDiscovery.IsAntigravityPath(editorPath))
            {
                installation = new CodeEditor.Installation
                {
                    Name = "Antigravity IDE",
                    Path = editorPath
                };
                return true;
            }

            installation = default;
            return false;
        }

        public bool OpenProject(string filePath, int line, int column)
        {
            string projectDirectory = Directory.GetCurrentDirectory();

            // Sync solution/csproj first so Intellisense is active
            if (!m_ProjectGeneration.HasSolutionBeenGenerated())
            {
                m_ProjectGeneration.Sync();
            }

            string selectedEditorPath = CodeEditor.CurrentEditorInstallation;
            if (string.IsNullOrEmpty(selectedEditorPath) || !File.Exists(selectedEditorPath))
            {
                var detectedInstallations = Installations;
                if (detectedInstallations.Length > 0)
                {
                    selectedEditorPath = detectedInstallations[0].Path;
                }
            }

            if (string.IsNullOrEmpty(selectedEditorPath))
            {
                Debug.LogError("[Antigravity IDE] Antigravity executable path could not be found. Please select it in Preferences > External Tools.");
                return false;
            }

            string arguments;
            if (!string.IsNullOrEmpty(filePath))
            {
                string fullPath = Path.IsPathRooted(filePath) ? filePath : Path.GetFullPath(filePath);
                int targetLine = line > 0 ? line : 1;
                int targetColumn = column > 0 ? column : 1;

                arguments = $"\"{projectDirectory}\" -g \"{fullPath}:{targetLine}:{targetColumn}\"";
            }
            else
            {
                arguments = $"\"{projectDirectory}\"";
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = selectedEditorPath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    WorkingDirectory = projectDirectory
                };

                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Antigravity IDE] Failed to launch Antigravity IDE ({selectedEditorPath}): {ex.Message}");
                return false;
            }
        }

        public void SyncAll()
        {
            m_ProjectGeneration.Sync();
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            m_ProjectGeneration.SyncIfNeeded(addedFiles, deletedFiles, movedFiles, movedFromFiles, importedFiles);
        }

        public void OnGUI()
        {
            EditorGUILayout.LabelField("Antigravity IDE Options", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Antigravity IDE is configured as your external script editor.", MessageType.Info);

            if (GUILayout.Button("Regenerate C# Solution Files", GUILayout.Width(250)))
            {
                SyncAll();
                Debug.Log("[Antigravity IDE] C# Solution and .csproj files successfully regenerated!");
            }
        }
    }
}
