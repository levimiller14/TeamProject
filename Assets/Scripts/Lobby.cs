using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{

    [SerializeField] string asylum = "LoadingIntroAsylum";
    [SerializeField] string mansion = "LoadingIntroMansion";
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void loadSceneAsylum()
    {
        SceneManager.LoadScene(asylum);
    }

    public void loadSceneMansion()
    {
        //Debug.Log("Trying to load scene: " + mansion);
        SceneManager.LoadScene(mansion);
    }
}
