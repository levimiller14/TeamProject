using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingIntro : MonoBehaviour
{

    [SerializeField] TMP_Text levelText;
    [SerializeField] Image loadBar;
    [SerializeField] TMP_Text continueText;

    [SerializeField] float loadingBarFilled = 3;

    [SerializeField] string loadScene;

    bool startGame;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadBar.fillAmount = 0;
        continueText.gameObject.SetActive(false);

        StartCoroutine(loadBarFill());
    }

    // Update is called once per frame
    void Update()
    {
        if(startGame && Input.anyKeyDown)
        {
            //playGame();

            loadMap();
        }
    }

    IEnumerator loadBarFill()
    {
        float timer = 0f;

        while(timer < loadingBarFilled)
        {
            timer += Time.deltaTime;
            loadBar.fillAmount = Mathf.Clamp01(timer / loadingBarFilled);
            yield return null;
        }
        startGame = true;
        continueText.gameObject.SetActive(true);
    }

    //void playGame()
    //{
    //    gameObject.SetActive(false);
    //    Debug.Log("continue");
   // }

    void loadMap()
    {
        SceneManager.LoadScene(loadScene);
    }
}
