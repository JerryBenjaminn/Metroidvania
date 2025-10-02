using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    public void OnResume()
    {
        PauseMenu.Toggle(); // tai PauseMenu.Hide() jos olet tehnyt sellaisen
    }
    public void OnSave()
    {
        GameManager.Instance.SaveGame();
    }
    public void OnLoadCurrentSlot()
    {
        var slot = SaveManager.Instance.HasActiveSlot ? SaveManager.Instance.CurrentSlot : 0;
        GameManager.Instance.LoadGame(slot);
    }
    public void OnQuitToMenu()
    {
        Time.timeScale = 1f; // varmuus ettei j‰‰dyt‰ p‰‰valikkoa
        SceneManager.LoadScene("MainMenu");
    }
}
