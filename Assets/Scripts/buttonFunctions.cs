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

    //// Levi additions
    //// open dialog menus (closes pause menu, opens specific dialog)
    //public void openSettings()
    //{
    //    gameManager.instance.menuPause.SetActive(false);
    //}
    
    //public void openStats()
    //{

    //}

    //public void openCustomization()
    //{

    //}

    //public void openCheats()
    //{

    //}

    //public void backToPause()
    //{

    //}
}
