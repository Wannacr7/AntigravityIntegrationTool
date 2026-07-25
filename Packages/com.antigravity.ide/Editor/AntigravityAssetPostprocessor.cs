using Unity.CodeEditor;
using UnityEditor;

namespace Antigravity.Editor
{
    public class AntigravityAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            string currentEditor = CodeEditor.CurrentEditorInstallation;
            if (!string.IsNullOrEmpty(currentEditor) && AntigravityDiscovery.IsAntigravityPath(currentEditor))
            {
                var editor = new AntigravityScriptEditor();
                editor.SyncIfNeeded(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths, importedAssets);
            }
        }
    }
}
