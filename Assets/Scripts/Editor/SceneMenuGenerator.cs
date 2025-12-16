using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

[InitializeOnLoad]
public class SceneMenuGenerator
{
    private const string GENERATED_MENU_PATH = "Assets/Scripts/Editor/SceneMenuGenerated.cs";
    private const string MENU_PATH = "Scenes/";

    public static bool isGenerating = false;

    static SceneMenuGenerator()
    {
        // Generate menu items when Unity loads
        EditorApplication.delayCall += () =>
        {
            if (!isGenerating)
            {
                GenerateMenuItems();
            }
        };
        
        // Regenerate when assets are imported (scenes might be added/removed)
        AssetDatabase.importPackageCompleted += (packageName) =>
        {
            if (!isGenerating)
            {
                GenerateMenuItems();
            }
        };
    }

    [MenuItem("Tools/Regenerate Scene Menu", false, 1)]
    public static void GenerateMenuItems()
    {
        if (isGenerating) return;
        isGenerating = true;
        
        try
        {
            // Get all scene paths
            List<string> scenePaths = GetAllScenePaths();
            
            // Generate the menu script
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEditor;");
            sb.AppendLine("using UnityEditor.SceneManagement;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine();
            sb.AppendLine("// AUTO-GENERATED FILE - Do not edit manually");
            sb.AppendLine("// This file is regenerated automatically when scenes are added/removed");
            sb.AppendLine("public static class SceneMenuGenerated");
            sb.AppendLine("{");
            sb.AppendLine($"    private const string MENU_PATH = \"{MENU_PATH}\";");
            sb.AppendLine();
            
            int priority = 10;
            foreach (string scenePath in scenePaths)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                // Sanitize scene name for menu (remove invalid characters)
                string menuName = sceneName.Replace("/", "_").Replace("\\", "_");
                
                sb.AppendLine($"    [MenuItem(MENU_PATH + \"{menuName}\", false, {priority})]");
                sb.AppendLine($"    private static void Open_{menuName.Replace(" ", "_").Replace("-", "_")}()");
                sb.AppendLine("    {");
                sb.AppendLine($"        SceneMenuHelper.OpenScene(\"{scenePath}\");");
                sb.AppendLine("    }");
                sb.AppendLine();
                
                priority++;
            }
            
            sb.AppendLine("}");
            
            // Write the file
            File.WriteAllText(GENERATED_MENU_PATH, sb.ToString());
            
            // Refresh asset database
            AssetDatabase.Refresh();
            
            //Debug.Log($"Generated scene menu with {scenePaths.Count} scene(s)");
        }
        finally
        {
            isGenerating = false;
        }
    }

    private static List<string> GetAllScenePaths()
    {
        List<string> scenePaths = new List<string>();
        
        // Use AssetDatabase to find all scene assets
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".unity"))
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (sceneAsset != null)
                {
                    scenePaths.Add(path);
                }
            }
        }
        
        // Sort alphabetically
        return scenePaths.OrderBy(path => Path.GetFileNameWithoutExtension(path)).ToList();
    }
}

// Asset post-processor to detect scene changes
public class SceneMenuAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool scenesChanged = false;
        
        // Check if any scenes were imported, deleted, or moved
        foreach (string asset in importedAssets)
        {
            if (asset.EndsWith(".unity"))
            {
                scenesChanged = true;
                break;
            }
        }
        
        if (!scenesChanged)
        {
            foreach (string asset in deletedAssets)
            {
                if (asset.EndsWith(".unity"))
                {
                    scenesChanged = true;
                    break;
                }
            }
        }
        
        if (!scenesChanged)
        {
            foreach (string asset in movedAssets)
            {
                if (asset.EndsWith(".unity"))
                {
                    scenesChanged = true;
                    break;
                }
            }
        }
        
        // Regenerate menu if scenes changed
        if (scenesChanged && !SceneMenuGenerator.isGenerating)
        {
            EditorApplication.delayCall += SceneMenuGenerator.GenerateMenuItems;
        }
    }
}

// Helper class for opening scenes
public static class SceneMenuHelper
{
    public static void OpenScene(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError("Scene path is empty!");
            return;
        }

        // Verify scene exists
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        if (sceneAsset == null && !File.Exists(scenePath))
        {
            Debug.LogError($"Scene not found: {scenePath}");
            // Regenerate menu in case scenes were moved/deleted
            SceneMenuGenerator.GenerateMenuItems();
            return;
        }

        // Check if current scene needs saving
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            if (EditorUtility.DisplayDialog("Scene has been modified",
                "Do you want to save the changes you made to the scene?\n\n" +
                "Your changes will be lost if you don't save them.",
                "Save", "Don't Save"))
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }

        // Open the scene
        EditorSceneManager.OpenScene(scenePath);
        Debug.Log($"Opened scene: {Path.GetFileNameWithoutExtension(scenePath)}");
    }
}

