using UnityEngine;

public class goalObject : MonoBehaviour
{
    private int goalChangeAmmount = 1;

    private void Start()
    {
        gameManager.instance.UpdateGameGoal(goalChangeAmmount);
    }
    private void OnTriggerEnter(Collider other)
    {
        // Updates goal tracker
        if (other.CompareTag("Player"))
        {
            if (gameManager.instance != null)
            {
                gameManager.instance.UpdateGameGoal(-goalChangeAmmount);
                Destroy(gameObject);
            }
        }
    }
}
