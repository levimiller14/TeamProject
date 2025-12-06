using UnityEngine;
using TMPro;

public class cheatManager : MonoBehaviour
{
    [Header("----Cheat UI----")]
    [SerializeField] TMP_InputField cheatInputField;
    [SerializeField] TMP_Text feedbackText;

    private void Start()
    {
        if(feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitCheat();
        }
    }

    public void SubmitCheat()
    {
        string cheatCode = cheatInputField.text.ToUpper().Trim();

        switch(cheatCode)
        {
            case "GODMODE":
                feedbackText.text = "GODMODE ACTIVATED";
                break;
            case "KILLALL":
                KillAllEnemies();
                break;
            case "REFILLHEALTH":
                RefillHealth();
                break;
            default:
                feedbackText.text = "INVALID ENTRY";
                break;
        }

        cheatInputField.text = "";

    }

    void KillAllEnemies()
    {
        enemyAI[] enemies = FindObjectsOfType<enemyAI>();
        foreach(enemyAI enemy in enemies)
        {
            enemy.takeDamage(999);
        }

        feedbackText.text = "ALL ENEMIES DEAD";

    }

    void RefillHealth()
    {
        if(gameManager.instance.playerScript != null)
        {
            gameManager.instance.playerScript.HP = gameManager.instance.playerScript.HPOrig;
            gameManager.instance.playerScript.updatePlayerUI();
            feedbackText.text = "HEALTH REFILLED";
        }
    }
}
