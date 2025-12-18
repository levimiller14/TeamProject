using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    // Levi additions
    // reusable dialog framework
    [Header("----Dialog Menus----")]
    public GameObject menuActive;
    public GameObject menuPause;
    public GameObject menuWin;
    public GameObject menuLose;
    public GameObject menuSettings;
    public GameObject menuStats;
    public GameObject menuCustomization;
    public GameObject menuCheats;
    // end Levi additions

    //[SerializeField] GameObject menuActive; // moving these to dialog framework as public instead of Serialized Fields for access in other script
    //[SerializeField] GameObject menuPause; 
    //[SerializeField] GameObject menuWin;
    //[SerializeField] GameObject menuLose;
    [SerializeField] TMP_Text gameGoalCountText;

    public alertSystem alertSys;
    public GameObject player;
    public playerController playerScript;
    public Image playerHPFrontBar;
    public Image playerHPBackBar;
    public GameObject playerDamageScreen;
    public GameObject playerHealScreen; //add

    // Aaron K add
    [Header("----Status Effect Icons----")]
    public Image poisonIcon;
    public Image poisonRing;
    public Image stunIcon;
    public Image stunRing;
    public Image frostIcon;
    public Image frostRing;

    public bool isPaused;

    float timeScaleOrig;
    int gameGoalCount;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            if(menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else //if(menuActive == menuPause)
            {
                stateUnpause();
            }
        }
    }

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void UpdateGameGoal(int amount)
    {
        gameGoalCount += amount;
        gameGoalCountText.text = gameGoalCount.ToString("F0");

        if(gameGoalCount <= 0)
        {
            // You Won!
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
}
