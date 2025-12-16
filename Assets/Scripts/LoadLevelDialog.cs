using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A dialog UI for loading existing levels into the editor.
/// </summary>
public class LoadLevelDialog : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogPanel;
    public TMP_Dropdown existingLevelsDropdown;
    public Button loadButton;
    public Button cancelButton;

    private GridLevelEditor levelEditor;
    private List<LevelData> existingLevels = new List<LevelData>();

    private void Awake()
    {
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);

        // Add listener for dropdown selection change to preview level info
        if (existingLevelsDropdown != null)
            existingLevelsDropdown.onValueChanged.AddListener(OnDropdownSelectionChanged);

        // Initially hide the dialog
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    /// <summary>
    /// Opens the load dialog and populates it with existing levels.
    /// </summary>
    public void ShowDialog(GridLevelEditor editor)
    {
        levelEditor = editor;

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        // Populate dropdown with existing levels
        PopulateExistingLevels();

        // Show info for initially selected level
        OnDropdownSelectionChanged(existingLevelsDropdown != null ? existingLevelsDropdown.value : 0);
    }

    private void PopulateExistingLevels()
    {
        existingLevels.Clear();

        if (existingLevelsDropdown == null)
            return;

        // Load all existing levels from Resources/Levels
        LevelData[] loadedLevels = Resources.LoadAll<LevelData>("Levels");

        List<string> dropdownOptions = new List<string>();

        if (loadedLevels.Length > 0)
        {
            foreach (var level in loadedLevels)
            {
                existingLevels.Add(level);
                dropdownOptions.Add(level.levelName);
            }
        }
        else
        {
            dropdownOptions.Add("No existing levels");
        }

        existingLevelsDropdown.ClearOptions();
        existingLevelsDropdown.AddOptions(dropdownOptions);

        // Enable/disable load button based on whether there are existing levels
        if (loadButton != null)
            loadButton.interactable = loadedLevels.Length > 0;
    }

    private void OnDropdownSelectionChanged(int index)
    {
        if ( existingLevels.Count == 0 || index < 0 || index >= existingLevels.Count)
            return;

        LevelData selectedLevel = existingLevels[index];
    }

    private void OnLoadClicked()
    {
        if (levelEditor == null)
        {
            return;
        }

        if (existingLevelsDropdown == null || existingLevels.Count == 0)
        {
            return;
        }

        int selectedIndex = existingLevelsDropdown.value;

        if (selectedIndex < 0 || selectedIndex >= existingLevels.Count)
        {
            return;
        }

        LevelData selectedLevel = existingLevels[selectedIndex];

        // Load the selected level into the editor
        levelEditor.LoadLevelFromScriptableObject(selectedLevel);

        Invoke(nameof(CloseDialog), 0f);
    }

    private void OnCancelClicked()
    {
        CloseDialog();
    }

    private void CloseDialog()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        levelEditor = null;
    }
}