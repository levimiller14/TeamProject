using UnityEngine;

public class currencyManager : MonoBehaviour
{
    public static currencyManager Instance;

    [Header("Currency")]
    public int credits = 0;

   void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CanAfford(int cost)
    {
        return credits >= cost;
    }

    public void Spend(int cost)
    {
        credits -= cost;
        // TODO: update currency UI

    }
    public void addCredits(int amount)
    {
        credits += amount;
        // TODO: update currency UI
    }
}
