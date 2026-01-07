using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class levelGenerator : MonoBehaviour
{
    public static levelGenerator instance;

    [Header("----Generation Settings----")]
    [SerializeField] levelSettings settings;
    [SerializeField] levelThemeData currentTheme;

    [Header("----Output----")]
    [SerializeField] Transform roomParent;

    [Header("----NavMesh----")]
    [SerializeField] NavMeshSurface navMeshSurface;
    [SerializeField] float navMeshBakeDelay = 0.5f;

    [Header("----Events----")]
    public System.Action onGenerationComplete;

    // runtime state
    List<roomInstance> spawnedRooms = new List<roomInstance>();
    Queue<roomConnectionPoint> openConnections = new Queue<roomConnectionPoint>();
    int currentRoomCount;
    int targetRoomCount;
    int currentSeed;

    roomInstance startRoom;
    roomInstance endRoom;

    public bool isGenerating { get; private set; }
    public bool isGenerated { get; private set; }

    void Awake()
    {
        instance = this;

        if (roomParent == null)
        {
            GameObject parent = new GameObject("GeneratedLevel");
            roomParent = parent.transform;
        }
    }

    public void generateLevel()
    {
        if (isGenerating) return;
        StartCoroutine(generateLevelCoroutine());
    }

    IEnumerator generateLevelCoroutine()
    {
        isGenerating = true;
        isGenerated = false;

        clearLevel();

        // init seed
        currentSeed = settings.getSeed();
        Random.InitState(currentSeed);

        targetRoomCount = settings.getTargetRoomCount();

        if (settings.logGeneration)
            Debug.Log($"[ProcGen] Starting generation. Seed: {currentSeed}, Target rooms: {targetRoomCount}");

        // phase 1: place start room
        placeStartRoom();
        yield return null;

        // phase 2: main generation loop
        while (currentRoomCount < targetRoomCount && openConnections.Count > 0)
        {
            bool placed = tryPlaceNextRoom();
            if (!placed && settings.logGeneration)
                Debug.Log("[ProcGen] Failed to place room, skipping connection");

            yield return null;
        }

        // phase 3: place end room
        placeEndRoom();
        yield return null;

        // phase 4: cap dead ends
        capDeadEnds();
        yield return null;

        // phase 5: bake navmesh
        if (navMeshSurface != null)
        {
            yield return new WaitForSeconds(navMeshBakeDelay);
            navMeshSurface.BuildNavMesh();

            if (settings.logGeneration)
                Debug.Log("[ProcGen] NavMesh baked");
        }

        isGenerating = false;
        isGenerated = true;

        if (settings.logGeneration)
            Debug.Log($"[ProcGen] Generation complete. Rooms placed: {currentRoomCount}");

        onGenerationComplete?.Invoke();
    }

    void placeStartRoom()
    {
        roomData startData = currentTheme.getRandomStartRoom();
        if (startData == null || startData.roomPrefab == null)
        {
            Debug.LogError("[ProcGen] No start room defined in theme!");
            return;
        }

        GameObject roomObj = Instantiate(startData.roomPrefab, Vector3.zero, Quaternion.identity, roomParent);
        roomInstance room = roomObj.GetComponent<roomInstance>();
        if (room == null)
        {
            room = roomObj.AddComponent<roomInstance>();
        }

        room.data = startData;
        room.roomIndex = currentRoomCount;
        room.depthFromStart = 0;

        spawnedRooms.Add(room);
        startRoom = room;
        currentRoomCount++;

        // queue all open connections
        foreach (var conn in room.openConnections)
        {
            openConnections.Enqueue(conn);
        }

        if (settings.logGeneration)
            Debug.Log($"[ProcGen] Placed start room: {startData.roomName}");
    }

    bool tryPlaceNextRoom()
    {
        if (openConnections.Count == 0) return false;

        roomConnectionPoint sourcePoint = openConnections.Dequeue();

        // skip if already connected
        if (sourcePoint.isConnected) return false;

        roomInstance sourceRoom = sourcePoint.GetComponentInParent<roomInstance>();
        if (sourceRoom == null) return false;

        // get opposite direction needed
        connectionDirection neededDir = roomConnectionPoint.getOpposite(sourcePoint.direction);

        // try to find a room that works
        for (int attempt = 0; attempt < settings.maxPlacementAttempts; attempt++)
        {
            roomData candidateData = currentTheme.getRandomRegularRoom();
            if (candidateData == null || candidateData.roomPrefab == null) continue;

            // check if candidate has a connection in the needed direction
            roomConnectionPoint[] candidatePoints = candidateData.roomPrefab.GetComponentsInChildren<roomConnectionPoint>();
            roomConnectionPoint matchingPoint = null;

            foreach (var point in candidatePoints)
            {
                if (point.direction == neededDir)
                {
                    matchingPoint = point;
                    break;
                }
            }

            if (matchingPoint == null) continue;

            // calculate placement
            Vector3 position;
            Quaternion rotation;
            calculatePlacement(sourcePoint, matchingPoint, candidateData.roomPrefab, out position, out rotation);

            // check for overlaps
            Bounds candidateBounds = new Bounds(position, candidateData.roomSize);
            candidateBounds.Expand(-settings.roomPadding); // shrink slightly for tolerance

            if (checkOverlap(candidateBounds, sourceRoom))
            {
                continue; // try another room
            }

            // spawn the room
            GameObject roomObj = Instantiate(candidateData.roomPrefab, position, rotation, roomParent);
            roomInstance room = roomObj.GetComponent<roomInstance>();
            if (room == null)
            {
                room = roomObj.AddComponent<roomInstance>();
            }

            room.data = candidateData;
            room.roomIndex = currentRoomCount;
            room.depthFromStart = sourceRoom.depthFromStart + 1;

            // find the actual matching point on the spawned room
            roomConnectionPoint actualMatchPoint = null;
            foreach (var point in room.connectionPoints)
            {
                if (point.direction == neededDir)
                {
                    actualMatchPoint = point;
                    break;
                }
            }

            // link connections
            sourcePoint.isConnected = true;
            sourcePoint.connectedTo = actualMatchPoint;
            sourceRoom.openConnections.Remove(sourcePoint);

            if (actualMatchPoint != null)
            {
                actualMatchPoint.isConnected = true;
                actualMatchPoint.connectedTo = sourcePoint;
                room.openConnections.Remove(actualMatchPoint);
            }

            spawnedRooms.Add(room);
            currentRoomCount++;

            // queue new connections (with branching chance)
            foreach (var conn in room.openConnections)
            {
                if (Random.value < settings.branchingChance || room.openConnections.Count == 1)
                {
                    openConnections.Enqueue(conn);
                }
            }

            if (settings.logGeneration)
                Debug.Log($"[ProcGen] Placed room: {candidateData.roomName} at depth {room.depthFromStart}");

            return true;
        }

        return false;
    }

    void calculatePlacement(roomConnectionPoint source, roomConnectionPoint target,
                           GameObject targetPrefab, out Vector3 position, out Quaternion rotation)
    {
        // source faces outward from existing room
        // target must face toward source (opposite direction)

        Vector3 sourceWorldDir = source.getWorldDirection();
        Vector3 targetLocalDir = roomConnectionPoint.getDirectionVector(target.direction);

        // rotation needed: target's direction should point toward source (opposite of source's direction)
        Vector3 desiredTargetDir = -sourceWorldDir;
        rotation = Quaternion.FromToRotation(targetLocalDir, desiredTargetDir);

        // position: align connection points
        Vector3 targetLocalPos = target.transform.localPosition;
        Vector3 rotatedOffset = rotation * targetLocalPos;
        position = source.getWorldPosition() - rotatedOffset;
    }

    bool checkOverlap(Bounds newBounds, roomInstance excludeRoom)
    {
        foreach (var room in spawnedRooms)
        {
            if (room == excludeRoom) continue;

            Bounds existingBounds = room.getWorldBounds();
            existingBounds.Expand(settings.roomPadding);

            if (newBounds.Intersects(existingBounds))
            {
                return true;
            }
        }
        return false;
    }

    void placeEndRoom()
    {
        // find deepest room with open connection
        roomInstance deepestRoom = null;
        int maxDepth = -1;

        foreach (var room in spawnedRooms)
        {
            if (room.hasOpenConnections() && room.depthFromStart > maxDepth)
            {
                maxDepth = room.depthFromStart;
                deepestRoom = room;
            }
        }

        if (deepestRoom == null)
        {
            if (settings.logGeneration)
                Debug.Log("[ProcGen] No room available for end room placement");
            return;
        }

        roomConnectionPoint sourcePoint = deepestRoom.getRandomOpenConnection();
        if (sourcePoint == null) return;

        roomData endData = currentTheme.getRandomEndRoom();
        if (endData == null || endData.roomPrefab == null)
        {
            if (settings.logGeneration)
                Debug.Log("[ProcGen] No end room defined in theme");
            return;
        }

        connectionDirection neededDir = roomConnectionPoint.getOpposite(sourcePoint.direction);
        roomConnectionPoint[] endPoints = endData.roomPrefab.GetComponentsInChildren<roomConnectionPoint>();
        roomConnectionPoint matchingPoint = null;

        foreach (var point in endPoints)
        {
            if (point.direction == neededDir)
            {
                matchingPoint = point;
                break;
            }
        }

        if (matchingPoint == null) return;

        Vector3 position;
        Quaternion rotation;
        calculatePlacement(sourcePoint, matchingPoint, endData.roomPrefab, out position, out rotation);

        GameObject roomObj = Instantiate(endData.roomPrefab, position, rotation, roomParent);
        roomInstance newRoom = roomObj.GetComponent<roomInstance>();
        if (newRoom == null)
        {
            newRoom = roomObj.AddComponent<roomInstance>();
        }

        newRoom.data = endData;
        newRoom.roomIndex = currentRoomCount;
        newRoom.depthFromStart = deepestRoom.depthFromStart + 1;

        // link connections
        sourcePoint.isConnected = true;
        deepestRoom.openConnections.Remove(sourcePoint);

        roomConnectionPoint actualMatchPoint = null;
        foreach (var point in newRoom.connectionPoints)
        {
            if (point.direction == neededDir)
            {
                actualMatchPoint = point;
                break;
            }
        }

        if (actualMatchPoint != null)
        {
            actualMatchPoint.isConnected = true;
            actualMatchPoint.connectedTo = sourcePoint;
            sourcePoint.connectedTo = actualMatchPoint;
            newRoom.openConnections.Remove(actualMatchPoint);
        }

        spawnedRooms.Add(newRoom);
        endRoom = newRoom;
        currentRoomCount++;

        if (settings.logGeneration)
            Debug.Log($"[ProcGen] Placed end room: {endData.roomName}");
    }

    void capDeadEnds()
    {
        int cappedCount = 0;

        foreach (var room in spawnedRooms)
        {
            foreach (var conn in new List<roomConnectionPoint>(room.openConnections))
            {
                roomData capData = currentTheme.getRandomDeadEndCap();
                if (capData == null || capData.roomPrefab == null) continue;

                connectionDirection neededDir = roomConnectionPoint.getOpposite(conn.direction);
                roomConnectionPoint[] capPoints = capData.roomPrefab.GetComponentsInChildren<roomConnectionPoint>();
                roomConnectionPoint matchingPoint = null;

                foreach (var point in capPoints)
                {
                    if (point.direction == neededDir)
                    {
                        matchingPoint = point;
                        break;
                    }
                }

                if (matchingPoint == null)
                {
                    // try placing cap directly at connection
                    Vector3 capDir = roomConnectionPoint.getDirectionVector(conn.direction);
                    Quaternion capRot = Quaternion.LookRotation(capDir);
                    Instantiate(capData.roomPrefab, conn.getWorldPosition(), capRot, room.transform);
                }
                else
                {
                    Vector3 position;
                    Quaternion rotation;
                    calculatePlacement(conn, matchingPoint, capData.roomPrefab, out position, out rotation);
                    Instantiate(capData.roomPrefab, position, rotation, room.transform);
                }

                conn.isConnected = true;
                room.openConnections.Remove(conn);
                cappedCount++;
            }
        }

        if (settings.logGeneration)
            Debug.Log($"[ProcGen] Capped {cappedCount} dead ends");
    }

    public void clearLevel()
    {
        foreach (var room in spawnedRooms)
        {
            if (room != null)
                Destroy(room.gameObject);
        }

        // also clear any children of roomParent not tracked
        if (roomParent != null)
        {
            foreach (Transform child in roomParent)
            {
                Destroy(child.gameObject);
            }
        }

        spawnedRooms.Clear();
        openConnections.Clear();
        currentRoomCount = 0;
        startRoom = null;
        endRoom = null;
        isGenerated = false;
    }

    public Transform getPlayerSpawnPoint()
    {
        if (startRoom != null && startRoom.playerSpawnPoint != null)
            return startRoom.playerSpawnPoint;
        if (startRoom != null)
            return startRoom.transform;
        return null;
    }

    public List<Transform> getAllEnemySpawnPoints()
    {
        var points = new List<Transform>();
        foreach (var room in spawnedRooms)
        {
            points.AddRange(room.enemySpawnPoints);
        }
        return points;
    }

    public List<roomInstance> getAllRooms() => new List<roomInstance>(spawnedRooms);

    public roomInstance getStartRoom() => startRoom;
    public roomInstance getEndRoom() => endRoom;
    public int getRoomCount() => currentRoomCount;
    public int getSeed() => currentSeed;
}
