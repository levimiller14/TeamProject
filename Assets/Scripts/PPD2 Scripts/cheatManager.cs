using UnityEngine;
using TMPro;
using System.Linq;

// can my push work?

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
                if(gameManager.instance.playerScript != null)
                {
                    gameManager.instance.playerScript.isGodMode = !gameManager.instance.playerScript.isGodMode;
                    feedbackText.text = gameManager.instance.playerScript.isGodMode ? "GODMODE ACTIVATED" : "GODMODE DEACTIVATED";
                }
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
        IDamage[] enemies = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDamage>().ToArray();

        int killCount = 0;

        foreach(IDamage enemy in enemies)
        {
            if (enemy is playerController) continue;

            enemy.takeDamage(999);
            killCount++;

        }

        feedbackText.text = "KILLED " + killCount + " ENEMIES";

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
