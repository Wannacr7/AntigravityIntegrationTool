using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.CodeEditor;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class AntigravityDiscovery
    {
        public static CodeEditor.Installation[] GetInstallations()
        {
            var installations = new List<CodeEditor.Installation>();

            foreach (var path in GetPotentialPaths())
            {
                if (!string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path)))
                {
                    installations.Add(new CodeEditor.Installation
                    {
                        Name = "Antigravity IDE",
                        Path = path
                    });
                    break;
                }
            }

            return installations.ToArray();
        }

        public static IEnumerable<string> GetPotentialPaths()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                if (!string.IsNullOrEmpty(localAppData))
                {
                    yield return Path.Combine(localAppData, "Programs", "Antigravity IDE", "Antigravity IDE.exe");
                    yield return Path.Combine(localAppData, "Programs", "Antigravity IDE", "bin", "antigravity-ide.cmd");
                    yield return Path.Combine(localAppData, "Programs", "Antigravity", "Antigravity.exe");
                    yield return Path.Combine(localAppData, "agy", "bin", "agy.exe");
                }

                if (!string.IsNullOrEmpty(programFiles))
                {
                    yield return Path.Combine(programFiles, "Antigravity IDE", "Antigravity IDE.exe");
                    yield return Path.Combine(programFiles, "Antigravity", "Antigravity.exe");
                }

                if (!string.IsNullOrEmpty(programFilesX86))
                {
                    yield return Path.Combine(programFilesX86, "Antigravity IDE", "Antigravity IDE.exe");
                }
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                yield return "/Applications/Antigravity IDE.app";
                yield return "/Applications/Antigravity.app";
                var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                if (!string.IsNullOrEmpty(home))
                {
                    yield return Path.Combine(home, "Applications", "Antigravity IDE.app");
                }
            }
            else if (Application.platform == RuntimePlatform.LinuxEditor)
            {
                yield return "/usr/bin/antigravity-ide";
                yield return "/usr/bin/antigravity";
                yield return "/usr/local/bin/antigravity-ide";
                yield return "/snap/bin/antigravity-ide";
            }
        }

        public static bool IsAntigravityPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string normalizedPath = path.Replace('\\', '/').ToLowerInvariant();
            return normalizedPath.Contains("antigravity") || normalizedPath.Contains("agy");
        }
    }
}
