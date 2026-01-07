using UnityEngine;

public enum roomType
{
    Corridor,
    SmallRoom,
    MediumRoom,
    LargeRoom,
    Junction,
    DeadEnd
}

[CreateAssetMenu(menuName = "ProcGen/Room Data")]
public class roomData : ScriptableObject
{
    [Header("----Room Info----")]
    public string roomName;
    public GameObject roomPrefab;

    [Header("----Room Type----")]
    public roomType type;
    public bool isStartRoom;
    public bool isEndRoom;

    [Header("----Size Info----")]
    [Tooltip("Approximate bounds for overlap checking")]
    public Vector3 roomSize = new Vector3(10, 5, 10);

    [Header("----Spawn Settings----")]
    [Range(0, 10)] public int minEnemySpawns;
    [Range(0, 10)] public int maxEnemySpawns;
    public bool hasPlayerSpawn;

    public int getRandomEnemyCount()
    {
        return Random.Range(minEnemySpawns, maxEnemySpawns + 1);
    }
}
