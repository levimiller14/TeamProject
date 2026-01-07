using UnityEngine;

public enum levelTheme
{
    Industrial,
    Military,
    Research
}

[CreateAssetMenu(menuName = "ProcGen/Level Theme")]
public class levelThemeData : ScriptableObject
{
    [Header("----Theme Info----")]
    public string themeName;
    public levelTheme themeType;

    [Header("----Room Pools----")]
    public roomData[] startRooms;
    public roomData[] regularRooms;
    public roomData[] endRooms;
    public roomData[] deadEndCaps;

    [Header("----Enemy Pools----")]
    public GameObject[] enemyPrefabs;
    public GameObject[] bossPrefabs;

    [Header("----Item Pools----")]
    public GameObject[] pickupPrefabs;
    public GameObject[] hazardPrefabs;

    public roomData getRandomStartRoom()
    {
        if (startRooms == null || startRooms.Length == 0) return null;
        return startRooms[Random.Range(0, startRooms.Length)];
    }

    public roomData getRandomRegularRoom()
    {
        if (regularRooms == null || regularRooms.Length == 0) return null;
        return regularRooms[Random.Range(0, regularRooms.Length)];
    }

    public roomData getRandomEndRoom()
    {
        if (endRooms == null || endRooms.Length == 0) return null;
        return endRooms[Random.Range(0, endRooms.Length)];
    }

    public roomData getRandomDeadEndCap()
    {
        if (deadEndCaps == null || deadEndCaps.Length == 0) return null;
        return deadEndCaps[Random.Range(0, deadEndCaps.Length)];
    }

    public GameObject getRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;
        return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
    }
}
