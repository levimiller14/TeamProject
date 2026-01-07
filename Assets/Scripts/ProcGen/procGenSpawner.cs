using System.Collections.Generic;
using UnityEngine;

public class procGenSpawner : MonoBehaviour
{
    public static procGenSpawner instance;

    [Header("----References----")]
    [SerializeField] levelGenerator levelGen;
    [SerializeField] levelThemeData theme;

    [Header("----Spawn Settings----")]
    [SerializeField] bool spawnOnGenComplete = true;
    [SerializeField] float spawnDelay = 0.1f;

    [Header("----Enemy Settings----")]
    [Range(0f, 2f)]
    [SerializeField] float enemyDensity = 1f;
    [SerializeField] bool skipStartRoom = true;
    [SerializeField] bool skipEndRoom = true;

    [Header("----Item Settings----")]
    [Range(0f, 1f)]
    [SerializeField] float pickupChance = 0.3f;

    List<GameObject> spawnedEnemies = new List<GameObject>();
    List<GameObject> spawnedItems = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (levelGen == null)
            levelGen = levelGenerator.instance;

        if (levelGen != null && spawnOnGenComplete)
        {
            levelGen.onGenerationComplete += onLevelGenerated;
        }
    }

    void OnDestroy()
    {
        if (levelGen != null)
        {
            levelGen.onGenerationComplete -= onLevelGenerated;
        }
    }

    void onLevelGenerated()
    {
        if (spawnDelay > 0)
        {
            Invoke(nameof(spawnAll), spawnDelay);
        }
        else
        {
            spawnAll();
        }
    }

    public void spawnAll()
    {
        clearSpawned();
        spawnEnemies();
        spawnPickups();
    }

    public void spawnEnemies()
    {
        if (levelGen == null || theme == null) return;

        var rooms = levelGen.getAllRooms();
        var startRoom = levelGen.getStartRoom();
        var endRoom = levelGen.getEndRoom();

        foreach (var room in rooms)
        {
            if (skipStartRoom && room == startRoom) continue;
            if (skipEndRoom && room == endRoom) continue;

            int baseCount = room.data != null ? room.data.getRandomEnemyCount() : 1;
            int enemyCount = Mathf.RoundToInt(baseCount * enemyDensity);

            // limit to available spawn points
            enemyCount = Mathf.Min(enemyCount, room.enemySpawnPoints.Count);

            // shuffle spawn points
            var shuffledPoints = new List<Transform>(room.enemySpawnPoints);
            shuffleList(shuffledPoints);

            for (int i = 0; i < enemyCount; i++)
            {
                GameObject enemyPrefab = theme.getRandomEnemy();
                if (enemyPrefab == null) continue;

                Transform spawnPoint = shuffledPoints[i];
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                spawnedEnemies.Add(enemy);
            }
        }
    }

    public void spawnPickups()
    {
        if (levelGen == null || theme == null) return;
        if (theme.pickupPrefabs == null || theme.pickupPrefabs.Length == 0) return;

        var rooms = levelGen.getAllRooms();

        foreach (var room in rooms)
        {
            // spawn pickup at random enemy spawn point with chance
            if (room.enemySpawnPoints.Count > 0 && Random.value < pickupChance)
            {
                Transform spawnPoint = room.enemySpawnPoints[Random.Range(0, room.enemySpawnPoints.Count)];
                GameObject pickupPrefab = theme.pickupPrefabs[Random.Range(0, theme.pickupPrefabs.Length)];

                if (pickupPrefab != null)
                {
                    // offset slightly so not on top of enemies
                    Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                    GameObject pickup = Instantiate(pickupPrefab, spawnPoint.position + offset, Quaternion.identity);
                    spawnedItems.Add(pickup);
                }
            }
        }
    }

    public void clearSpawned()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        spawnedEnemies.Clear();

        foreach (var item in spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }
        spawnedItems.Clear();
    }

    void shuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public int getEnemyCount() => spawnedEnemies.Count;
    public int getItemCount() => spawnedItems.Count;
}
