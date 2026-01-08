using UnityEngine;

[CreateAssetMenu]
public class healStats : ScriptableObject
{
    public GameObject healthPack;

    [Range(1, 10)] public int healAmt;
    [Range(1, 3)] public int healRate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
}
