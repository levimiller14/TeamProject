using UnityEngine;

public class difficultyManager : MonoBehaviour
{
    public static difficultyManager instance;

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    public Difficulty currentDifficulty = Difficulty.Normal;

    // health mult for enemies
    public float GetHealthMultiplier()
    {
        switch(currentDifficulty)
        {
            case Difficulty.Easy: return 0.75f;
            case Difficulty.Normal: return 1f;
            case Difficulty.Hard: return 1.5f;
            default: return 1f;
        }
    }

    // dmg mult for enemies
    public float GetDamageMultiplier()
    {
        switch(currentDifficulty)
        {
            case Difficulty.Easy: return 0.75f;
            case Difficulty.Normal: return 1f;
            case Difficulty.Hard: return 1.5f;
            default: return 1f;
        }
    }

    // dog detection radius mult
    public float GetDogDetectionMultiplier()
    {
        switch(currentDifficulty)
        {
            case Difficulty.Easy: return 0.75f;
            case Difficulty.Normal: return 1f;
            case Difficulty.Hard: return 1.5f;
            default: return 1f;
        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDifficulty(Difficulty newDifficulty)
    {
        currentDifficulty = newDifficulty;
    }
}
