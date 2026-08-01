using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

using static UnityEditor.AssetDatabase;

namespace Nadaked.ProjectSetup.Editor
{
    public static class ProjectSetup
    {
        private const string ProjectFolder = "_Project";

        // =========================================================
        // IMPORT ESSENTIAL ASSETS
        // =========================================================

        [MenuItem("Tools/Setup/Import Essential Assets")]
        public static void ImportEssentialAssets()
        {
            ImportTextMeshProEssentials();
        }

        // =========================================================
        // INSTALL ESSENTIAL PACKAGES
        // =========================================================

        [MenuItem("Tools/Setup/Install Essential Packages")]
        public static void InstallEssentialPackages()
        {
            Packages.InstallPackages(new[]
            {
                // TextMeshPro / Unity UI
                "com.unity.ugui",

                // UniTask
                "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",

                // Unity Utils
                "https://github.com/adammyhre/Unity-Utils.git",

                // Alchemy
                "https://github.com/annulusgames/Alchemy.git?path=/Alchemy/Assets/Alchemy",

                // Selection History
                "https://github.com/acoppes/unity-history-window.git?path=/packages/com.gemserk.selectionhistory#v1.5.16",

                // PrimeTween
                "com.kyrylokuzyk.primetween@1.4.11"
            });
        }

        // =========================================================
        // CREATE FOLDERS
        // =========================================================

        [MenuItem("Tools/Setup/Create Folders")]
        public static void CreateFolders()
        {
            Folders.Create(
                ProjectFolder,
                "Resources",
                "Materials",
                "Models",
                "Prefabs",
                "Textures",
                "Scripts",
                "Editor",
                "ScriptableObjects",
                "Animations"
            );

            Refresh();

            Folders.MoveOrCreate(
                ProjectFolder,
                "Scenes"
            );

            Folders.MoveOrCreate(
                ProjectFolder,
                "Settings"
            );

            Refresh();

            MoveIfExists(
                "Assets/InputSystem_Actions.inputactions",
                $"Assets/{ProjectFolder}/Settings/InputSystem_Actions.inputactions"
            );

            DeleteIfExists("Assets/Readme.asset");
            DeleteIfExists("Assets/TutorialInfo");

            Refresh();

            Debug.Log("Project folders created.");
        }

        // =========================================================
        // TEXTMESHPRO
        // =========================================================

        private static void ImportTextMeshProEssentials()
        {
            const string tmpSettingsPath =
                "Assets/TextMesh Pro/Resources/TMP Settings.asset";

            if (LoadAssetAtPath<UnityEngine.Object>(tmpSettingsPath) != null)
            {
                Debug.Log("TMP Essential Resources already imported.");
                return;
            }

            var result = EditorApplication.ExecuteMenuItem(
                "Window/TextMeshPro/Import TMP Essential Resources"
            );

            if (result)
            {
                Debug.Log("TMP Essential Resources import started.");
            }
            else
            {
                Debug.LogError(
                    "Could not import TMP Essential Resources. " +
                    "Run Install Essential Packages first."
                );
            }
        }

        // =========================================================
        // PACKAGE INSTALLER
        // =========================================================

        private static class Packages
        {
            private static AddAndRemoveRequest _request;

            public static void InstallPackages(string[] packages)
            {
                if (_request != null && !_request.IsCompleted)
                {
                    Debug.LogWarning(
                        "Package installation is already running."
                    );

                    return;
                }

                EnsurePrimeTweenRegistry();

                Debug.Log("Installing essential packages...");

                _request = Client.AddAndRemove(
                    packages,
                    Array.Empty<string>()
                );

                EditorApplication.update -= PackageProgress;
                EditorApplication.update += PackageProgress;
            }

            private static void PackageProgress()
            {
                if (_request == null || !_request.IsCompleted)
                    return;

                EditorApplication.update -= PackageProgress;

                if (_request.Status != StatusCode.Success)
                {
                    if (_request.Status == StatusCode.Failure)
                    {
                        Debug.LogError(
                            $"Package installation failed: {_request.Error.message}"
                        );
                    }
                }
                else
                {
                    Debug.Log(
                        "Essential packages installed successfully."
                    );
                }

                _request = null;
            }

            // =====================================================
            // PRIMETWEEN REGISTRY
            // =====================================================

            private static void EnsurePrimeTweenRegistry()
            {
                var projectRoot =
                    Directory.GetParent(Application.dataPath)?.FullName;

                if (string.IsNullOrEmpty(projectRoot))
                {
                    Debug.LogError(
                        "Could not find Unity project root."
                    );

                    return;
                }

                var manifestPath = Path.Combine(
                    projectRoot,
                    "Packages",
                    "manifest.json"
                );

                if (!File.Exists(manifestPath))
                {
                    Debug.LogError(
                        "Packages/manifest.json not found."
                    );

                    return;
                }

                var manifest = File.ReadAllText(manifestPath);

                if (manifest.Contains("\"com.kyrylokuzyk\""))
                    return;

                const string registry =
                    "{\n" +
                    "      \"name\": \"npm\",\n" +
                    "      \"url\": \"https://registry.npmjs.org/\",\n" +
                    "      \"scopes\": [\n" +
                    "        \"com.kyrylokuzyk\"\n" +
                    "      ]\n" +
                    "    }";

                var scopedRegistriesIndex = manifest.IndexOf(
                    "\"scopedRegistries\"",
                    StringComparison.Ordinal
                );

                if (scopedRegistriesIndex >= 0)
                {
                    AddToExistingRegistries(
                        ref manifest,
                        scopedRegistriesIndex,
                        registry
                    );
                }
                else
                {
                    AddRegistrySection(
                        ref manifest,
                        registry
                    );
                }

                File.WriteAllText(
                    manifestPath,
                    manifest
                );

                Debug.Log(
                    "PrimeTween scoped registry added."
                );
            }

