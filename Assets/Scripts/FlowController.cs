using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowController : MonoBehaviour
{
    public static FlowController Instance;

    public bool isEditorTest = false;
    public LevelData startingLevelData;

    private LevelLoadController loader;
    public LevelData[] startingLevels;
    public bool[] levelValids;

    private void Awake()
    {
        Instance = this;
        startingLevels = Resources.LoadAll<LevelData>("Levels");
    }

    void Start()
    {
        loader = LevelLoadController.Instance;
        levelValids = new bool[startingLevels.Length];


        //loader.LoadLevel(startingLevels[0]);

        /*Debug.Log(" === LEVEL VALIDATION ===");
        for (int i = 0; i < startingLevels.Length; i++)
        {
            LevelData lvl = startingLevels[i];
            Debug.Log(" === LEVEL " + lvl.levelName + " ===");
            levelValids[i] = LevelValidator.IsLevelSolvable(lvl);
        }*/
    }

    private int selectedIndex = 0;
    private bool showDropdown = false;

    private void OnGUI()
    {

        if (GUI.Button(new Rect(10, 10, 150, 20), startingLevels[selectedIndex].levelName + " - " + startingLevels[selectedIndex].levelDescription))
        {
            showDropdown = !showDropdown;
        }


        if (showDropdown)
        {
            for (int i = 0; i < startingLevels.Length; i++)
            {
                if (GUI.Button(new Rect(10, 35 + (20 * i), 150, 20), startingLevels[i].levelName + " - " + startingLevels[i].levelDescription))
                {
                    selectedIndex = i;
                    showDropdown = false;
                    loader.LoadLevel(startingLevels[selectedIndex]);

                }
            }
        }
        GUIStyle style = new GUIStyle();
        style.normal.textColor = levelValids[selectedIndex] ? Color.green : Color.red;
        string check = levelValids[selectedIndex] ? "solvable" : "not solvable";
        GUI.Box(new Rect(200, 10, 150, 20), check, style);
    }

    internal void LevelComplete()
    {
        if (isEditorTest)
        {
            //return to editor scene
            StartCoroutine(LoadEditorScene(startingLevelData));
        }
        else
        {
            Debug.LogWarning("TODO actual level completion logic :pepeD:");
        }
    }

    private IEnumerator LoadEditorScene(LevelData data)
    {
        var load = SceneManager.LoadSceneAsync("EditorScene", LoadSceneMode.Additive);

        yield return load;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("EditorScene"));
        GridLevelEditor.Instance.LoadLevelDataIntoEditor(startingLevelData);
        yield return SceneManager.UnloadSceneAsync("PlayScene");

        yield return null;
    }
}
