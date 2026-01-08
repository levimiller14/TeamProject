using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public void resume()
    {
        gameManager.instance.stateUnpause();
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.stateUnpause();
    }

    public void quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Levi additions
    // open dialog menus (closes pause menu, opens specific dialog)
    public void openSettings()
    {
        gameManager.instance.menuPause.SetActive(false);
        gameManager.instance.menuSettings.SetActive(true);
        gameManager.instance.menuActive = gameManager.instance.menuSettings;
    }

    public void openStats()
    {
        gameManager.instance.menuPause.SetActive(false);
        gameManager.instance.menuStats.SetActive(true);
        gameManager.instance.menuActive = gameManager.instance.menuStats;

        // update stats on open
        if (statTracker.instance != null)
        {
            statTracker.instance.UpdateStatsDisplay();
        }
    }

    public void openCustomization()
    {
        gameManager.instance.menuPause.SetActive(false);
        gameManager.instance.menuCustomization.SetActive(true);
        gameManager.instance.menuActive = gameManager.instance.menuCustomization;
    }

    public void openCheats()
    {
        gameManager.instance.menuPause.SetActive(false);
        gameManager.instance.menuCheats.SetActive(true);
        gameManager.instance.menuActive = gameManager.instance.menuCheats;
    }

    // back button, returns to pause menu from any dialog
    public void backToPause()
    {
        gameManager.instance.menuActive.SetActive(false);
        gameManager.instance.menuPause.SetActive(true);
        gameManager.instance.menuActive = gameManager.instance.menuPause;
    }

}
