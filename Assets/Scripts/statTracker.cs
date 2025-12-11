using UnityEngine;
using TMPro;

public class statTracker : MonoBehaviour
{
    public static statTracker instance;

    [Header("----Stats Display----")]
    [SerializeField] TMP_Text enemiesDefeatedText;
    [SerializeField] TMP_Text distanceTravelledText;
    [SerializeField] TMP_Text objectivesCompletedText;
    [SerializeField] TMP_Text shotsFiredText;
    [SerializeField] TMP_Text accuracyText;

    public int enemiesDefeated;
    public float distanceTravelled;
    public int objectivesCompleted;
    public int shotsFired;
    public int shotsHit;

    Vector3 lastPos;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if(gameManager.instance.player != null)
        {
            lastPos = gameManager.instance.player.transform.position;
        }

    }

    void Update()
    {
        if(gameManager.instance.player != null && !gameManager.instance.isPaused)
        {
            Vector3 currentPos = gameManager.instance.player.transform.position;
            float distanceThisFrame = Vector3.Distance(lastPos, currentPos);
            distanceTravelled += distanceThisFrame;
            lastPos = currentPos;
        }
    }

    public void UpdateStatsDisplay()
    {
        enemiesDefeatedText.text = enemiesDefeated.ToString();
        distanceTravelledText.text = distanceTravelled.ToString("F1") + "m";
        objectivesCompletedText.text = objectivesCompleted.ToString();
        shotsFiredText.text = shotsFired.ToString();

        float accuracy = shotsFired > 0 ? ((float)shotsHit / shotsFired) * 100f : 0f;
        accuracyText.text = accuracy.ToString("F1") + "%";

    }

    public void IncrementEnemiesDefeated()
    {
        enemiesDefeated++;
    }

    public void IncrementObjectives()
    {
        objectivesCompleted++;
    }

    public void IncrementShotsFired()
    {
        shotsFired++;
    }

    public void IncrementShotsHit()
    {
        shotsHit++;
    }
}
