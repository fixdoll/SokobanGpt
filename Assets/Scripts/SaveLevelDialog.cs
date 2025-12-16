using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A dialog UI for saving levels - allows creating new or overwriting existing levels.
/// </summary>
public class SaveLevelDialog : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogPanel;
    public TMP_InputField levelNameInput;
    public TMP_Dropdown existingLevelsDropdown;
    public Button saveNewButton;
    public Button overwriteButton;
    public Button cancelButton;
    public TextMeshProUGUI statusText;

    private GridLevelEditor levelEditor;
    private List<string> existingLevelNames = new List<string>();

    private void Awake()
    {
        // Setup button listeners
        if (saveNewButton != null)
            saveNewButton.onClick.AddListener(OnSaveNewClicked);

        if (overwriteButton != null)
            overwriteButton.onClick.AddListener(OnOverwriteClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);

        // Initially hide the dialog
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    /// <summary>
    /// Opens the save dialog and populates it with existing levels.
    /// </summary>
    public void ShowDialog(GridLevelEditor editor)
    {
        levelEditor = editor;

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        // Set the current level name in the input field
        if (levelNameInput != null)
            levelNameInput.text = editor.levelName;

        // Populate dropdown with existing levels
        PopulateExistingLevels();

        // Clear status text
        if (statusText != null)
            statusText.text = "";
    }

    private void PopulateExistingLevels()
    {
        existingLevelNames.Clear();

        if (existingLevelsDropdown == null)
            return;

        // Load all existing levels from Resources/Levels
        LevelData[] existingLevels = Resources.LoadAll<LevelData>("Levels");

        List<string> dropdownOptions = new List<string>();

        if (existingLevels.Length > 0)
        {
            foreach (var level in existingLevels)
            {
                existingLevelNames.Add(level.levelName);
                dropdownOptions.Add(level.levelName);
            }
        }
        else
        {
            dropdownOptions.Add("No existing levels");
        }

        existingLevelsDropdown.ClearOptions();
        existingLevelsDropdown.AddOptions(dropdownOptions);

        // Enable/disable overwrite button based on whether there are existing levels
        if (overwriteButton != null)
            overwriteButton.interactable = existingLevels.Length > 0;
    }

    private void OnSaveNewClicked()
    {
        if (levelEditor == null)
        {
            ShowStatus("Error: Level editor reference is null!", true);
            return;
        }

        if (levelNameInput == null || string.IsNullOrWhiteSpace(levelNameInput.text))
        {
            ShowStatus("Please enter a level name!", true);
            return;
        }

        string newName = levelNameInput.text.Trim();

        // Check if name already exists
        if (existingLevelNames.Contains(newName))
        {
            ShowStatus($"Level '{newName}' already exists! Use Overwrite to replace it.", true);
            return;
        }

        // Update the level editor's level name and save
        levelEditor.levelName = newName;
        levelEditor.SaveLevelToScriptableObject();

        ShowStatus($"Level '{newName}' saved successfully!", false);

        // Close dialog after a short delay
        Invoke(nameof(CloseDialog), 1.5f);
    }

    private void OnOverwriteClicked()
    {
        if (levelEditor == null)
        {
            ShowStatus("Error: Level editor reference is null!", true);
            return;
        }

        if (existingLevelsDropdown == null || existingLevelNames.Count == 0)
        {
            ShowStatus("No existing levels to overwrite!", true);
            return;
        }

        int selectedIndex = existingLevelsDropdown.value;

        if (selectedIndex < 0 || selectedIndex >= existingLevelNames.Count)
        {
            ShowStatus("Invalid selection!", true);
            return;
        }

        string selectedLevelName = existingLevelNames[selectedIndex];

        // Update the level editor's level name and save (will overwrite existing)
        levelEditor.levelName = selectedLevelName;
        levelEditor.SaveLevelToScriptableObject();

        ShowStatus($"Level '{selectedLevelName}' overwritten successfully!", false);

        // Close dialog after a short delay
        Invoke(nameof(CloseDialog), 1.5f);
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

    private void ShowStatus(string message, bool isError)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = isError ? Color.red : Color.green;
        }
    }
}