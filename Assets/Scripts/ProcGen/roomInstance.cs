using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class roomInstance : MonoBehaviour
{
    [Header("----Room Data----")]
    public roomData data;
    public int roomIndex;
    public int depthFromStart;

    [Header("----Connections----")]
    public List<roomConnectionPoint> connectionPoints = new List<roomConnectionPoint>();
    public List<roomConnectionPoint> openConnections = new List<roomConnectionPoint>();

    [Header("----Spawn Points----")]
    public Transform playerSpawnPoint;
    public List<Transform> enemySpawnPoints = new List<Transform>();

    [Header("----Bounds----")]
    public BoxCollider roomBounds;

    void Awake()
    {
        collectConnectionPoints();
        collectSpawnPoints();
        findBounds();
    }

    void collectConnectionPoints()
    {
        connectionPoints.Clear();
        openConnections.Clear();

        var points = GetComponentsInChildren<roomConnectionPoint>();
        foreach (var point in points)
        {
            connectionPoints.Add(point);
            if (!point.isConnected)
            {
                openConnections.Add(point);
            }
        }
    }

    void collectSpawnPoints()
    {
        enemySpawnPoints.Clear();

        // find all transforms tagged as spawn points or named with spawn
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            string nameLower = child.name.ToLower();

            if (child.CompareTag("PlayerSpawn") || nameLower.Contains("playerspawn"))
            {
                playerSpawnPoint = child;
            }
            else if (child.CompareTag("EnemySpawn") || nameLower.Contains("enemyspawn"))
            {
                enemySpawnPoints.Add(child);
            }
        }
    }

    void findBounds()
    {
        // try to find existing bounds collider
        roomBounds = GetComponent<BoxCollider>();
        if (roomBounds == null)
        {
            // look for child named Bounds
            Transform boundsChild = transform.Find("Bounds");
            if (boundsChild != null)
            {
                roomBounds = boundsChild.GetComponent<BoxCollider>();
            }
        }
    }

    public void markConnectionUsed(roomConnectionPoint point)
    {
        point.isConnected = true;
        openConnections.Remove(point);
    }

    public bool hasOpenConnections() => openConnections.Count > 0;

    public roomConnectionPoint getRandomOpenConnection()
    {
        if (openConnections.Count == 0) return null;
        return openConnections[Random.Range(0, openConnections.Count)];
    }

    public roomConnectionPoint getConnectionWithDirection(connectionDirection dir)
    {
        foreach (var point in openConnections)
        {
            if (point.direction == dir)
            {
                return point;
            }
        }
        return null;
    }

    public Bounds getWorldBounds()
    {
        if (roomBounds != null)
        {
            return roomBounds.bounds;
        }

        // fallback: use data's room size centered on position
        if (data != null)
        {
            return new Bounds(transform.position, data.roomSize);
        }

        // default bounds
        return new Bounds(transform.position, new Vector3(10, 5, 10));
    }

    void OnDrawGizmosSelected()
    {
        Bounds b = getWorldBounds();
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(b.center, b.size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}
