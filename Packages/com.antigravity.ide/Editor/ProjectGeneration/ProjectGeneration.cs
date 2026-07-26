using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Antigravity.Editor
{
    public class ProjectGeneration
    {
        private readonly string m_ProjectDirectory;

        public ProjectGeneration(string projectDirectory)
        {
            m_ProjectDirectory = projectDirectory;
        }

        public bool HasSolutionBeenGenerated()
        {
            string solutionPath = GetSolutionFilePath();
            return File.Exists(solutionPath);
        }

        public void Sync()
        {
            SyncProjectFiles();
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            if (ShouldSync(addedFiles, deletedFiles, movedFiles, movedFromFiles, importedFiles) || !HasSolutionBeenGenerated())
            {
                Sync();
            }
        }

        private bool ShouldSync(params string[][] fileLists)
        {
            foreach (var fileList in fileLists)
            {
                if (fileList == null) continue;
                foreach (var file in fileList)
                {
                    if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".asmref", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string GetSolutionFilePath()
        {
            string projectName = Path.GetFileName(m_ProjectDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            return Path.Combine(m_ProjectDirectory, projectName + ".sln");
        }

        private void SyncProjectFiles()
        {
            var editorAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            var playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);

            var allAssemblies = editorAssemblies.Concat(playerAssemblies)
                .GroupBy(a => a.name)
                .Select(g => g.First())
                .ToList();

            string projectName = Path.GetFileName(m_ProjectDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string solutionPath = Path.Combine(m_ProjectDirectory, projectName + ".sln");

            List<string> projectFilePaths = new List<string>();

            foreach (var assembly in allAssemblies)
            {
                string csprojPath = Path.Combine(m_ProjectDirectory, assembly.name + ".csproj");
                string csprojContent = GenerateCsproj(assembly, allAssemblies);
                File.WriteAllText(csprojPath, csprojContent, Encoding.UTF8);
                projectFilePaths.Add(csprojPath);
            }

            string solutionContent = GenerateSolution(projectName, allAssemblies);
            File.WriteAllText(solutionPath, solutionContent, Encoding.UTF8);

            WriteIDESettings();
        }

        private string GenerateCsproj(Assembly assembly, List<Assembly> allAssemblies)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("    <LangVersion>latest</LangVersion>");
            sb.AppendLine("    <CsharpLangVersion>latest</CsharpLangVersion>");
            sb.AppendLine("    <TargetFramework>netstandard2.1</TargetFramework>");
            sb.AppendLine($"    <RootNamespace>{assembly.rootNamespace ?? ""}</RootNamespace>");
            sb.AppendLine($"    <AssemblyName>{assembly.name}</AssemblyName>");
            sb.AppendLine($"    <ProjectGuid>{SolutionGuidGenerator.GuidForProject(assembly.name)}</ProjectGuid>");
            sb.AppendLine("    <OutputType>Library</OutputType>");
            sb.AppendLine("    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>");
            sb.AppendLine("    <EnableDefaultItems>false</EnableDefaultItems>");
            sb.AppendLine("    <NoConfig>true</NoConfig>");
            sb.AppendLine("    <NoStdLib>true</NoStdLib>");
            sb.AppendLine("    <DisableImplicitFrameworkReferences>true</DisableImplicitFrameworkReferences>");
            sb.AppendLine("    <AddAdditionalExplicitAssemblyReferences>false</AddAdditionalExplicitAssemblyReferences>");
            sb.AppendLine("    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>");
            sb.AppendLine("  </PropertyGroup>");

            sb.AppendLine("  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">");
            sb.AppendLine("    <DebugSymbols>true</DebugSymbols>");
            sb.AppendLine("    <DebugType>full</DebugType>");
            sb.AppendLine("    <Optimize>false</Optimize>");
            sb.AppendLine("    <OutputPath>Temp\\bin\\Debug\\</OutputPath>");
            string defines = string.Join(";", assembly.defines);
            sb.AppendLine($"    <DefineConstants>{defines}</DefineConstants>");
            sb.AppendLine("    <ErrorReport>prompt</ErrorReport>");
            sb.AppendLine("    <WarningLevel>4</WarningLevel>");
            sb.AppendLine("    <NoWarn>0169;2003;0649;0414</NoWarn>");
            sb.AppendLine("  </PropertyGroup>");

            // Source Files
            sb.AppendLine("  <ItemGroup>");
            foreach (var sourceFile in assembly.sourceFiles)
            {
                string relativePath = sourceFile.Replace('/', '\\');
                sb.AppendLine($"    <Compile Include=\"{SecurityElement(relativePath)}\" />");
            }
            sb.AppendLine("  </ItemGroup>");

            // Compiled References (DLLs)
            sb.AppendLine("  <ItemGroup>");
            foreach (var dllRef in assembly.compiledAssemblyReferences)
            {
                string refName = Path.GetFileNameWithoutExtension(dllRef);
                sb.AppendLine($"    <Reference Include=\"{refName}\">");
                sb.AppendLine($"      <HintPath>{SecurityElement(dllRef)}</HintPath>");
                sb.AppendLine("    </Reference>");
            }
            sb.AppendLine("  </ItemGroup>");

            // Assembly Project References
            sb.AppendLine("  <ItemGroup>");
            foreach (var assemblyRef in assembly.assemblyReferences)
            {
                string refName = assemblyRef.name;
                string guid = SolutionGuidGenerator.GuidForProject(refName);
                sb.AppendLine($"    <ProjectReference Include=\"{refName}.csproj\">");
                sb.AppendLine($"      <Project>{guid}</Project>");
                sb.AppendLine($"      <Name>{refName}</Name>");
                sb.AppendLine("    </ProjectReference>");
            }
            sb.AppendLine("  </ItemGroup>");
            sb.AppendLine("</Project>");

            return sb.ToString();
        }

        private string GenerateSolution(string projectName, List<Assembly> assemblies)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            sb.AppendLine("# Visual Studio 2017");

            string csharpGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

            foreach (var assembly in assemblies)
            {
                string projectGuid = SolutionGuidGenerator.GuidForProject(assembly.name);
                sb.AppendLine($"Project(\"{csharpGuid}\") = \"{assembly.name}\", \"{assembly.name}.csproj\", \"{projectGuid}\"");
                sb.AppendLine("EndProject");
            }

            sb.AppendLine("Global");
            sb.AppendLine("	GlobalSection(SolutionConfigurationPlatforms) = preSolution");
            sb.AppendLine("		Debug|Any CPU = Debug|Any CPU");
            sb.AppendLine("		Release|Any CPU = Release|Any CPU");
            sb.AppendLine("	EndGlobalSection");
            sb.AppendLine("	GlobalSection(ProjectConfigurationPlatforms) = postSolution");

            foreach (var assembly in assemblies)
            {
                string projectGuid = SolutionGuidGenerator.GuidForProject(assembly.name);
                sb.AppendLine($"		{projectGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                sb.AppendLine($"		{projectGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                sb.AppendLine($"		{projectGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                sb.AppendLine($"		{projectGuid}.Release|Any CPU.Build.0 = Release|Any CPU");
            }

            sb.AppendLine("	EndGlobalSection");
            sb.AppendLine("EndGlobal");

            return sb.ToString();
        }

        private void WriteIDESettings()
        {
            string projectName = Path.GetFileName(m_ProjectDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string solutionName = projectName + ".sln";

            string settingsContent = $@"{{
  ""editor.codeLens"": true,
  ""[csharp]"": {{
    ""editor.codeLens"": true
  }},
  ""csharp.referencesCodeLens.enabled"": true,
  ""csharp.showReferencesCodeLens"": true,
  ""csharp.showMethodReferencesCodeLens"": true,
  ""dotnet.codeLens.enableReferencesCodeLens"": true,
  ""dotnet.codeLens.enableMethodReferencesCodeLens"": true,
  ""omnisharp.enableCodeLens"": true,
  ""omnisharp.enableDecompilationSupport"": true,
  ""omnisharp.enableImportCompletion"": true,
  ""dotnet.server.useOmnisharp"": true,
  ""dotnet.defaultSolution"": ""{solutionName}"",
  ""csharp.solution"": ""{solutionName}"",
  ""omnisharp.projectLoadTimeout"": 120,
  ""omnisharp.enableRoslynAnalyzers"": true,
  ""omnisharp.enableEditorConfigSupport"": true,
  ""csharp.inlayHints.parameters.enabled"": true,
  ""csharp.inlayHints.types.enabled"": true,
  ""dotnet.inlayHints.enableInlayHintsForParameters"": true,
  ""csharp.inlayHints.enableInlayHintsForTypes"": true,
  ""files.exclude"": {{
    ""**/.git"": true,
    ""**/.svn"": true,
    ""**/.hg"": true,
    ""**/CVS"": true,
    ""**/.DS_Store"": true,
    ""**/Library"": true,
    ""**/Temp"": true,
    ""**/Obj"": true,
    ""**/Build"": true,
    ""**/Builds"": true,
    ""**/Logs"": true,
    ""**/UserSettings"": true
  }}
}}";

            string[] targetDirectories = new[]
            {
                Path.Combine(m_ProjectDirectory, ".vscode"),
                Path.Combine(m_ProjectDirectory, ".antigravity")
            };

            foreach (var dir in targetDirectories)
            {
                try
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    string settingsPath = Path.Combine(dir, "settings.json");
                    File.WriteAllText(settingsPath, settingsContent, Encoding.UTF8);

                    string extensionsContent = @"{
  ""recommendations"": [
    ""ms-dotnettools.csharp"",
    ""ms-dotnettools.csdevkit""
  ]
}";
                    string extensionsPath = Path.Combine(dir, "extensions.json");
                    File.WriteAllText(extensionsPath, extensionsContent, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Antigravity IDE] Could not write settings in {dir}: {ex.Message}");
                }
            }
        }

        private static string SecurityElement(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("&", "&amp;")
                      .Replace("<", "&lt;")
                      .Replace(">", "&gt;")
                      .Replace("\"", "&quot;")
                      .Replace("'", "&apos;");
        }
    }
}