            private static void AddToExistingRegistries(
                ref string manifest,
                int scopedRegistriesIndex,
                string registry
            )
            {
                var arrayStart = manifest.IndexOf(
                    '[',
                    scopedRegistriesIndex
                );

                if (arrayStart < 0)
                {
                    Debug.LogError(
                        "Invalid scopedRegistries section."
                    );

                    return;
                }

                var arrayEnd = FindClosingBracket(
                    manifest,
                    arrayStart
                );

                if (arrayEnd < 0)
                {
                    Debug.LogError(
                        "Invalid scopedRegistries array."
                    );

                    return;
                }

                var existingContent = manifest.Substring(
                    arrayStart + 1,
                    arrayEnd - arrayStart - 1
                );

                var hasExistingRegistries =
                    !string.IsNullOrWhiteSpace(existingContent);

                var insertion = hasExistingRegistries
                    ? $",\n    {registry}"
                    : $"\n    {registry}\n  ";

                manifest = manifest.Insert(
                    arrayEnd,
                    insertion
                );
            }

            private static void AddRegistrySection(
                ref string manifest,
                string registry
            )
            {
                var rootEnd = manifest.LastIndexOf('}');

                if (rootEnd < 0)
                {
                    Debug.LogError(
                        "Invalid manifest.json."
                    );

                    return;
                }

                var previousIndex = rootEnd - 1;

                while (
                    previousIndex >= 0 &&
                    char.IsWhiteSpace(manifest[previousIndex])
                )
                {
                    previousIndex--;
                }

                var hasProperties =
                    previousIndex >= 0 &&
                    manifest[previousIndex] != '{';

                var section =
                    (hasProperties ? "," : "") +
                    "\n" +
                    "  \"scopedRegistries\": [\n" +
                    $"    {registry}\n" +
                    "  ]\n";

                manifest = manifest.Insert(
                    rootEnd,
                    section
                );
            }

            private static int FindClosingBracket(
                string text,
                int start
            )
            {
                var depth = 0;
                var insideString = false;
                var escaped = false;

                for (var i = start; i < text.Length; i++)
                {
                    var c = text[i];

                    if (insideString)
                    {
                        if (escaped)
                        {
                            escaped = false;
                            continue;
                        }

                        if (c == '\\')
                        {
                            escaped = true;
                            continue;
                        }

                        if (c == '"')
                            insideString = false;

                        continue;
                    }

                    if (c == '"')
                    {
                        insideString = true;
                        continue;
                    }

                    if (c == '[')
                    {
                        depth++;
                    }
                    else if (c == ']')
                    {
                        depth--;

                        if (depth == 0)
                            return i;
                    }
                }

                return -1;
            }
        }

        // =========================================================
        // ASSET HELPERS
        // =========================================================

        private static void MoveIfExists(
            string sourcePath,
            string destinationPath
        )
        {
            var asset =
                LoadAssetAtPath<UnityEngine.Object>(sourcePath);

            if (asset == null)
                return;

            if (
                LoadAssetAtPath<UnityEngine.Object>(
                    destinationPath
                ) != null
            )
            {
                Debug.LogWarning(
                    $"Asset already exists: {destinationPath}"
                );

                return;
            }

            var error = MoveAsset(
                sourcePath,
                destinationPath
            );

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(
                    $"Failed to move {sourcePath}: {error}"
                );
            }
        }

        private static void DeleteIfExists(
            string path
        )
        {
            var exists =
                IsValidFolder(path) ||
                LoadAssetAtPath<UnityEngine.Object>(path) != null;

            if (!exists)
                return;

            if (DeleteAsset(path))
            {
                Debug.Log($"Deleted: {path}");
            }
            else
            {
                Debug.LogWarning(
                    $"Could not delete: {path}"
                );
            }
        }

        // =========================================================
        // FOLDERS
        // =========================================================

        private static class Folders
        {
            public static void Create(
                string root,
                params string[] folders
            )
            {
                var rootPath = Path.Combine(
                    Application.dataPath,
                    root
                );

                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);

                foreach (var folder in folders)
                {
                    CreateSubFolders(
                        rootPath,
                        folder
                    );
                }
            }

            private static void CreateSubFolders(
                string rootPath,
                string folderHierarchy
            )
            {
                var folders =
                    folderHierarchy.Split('/');

                var currentPath = rootPath;

                foreach (var folder in folders)
                {
                    currentPath = Path.Combine(
                        currentPath,
                        folder
                    );

                    if (!Directory.Exists(currentPath))
                    {
                        Directory.CreateDirectory(
                            currentPath
                        );
                    }
                }
            }

            public static void MoveOrCreate(
                string newParent,
                string folderName
            )
            {
                var sourcePath =
                    $"Assets/{folderName}";

                var destinationPath =
                    $"Assets/{newParent}/{folderName}";

                if (IsValidFolder(destinationPath))
                    return;

                if (IsValidFolder(sourcePath))
                {
                    var error = MoveAsset(
                        sourcePath,
                        destinationPath
                    );

                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError(
                            $"Failed to move {folderName}: {error}"
                        );
                    }

                    return;
                }

                var physicalPath = Path.Combine(
                    Application.dataPath,
                    newParent,
                    folderName
                );

                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(
                        physicalPath
                    );
                }
            }
        }
    }
}