using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{

    [SerializeField] string asylumScene = "LoadingSceneAsylum";
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void LoadSceneAsylum()
    {
        SceneManager.LoadScene(asylumScene);
    }
}
