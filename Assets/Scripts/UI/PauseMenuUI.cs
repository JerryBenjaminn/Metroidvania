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
        // jos slot ei ole asetettu, ‰l‰ s‰ik‰yt‰ k‰ytt‰j‰‰ ñ ‰l‰ tallenna
        if (SaveManager.Instance.HasActiveSlot)
            GameManager.Instance.SaveGame();

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}
