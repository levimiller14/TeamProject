using UnityEngine;

[CreateAssetMenu(menuName = "ProcGen/Level Settings")]
public class levelSettings : ScriptableObject
{
    [Header("----Level Size----")]
    [Range(3, 20)] public int minRooms = 5;
    [Range(3, 20)] public int maxRooms = 10;

    [Header("----Generation Rules----")]
    [Range(0f, 1f)] public float branchingChance = 0.3f;
    [Range(1, 5)] public int maxBranchDepth = 3;

    [Header("----Overlap Detection----")]
    [Tooltip("Extra padding between rooms")]
    public float roomPadding = 0.5f;
    public int maxPlacementAttempts = 10;

    [Header("----Difficulty Scaling----")]
    public bool scaleEnemiesWithDepth;
    [Range(0f, 2f)] public float enemyDensityMultiplier = 1f;

    [Header("----Seed----")]
    public bool useRandomSeed = true;
    public int fixedSeed = 12345;

    [Header("----Debug----")]
    public bool logGeneration;

    public int getTargetRoomCount()
    {
        return Random.Range(minRooms, maxRooms + 1);
    }

    public int getSeed()
    {
        return useRandomSeed ? System.Environment.TickCount : fixedSeed;
    }
}
