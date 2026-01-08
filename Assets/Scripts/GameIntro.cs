using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameIntro : MonoBehaviour
{
    
    [SerializeField] string lobbyScene = "Lobby";

    [SerializeField] float introLoad = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(intro());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator intro()
    {
        yield return new WaitForSeconds(introLoad);
        SceneManager.LoadScene(lobbyScene);
    }
}
