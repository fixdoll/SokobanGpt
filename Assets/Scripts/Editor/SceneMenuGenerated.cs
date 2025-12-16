using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

// AUTO-GENERATED FILE - Do not edit manually
// This file is regenerated automatically when scenes are added/removed
public static class SceneMenuGenerated
{
    private const string MENU_PATH = "Scenes/";

    [MenuItem(MENU_PATH + "EditorScene", false, 10)]
    private static void Open_EditorScene()
    {
        SceneMenuHelper.OpenScene("Assets/Scenes/EditorScene.unity");
    }

    [MenuItem(MENU_PATH + "MainMenuScene", false, 11)]
    private static void Open_MainMenuScene()
    {
        SceneMenuHelper.OpenScene("Assets/Scenes/MainMenuScene.unity");
    }

    [MenuItem(MENU_PATH + "PlayScene", false, 12)]
    private static void Open_PlayScene()
    {
        SceneMenuHelper.OpenScene("Assets/Scenes/PlayScene.unity");
    }

}
