using System.IO;
using UnityEditor;
using UnityEditor.CodeEditor;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class AntigravityMenu
    {
        [MenuItem("Antigravity/Open Project in Antigravity IDE", false, 10)]
        public static void OpenProjectInAntigravity()
        {
            var editor = new AntigravityScriptEditor();
            editor.OpenProject(string.Empty, 0, 0);
        }

        [MenuItem("Antigravity/Regenerate C# Solution Files", false, 11)]
        public static void RegenerateSolutionFiles()
        {
            var editor = new AntigravityScriptEditor();
            editor.SyncAll();
            Debug.Log("[Antigravity IDE] C# Solution and .csproj files successfully regenerated!");
        }

        [MenuItem("Antigravity/Set as Active Unity Editor", false, 30)]
        public static void SetAsActiveEditor()
        {
            var installations = AntigravityDiscovery.GetInstallations();
            if (installations.Length > 0)
            {
                CodeEditor.SetExternalScriptEditor(installations[0].Path);
                Debug.Log($"[Antigravity IDE] Set Antigravity IDE as active editor: {installations[0].Path}");
            }
            else
            {
                Debug.LogWarning("[Antigravity IDE] Antigravity executable not found automatically. Please set it in Preferences > External Tools.");
            }
        }
    }
}
