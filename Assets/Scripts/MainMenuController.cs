using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayButtonClicked()
    {

    }

    public void EditorButtonClicked()
    {
        SceneManager.LoadScene("EditorScene");
    }

    public void OptionsButtonClicked()
    {

    }

    public void ExitButtonClicked()
    {
#if !UNITY_EDITOR
        Application.Quit();
#else
        Debug.Log("Quit your job to become a game developer");
#endif
    }
}
