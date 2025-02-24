﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using RandomR = UnityEngine.Random;
using Graphs;

public class Generator3D : MonoBehaviour 
{
    private enum CellType 
    {
        None,
        Room,
        Hallway,
        Stairs
    }

    private class Room 
    {
        public BoundsInt bounds;

        public Room(Vector3Int location, Vector3Int size) 
        {
            bounds = new BoundsInt(location, size);
        }

        public static bool Intersect(Room a, Room b) 
        {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y)
                || (a.bounds.position.z >= (b.bounds.position.z + b.bounds.size.z)) || ((a.bounds.position.z + a.bounds.size.z) <= b.bounds.position.z));
        }
    }

    [SerializeField] public List<MapRoomObjManagerData> mRoomObjManagerList;
    [SerializeField] private List<GameObject> randomWeaponlist;
    [SerializeField] private int seed;
    [SerializeField] private Vector3Int size;
    [SerializeField] private int roomCount;
    [SerializeField] private float hallwayDensity;
    [SerializeField] private Vector3Int roomMaxSize;
    [SerializeField] private Vector3Int roomMinSize;
    [SerializeField] private List<GameObject> floorPrefab;
    [SerializeField] private GameObject ceilingPrefab;
    [SerializeField] private List<GameObject> wallPrefab;
    [SerializeField] private GameObject wallDoorPrefab;
    [SerializeField] private GameObject hallwayPrefab;
    [SerializeField] private GameObject stairPrefab;
    [SerializeField] private GameObject roomLightPrefab;
    [SerializeField] private GameObject hallwayLightPrefab;
    [SerializeField] private GameObject playerObj;
    private GameObject camObj;
    [SerializeField] private GameObject endObj;

    // algorithm & others
    private Random random;
    private Grid3D<CellType> grid;
    private List<Room> rooms;
    private Delaunay3D delaunay;
    private HashSet<Prim.Edge> selectedEdges;
    // storage lists
    private List<List<Vector3Int>> pathList;
    private List<GameObject> doorList;
    private List<GameObject> mapContent;
    private List<Room> enemyRooms;
    private List<RoomData> enemyRoomData;
    // enemy room
    private Room EndRoom;
    private List<GameObject> currEnemiesInRoom;
    private Room currEnemyRoom;
    // floor
    private MapRoomObjManagerData mRoomObjManager;
    private int floorNum;

    void Awake()
    {
        if (AudioManager.Instance != null)
        {
            int rand = RandomR.Range(0, 2);
            string bgmToPlay = string.Empty;

            switch (rand)
            {
                case 0:
                    bgmToPlay = "BGMSunsetSchool";
                    break;
                case 1:
                    bgmToPlay = "BGMDistorted";
                    break;
            }

            AudioManager.Instance.StartCoroutine(AudioManager.Instance.PlayBGM(bgmToPlay));
        }
    }

    void Start()
    {
        camObj = GameObject.FindGameObjectWithTag("CameraHolder");
        ChangeSeed();
        InitializeMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ChangeSeed();
            foreach (var obj in mapContent)
            {
                StartCoroutine(ObjectPoolManager.Instance.ReturnObjectToPool(obj));
            }
            InitializeMap();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            foreach (var obj in mapContent)
            {
                StartCoroutine(ObjectPoolManager.Instance.ReturnObjectToPool(obj));
            }
            InitializeMap();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            foreach (var enemy in currEnemiesInRoom)
            {
                RemoveEnemyFromRoom(enemy);
            }
        }

        CheckEnemyRoomActivation();
    }

    private void ChangeSeed()
    {
        seed = RandomR.Range(10000000, 99999999);
    }

    private void InitializeMap()
    {
        random = new Random(seed);
        grid = new Grid3D<CellType>(size, Vector3Int.zero);
        rooms = new List<Room>();

        pathList = new List<List<Vector3Int>>();
        doorList = new List<GameObject>();
        mapContent = new List<GameObject>();
        enemyRooms = new List<Room>();
        enemyRoomData = new List<RoomData>();
        currEnemiesInRoom = new List<GameObject>();
        currEnemyRoom = null;
        EndRoom = null;
        Debug.Log("Level " + GameManager.Instance.floorNum);
        if (floorNum >= mRoomObjManagerList.Count + 1)
        {
            GameManager.Instance.floorNum--;
        }
        floorNum = GameManager.Instance.floorNum;
        mRoomObjManager = mRoomObjManagerList[floorNum - 1];

        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();
        DeleteWalls();
        InitRoomObjects();
    }

    public IEnumerator ClearMap(bool loadNextLevel)
    {
        GameManager.Instance.floorNum = floorNum + 1;
        foreach (var obj in mapContent)
        {
            StartCoroutine(ObjectPoolManager.Instance.ReturnObjectToPool(obj));
        }
        yield return null;
        if (loadNextLevel)
        {
            GameManager.Instance.ChangeScene("LevelScene");
        }
    }

    private void PlaceRooms() 
    {
        for (int i = 0; i < roomCount; i++) 
        {

            Vector3Int roomSize = new Vector3Int(
                random.Next(roomMinSize.x + 1, roomMaxSize.x + 1),
                random.Next(roomMinSize.y + 1, roomMaxSize.y + 1),
                random.Next(roomMinSize.z + 1, roomMaxSize.z + 1)
            );

            Vector3Int location = new Vector3Int(
                random.Next(0, size.x),
                random.Next(0, size.y),
                random.Next(0, size.z)
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector3Int(-1, 0, -1), roomSize + new Vector3Int(2, 0, 2));

            foreach (var room in rooms) 
            {
                if (Room.Intersect(room, buffer)) 
                {
                    add = false;
                    break;
                }
            }

            if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y
                || newRoom.bounds.zMin < 0 || newRoom.bounds.zMax >= size.z) 
            {
                add = false;
            }

            if (add) 
            {
                rooms.Add(newRoom);
                PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                foreach (var pos in newRoom.bounds.allPositionsWithin) 
                {
                    grid[pos] = CellType.Room;
                }
            }
        }
    }

    private void Triangulate() 
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms) 
        {
            vertices.Add(new Vertex<Room>((Vector3)room.bounds.position + ((Vector3)room.bounds.size) / 2, room));
        }

        delaunay = Delaunay3D.Triangulate(vertices);
    }

    private void CreateHallways() 
    {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges) 
        {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges) 
        {
            if (random.NextDouble() < hallwayDensity) 
            {
                selectedEdges.Add(edge);
            }
        }
    }

    private void PathfindHallways() 
    {
        DungeonPathfinder3D aStar = new DungeonPathfinder3D(size);

        foreach (var edge in selectedEdges) 
        {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)(startPosf.y - 0.5f), (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x, (int)(endPosf.y - 0.5f), (int)endPosf.z);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) => 
            {
                var pathCost = new DungeonPathfinder3D.PathCost();

                var delta = b.Position - a.Position;

                if (delta.y == 0) 
                {
                    //flat hallway
                    pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

                    if (grid[b.Position] == CellType.Stairs) 
                    {
                        return pathCost;
                    } 
                    else if (grid[b.Position] == CellType.Room) 
                    {
                        pathCost.cost += 5;
                    } 
                    else if (grid[b.Position] == CellType.None) 
                    {
                        pathCost.cost += 1;
                    }

                    pathCost.traversable = true;
                } 
                else 
                {
                    //staircase
                    if ((grid[a.Position] != CellType.None && grid[a.Position] != CellType.Hallway)
                        || (grid[b.Position] != CellType.None && grid[b.Position] != CellType.Hallway)) return pathCost;

                    pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);    //base cost + heuristic

                    int xDir = Mathf.Clamp(delta.x, -1, 1);
                    int zDir = Mathf.Clamp(delta.z, -1, 1);
                    Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                    Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                    if (!grid.InBounds(a.Position + verticalOffset)
                        || !grid.InBounds(a.Position + horizontalOffset)
                        || !grid.InBounds(a.Position + verticalOffset + horizontalOffset)) 
                    {
                        return pathCost;
                    }

                    if (grid[a.Position + horizontalOffset] != CellType.None
                        || grid[a.Position + horizontalOffset * 2] != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset] != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None) 
                    {
                        return pathCost;
                    }

                    pathCost.traversable = true;
                    pathCost.isStairs = true;
                }

                return pathCost;
            });

            if (path != null) 
            {

                pathList.Add(path);

                for (int i = 0; i < path.Count; i++) 
                {
                    var current = path[i];

                    if (grid[current] == CellType.None) 
                    {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0) 
                    {
                        var prev = path[i - 1];

                        var delta = current - prev;

                        if (delta.y != 0) 
                        {
                            int xDir = Mathf.Clamp(delta.x, -1, 1);
                            int zDir = Mathf.Clamp(delta.z, -1, 1);
                            Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);
                            
                            grid[prev + horizontalOffset] = CellType.Stairs;
                            grid[prev + horizontalOffset * 2] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset * 2] = CellType.Stairs;

                            // spawn stairs
                            // going up
                            if (delta.y > 0)
                            {
                                Vector3 tilePos1 = prev + (Vector3)horizontalOffset * 1.5f + new Vector3(0.5f, -1.5f, 0.5f);
                                if (delta.x > 0)
                                {
                                    SpawnTileWithRotation(stairPrefab, tilePos1, -90);
                                }
                                else if (delta.x < 0)
                                {
                                    SpawnTileWithRotation(stairPrefab, tilePos1, 90);
                                }
                                else if (delta.z > 0)
                                {
                                    SpawnTileWithRotation(stairPrefab, tilePos1, 180);
                                }
                                else if (delta.z < 0)
                                {
                                    SpawnTileWithRotation(stairPrefab, tilePos1, 0);
                                }
                            }
                            // goind down
                            if (delta.y < 0)
                            {
                                Vector3 tilePos2 = prev + verticalOffset + (Vector3)horizontalOffset * 1.5f + new Vector3(0.5f, -1.5f, 0.5f);
                                if (delta.x > 0)
                                {
                                    SpawnTileWithRotation(stairPrefab, tilePos2, 90);
                                }
                                else if (delta.x < 0)
                                {
                                    SpawnTileWithRotation(stairPrefab, tilePos2, -90);
                                }
                                else if (delta.z > 0)
                                {
                                    SpawnTileWithRotation(stairPrefab, tilePos2, 0);
                                }
                                else if (delta.z < 0)
                                {
                                    SpawnTileWithRotation(stairPrefab, tilePos2, 180);
                                }
                            }
                        }
                    }
                }
                // spawn hallway
                List<Vector3Int> tempList = new List<Vector3Int>();
                for(int i = 0; i < path.Count; i++) 
                {
                    Vector3Int pos = path[i];
                    if (tempList.Count == 0)
                    {
                        bool check = true;
                        foreach (var p in tempList)
                        {
                            if (pos == p)
                            {
                                check = false;
                            }
                        }
                        if (grid[pos] == CellType.Hallway && check)
                        {
                            PlaceHallway(pos, i);
                            tempList.Add(pos);
                        }
                    }
                    else
                    {
                        if (grid[pos] == CellType.Hallway)
                        {
                            PlaceHallway(pos, i);
                            tempList.Add(pos);
                        }
                    }
                }
            }
        }
    }

    private void DeleteWalls()
    {
        // get paths
        foreach (var path in pathList)
        {
            if (path != null)
            {
                Vector3 offset = new Vector3(0.55f, -0.75f, 0.5f);
                for (int i = 0; i < path.Count - 1; i++)
                {
                    // check in between each path node
                    Debug.DrawLine(path[i] + offset, path[i + 1] + offset, UnityEngine.Color.blue, 60, false);
                    RaycastHit[] hitList = Physics.RaycastAll(path[i] + offset, path[i + 1] - path[i], (path[i] - path[i + 1]).magnitude);
                    foreach (RaycastHit hit in hitList)
                    {
                        // if theres a wall
                        if (hit.transform.CompareTag("MapWallTile"))
                        {
                            // place pillars object
                            if (hit.transform.gameObject.name != "Wall")
                            {
                                bool placeDoor = true;
                                foreach (var doorPos in doorList)
                                {
                                    if (hit.transform.position == doorPos.transform.position)
                                    {
                                        placeDoor = false;
                                        break;
                                    }
                                }
                                if (placeDoor)
                                {
                                    doorList.Add(SpawnTileWithRotation(wallDoorPrefab, hit.transform.position, hit.transform.eulerAngles.y));
                                }
                            }
                            // Destroy because it is unable to be pooled as it is part of a prefab
                            // (To create path through rooms)
                            hit.transform.gameObject.SetActive(false);
                        }
                    }
                }
                Debug.DrawLine(path[0] + offset, path[1] + offset, UnityEngine.Color.green, 60, false);
                Debug.DrawLine(path[path.Count - 2] + offset, path[path.Count - 1] + offset, UnityEngine.Color.red, 60, false);
            }
        }
    }

    private void InitRoomObjects()
    {
        // place light in the middle of the room
        GameObject obj = ObjectPoolManager.Instance.SpawnObject(roomLightPrefab, rooms[0].bounds.center + new Vector3(0, -0.15f, 0), Quaternion.identity, ObjectPoolManager.PoolType.Map);
        mapContent.Add(obj);
        PlaceRoomObjects(rooms[0].bounds.position, rooms[0].bounds.size, mRoomObjManager.startRoomData, 0);
        Vector3 playerStartPos = rooms[0].bounds.center + Vector3.down;
        playerObj.transform.position = playerStartPos;
        camObj.transform.position = playerStartPos;

        bool isEndPlaced = false;
        for (int i = 1; i < rooms.Count; i++)
        {
            // place light in the middle of the room
            obj = ObjectPoolManager.Instance.SpawnObject(roomLightPrefab, rooms[i].bounds.center + new Vector3(0, -0.15f, 0), Quaternion.identity, ObjectPoolManager.PoolType.Map);
            mapContent.Add(obj);

            if (!isEndPlaced)
            {
                // try place end
                Vector3 playerEndPos = rooms[i].bounds.center;
                // if end is far enough away from start or if it's the last room
                if (Vector3.Distance(playerStartPos, playerEndPos) > size.x / 2 || 1 == roomCount - 1)
                {
                    isEndPlaced = true;
                    if (mRoomObjManager.endRoomData.name.Contains("Boss"))
                    {
                        // add room to enemy room for later spawning
                        enemyRooms.Add(rooms[i]);
                        enemyRoomData.Add(mRoomObjManager.endRoomData);
                        endObj.SetActive(false);
                        EndRoom = rooms[i];
                    }
                    else
                    {
                        PlaceRoomObjects(rooms[i].bounds.position, rooms[i].bounds.size, mRoomObjManager.endRoomData, -0.4f);
                    }
                    endObj.transform.position = playerEndPos + new Vector3(0, -1.8f, 0);
                }
                // randomise other tyes of rooms
                else
                {
                    int contentRoomIndex = RandomR.Range(0, mRoomObjManager.contentRoomData.Count);
                    if (mRoomObjManager.contentRoomData[contentRoomIndex].name.Contains("Enemy"))
                    {
                        // add room to enemy room for later spawning
                        enemyRooms.Add(rooms[i]);
                        enemyRoomData.Add(mRoomObjManager.contentRoomData[contentRoomIndex]);
                    }
                    else
                    {
                        // place rest of the objects
                        PlaceRoomObjects(rooms[i].bounds.position, rooms[i].bounds.size, mRoomObjManager.contentRoomData[contentRoomIndex], -0.4f);
                    }
                }
            }
            // randomise other tyes of rooms
            else
            {
                int contentRoomIndex = RandomR.Range(0, mRoomObjManager.contentRoomData.Count);
                if (mRoomObjManager.contentRoomData[contentRoomIndex].name.Contains("Enemy"))
                {
                    // add room to enemy room for later spawning
                    enemyRooms.Add(rooms[i]);
                    enemyRoomData.Add(mRoomObjManager.contentRoomData[contentRoomIndex]);
                }
                else
                {
                    // place rest of the objects
                    PlaceRoomObjects(rooms[i].bounds.position, rooms[i].bounds.size, mRoomObjManager.contentRoomData[contentRoomIndex], -0.4f);
                }
            }
        }
    }

    private void PlaceRoomObjects(Vector3Int location, Vector3Int size, RoomData data, float vertOffset)
    {
        // set room type
        RoomData roomData = data;
        // create list of available spaces
        List<Vector3> vacantSpaces = new List<Vector3>();
        for (float x = 1.0f; x < size.x - 0.5f; x += 0.5f)
        {
            for (float z = 1.0f; z < size.z - 0.5f; z += 0.5f)
            {
                vacantSpaces.Add(location + new Vector3(x, vertOffset, z));
            }
        }
        // loops all items
        for (int i = 0; i < roomData.maxMinChanceList.Count; i++)
        {
            // check if item is spawned
            if (RandomR.Range(1, 100) < roomData.maxMinChanceList[i].z)
            {
                // check the number of times that item spawns
                int amount = RandomR.Range((int)roomData.maxMinChanceList[i].x, (int)roomData.maxMinChanceList[i].x);
                for (int j = 0; j < amount; j++)
                {
                    // find random space and places object
                    // removes that space from list of vacant spaces
                    if (vacantSpaces.Count > 0)
                    {
                        GameObject obj;
                        int randIndex = RandomR.Range(0, vacantSpaces.Count);
                        Vector3 randPos = vacantSpaces[randIndex];
                        vacantSpaces.RemoveAt(randIndex);
                        // check if obj is a weapon
                        if (roomData.ObjectsList[i].name.Contains("Weapon"))
                        {
                            int randWeapon = RandomR.Range(0, randomWeaponlist.Count);
                            obj = ObjectPoolManager.Instance.SpawnObject(randomWeaponlist[randWeapon], randPos + new Vector3(0, -vertOffset, 0), Quaternion.identity, ObjectPoolManager.PoolType.Map);
                        }
                        else
                        {
                            obj = ObjectPoolManager.Instance.SpawnObject(roomData.ObjectsList[i], randPos, Quaternion.identity, ObjectPoolManager.PoolType.Map);
                        }
                        mapContent.Add(obj);
                        obj.transform.eulerAngles = new Vector3(0, RandomR.Range(0.0f, 360.0f), 0);
                    }
                }
            }
        }
    }

    private void CheckEnemyRoomActivation()
    {
        if (enemyRooms.Count == 0)
        {
            return;
        }
        for (int i = 0; i < enemyRooms.Count; i++)
        {
            Collider[] colliders = Physics.OverlapBox(enemyRooms[i].bounds.center, new Vector3(enemyRooms[i].bounds.size.x / 2 - 0.25f, enemyRooms[i].bounds.size.y, enemyRooms[i].bounds.size.z / 2 - 0.25f));
            foreach (var col in colliders)
            {
                if (col.CompareTag("PlayerCollider"))
                {
                    int rand = RandomR.Range(0, 3);
                    string bgmToPlay = string.Empty;

                    switch (rand)
                    {
                        case 0:
                            bgmToPlay = "BGMDemonDance";
                            break;
                        case 1:
                            bgmToPlay = "BGMBeasTrap";
                            break;
                        case 2:
                            bgmToPlay = "BGMTorsionPrinciple";
                            break;
                    }

                    // BGM
                    AudioManager.Instance.StartCoroutine(AudioManager.Instance.PlayBGM(bgmToPlay));

                    // lock players in
                    colliders = Physics.OverlapBox(enemyRooms[i].bounds.center, new Vector3(enemyRooms[i].bounds.size.x / 2 + 0.5f, enemyRooms[i].bounds.size.y, enemyRooms[i].bounds.size.z / 2 + 0.5f));
                    foreach (var collider in colliders)
                    {
                        if (collider.CompareTag("Kick"))
                        {
                            collider.GetComponent<DoorTrigger>().ToggleDoor(true);
                        }
                    }
                    // place enemies
                    PlaceEnemies(enemyRooms[i].bounds.position, enemyRooms[i].bounds.center, enemyRooms[i].bounds.size, enemyRoomData[i]);
                    currEnemyRoom = enemyRooms[i];
                    enemyRooms.RemoveAt(i);
                    enemyRoomData.RemoveAt(i);
                }
            }
        }
    }

    private void PlaceEnemies(Vector3Int location, Vector3 center, Vector3Int size, RoomData data)
    {
        // set room type
        RoomData roomData = data;
        // create list of available spaces
        List<Vector3> vacantSpaces = new List<Vector3>();
        List<Vector3> allSpaces = new List<Vector3>();
        for (float x = 1.0f; x < size.x - 0.5f; x+= 0.25f)
        {
            for (float z = 1.0f; z < size.z - 0.5f; z += 0.25f)
            {
                vacantSpaces.Add(location + new Vector3(x, -0.2f, z));
            }
        }
        allSpaces.AddRange(vacantSpaces);
        // loops all items
        for (int i = 0; i < roomData.maxMinChanceList.Count; i++)
        {
            // check if item is spawned
            if (RandomR.Range(1, 100) < roomData.maxMinChanceList[i].z)
            {
                // check the number of times that item spawns
                int amount = RandomR.Range((int)roomData.maxMinChanceList[i].x, (int)roomData.maxMinChanceList[i].x);
                for (int j = 0; j < amount; j++)
                {
                    // find random space and places object
                    // removes that space from list of vacant spaces
                    if (vacantSpaces.Count > 0)
                    {
                        int randIndex = RandomR.Range(0, vacantSpaces.Count);
                        Vector3 randPos = vacantSpaces[randIndex];
                        GameObject obj = ObjectPoolManager.Instance.SpawnObject(roomData.ObjectsList[i], randPos, Quaternion.identity, ObjectPoolManager.PoolType.Map);
                        obj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        mapContent.Add(obj);
                        currEnemiesInRoom.Add(obj);

                        // randomize waypoint pos
                        for (int k = 0; k < obj.transform.childCount; k++)
                        {
                            randIndex = RandomR.Range(0, allSpaces.Count);
                            randPos = allSpaces[randIndex];
                            if (obj.transform.GetChild(k).CompareTag("Waypoint"))
                            {
                                obj.transform.GetChild(k).transform.position = randPos;
                            }
                        }
                    }
                }
            }
        }
    }

    private void PlaceRoom(Vector3Int location, Vector3Int size) 
    {
        GameObject obj;
        for (int j = 0; j < size.x; j++)
        {
            for (int k = 0; k < size.y; k++)
            {
                for (int l = 0; l < size.z; l++)
                {
                    Vector3 tileOffset = new Vector3(j + 0.5f, k - 0.5f, l + 0.5f);
                    // spawn floor
                    if (k == 0)
                    {
                        if (j == 0 && l == 0)
                        {
                            SpawnTileWithRotation(floorPrefab[1], location + tileOffset, 0);
                        }
                        else if (j == 0 && l == size.z - 1)
                        {
                            SpawnTileWithRotation(floorPrefab[1], location + tileOffset, 90);
                        }
                        else if (j == size.x - 1 && l == 0)
                        {
                            SpawnTileWithRotation(floorPrefab[1], location + tileOffset, -90);
                        }
                        else if (j == size.x - 1 && l == size.z - 1)
                        {
                            SpawnTileWithRotation(floorPrefab[1], location + tileOffset, 180);
                        }
                        else if (j == 0)
                        {
                            SpawnTileWithRotation(floorPrefab[2], location + tileOffset, 90);
                        }
                        else if (j == size.x - 1)
                        {
                            SpawnTileWithRotation(floorPrefab[2], location + tileOffset, -90);
                        }
                        else if (l == 0)
                        {
                            SpawnTileWithRotation(floorPrefab[2], location + tileOffset, 0);
                        }
                        else if (l == size.z - 1)
                        {
                            SpawnTileWithRotation(floorPrefab[2], location + tileOffset, 180);
                        }
                        else
                        {
                            obj = ObjectPoolManager.Instance.SpawnObject(floorPrefab[0], location + tileOffset, Quaternion.identity, ObjectPoolManager.PoolType.Map);
                            mapContent.Add(obj);
                        }
                    }
                    // spawn ceiling
                    else if (k == size.y - 1)
                    {
                        obj = ObjectPoolManager.Instance.SpawnObject(ceilingPrefab, location + tileOffset + new Vector3(0, -0.15f, 0), Quaternion.identity, ObjectPoolManager.PoolType.Map);
                        mapContent.Add(obj);
                    }
                    // spawn walls
                    if (k < size.y - 1)
                    {
                        if (j == 0)
                        {
                            SpawnTileWithRotation(wallPrefab[RandomR.Range(0, wallPrefab.Count)], location + tileOffset + new Vector3(-0.5f, 0, 0), 90);
                        }
                        else if (j == size.x - 1)
                        {
                            SpawnTileWithRotation(wallPrefab[RandomR.Range(0, wallPrefab.Count)], location + tileOffset + new Vector3(0.5f, 0, 0), -90);
                        }
                        if (l == 0)
                        {
                            SpawnTileWithRotation(wallPrefab[RandomR.Range(0, wallPrefab.Count)], location + tileOffset + new Vector3(0, 0, -0.5f), 0);
                        }
                        else if (l == size.z - 1)
                        {
                            SpawnTileWithRotation(wallPrefab[RandomR.Range(0, wallPrefab.Count)], location + tileOffset + new Vector3(0, 0, 0.5f), 180);
                        }
                    }
                }
            }
        }
    }

    public void RemoveEnemyFromRoom(GameObject enemy)
    {
        currEnemiesInRoom.Remove(enemy);
        StartCoroutine(ObjectPoolManager.Instance.ReturnObjectToPool(enemy));
        CheckAllEnemiedDead();
    }

    private void CheckAllEnemiedDead()
    {
        if (currEnemiesInRoom.Count <= 0)
        {
            int rand = RandomR.Range(0, 2);
            string bgmToPlay = string.Empty;

            switch (rand)
            {
                case 0:
                    bgmToPlay = "BGMSunsetSchool";
                    break;
                case 1:
                    bgmToPlay = "BGMDistorted";
                    break;
            }

            // BGM
            AudioManager.Instance.StartCoroutine(AudioManager.Instance.PlayBGM(bgmToPlay));

            // unlock doors
            Collider[] colliders = Physics.OverlapBox(currEnemyRoom.bounds.center, new Vector3(currEnemyRoom.bounds.size.x / 2 + 0.5f, currEnemyRoom.bounds.size.y, currEnemyRoom.bounds.size.z / 2 + 0.5f));
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Kick"))
                {
                    collider.GetComponent<DoorTrigger>().ToggleDoor(false);
                }
            }
            if (mRoomObjManager.endRoomData.name.Contains("Boss") && currEnemyRoom == EndRoom)
            {
                endObj.SetActive(true);
            }
            BuffManager.Instance.ShowBuffPanel();
            currEnemyRoom = null;
        }
    }

    private void PlaceHallway(Vector3Int curr, int pathIndex) {
        Vector3 tileOffset = new Vector3(0.5f, -1.5f, 0.5f);
        // spawn floor
        GameObject obj = ObjectPoolManager.Instance.SpawnObject(hallwayPrefab, curr + tileOffset, Quaternion.identity, ObjectPoolManager.PoolType.Map);
        mapContent.Add(obj);
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).gameObject.SetActive(true);
        }
        // spawn light in intervals
        if (pathIndex % 3 == 0)
        {
            obj = ObjectPoolManager.Instance.SpawnObject(hallwayLightPrefab, curr + tileOffset + new Vector3(0, 0.975f, 0), Quaternion.identity, ObjectPoolManager.PoolType.Map);
            mapContent.Add(obj);
        }
    }

    private GameObject SpawnTileWithRotation(GameObject go, Vector3 location, float angle)
    {
        GameObject obj = ObjectPoolManager.Instance.SpawnObject(go, location, Quaternion.identity, ObjectPoolManager.PoolType.Map);
        mapContent.Add(obj);
        obj.transform.eulerAngles = new Vector3(0, angle, 0);
        return obj;
    }
}
