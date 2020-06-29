using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DungeonGeneration : MonoBehaviour
{
    public string m_graphFileToGenerate;

    public List<GameObject> m_roomPrefabsToSpawn = new List<GameObject>();

    private RoomInfoContainer m_graphToLoad;

    public Dictionary<string, GameObject> m_roomDictionary = new Dictionary<string, GameObject>();

    private GameObject m_parent;

    public GameObject m_keyPrefab;

    [Tooltip("Al marcar esta opción, se cogerá un prefab aleatorio en el caso de que haya mas de 1 con el mismo nombre. Si no esta marcado, se cogerá el prefab que coincida exactamente con el nombre de la sala")]
    public bool m_selectRandomPrefabs = false;

    public void GenerateDungeon(string _fileName)
    {
        m_parent = new GameObject();
        m_parent.name = "Dungeon " + _fileName;
        m_graphToLoad = Resources.Load<RoomInfoContainer>($"FinalGraphs/{_fileName}");
        LoadRoomsInfo();
    }


    void LoadRoomsInfo()
    {
        m_roomDictionary.Clear();

        string l_startRoomID = m_graphToLoad.roomNodeData.Find(x => x.nodeType == "Start").nodeID;

        GameObject l_startRoom = SpawnRoom(l_startRoomID);
        m_roomDictionary.Add(l_startRoomID, l_startRoom);
        GenerateNeighbours(l_startRoomID);

        List<RoomNodeData> roomsToGenerate = m_graphToLoad.roomNodeData.Where(x => x.nodeType != "Start").ToList();//always generate the start room 1st

        for (int i = 0; i < roomsToGenerate.Count; i++)
        {
            GenerateNeighbours(roomsToGenerate[i].nodeID);
        }

    }

    GameObject SpawnRoom(string _roomID)
    {
        string l_roomType = m_graphToLoad.roomNodeData.Find(x => x.nodeID == _roomID).nodeType;
        GameObject nextRoomToSpawn;

        //si marcamos la opcion en el inspector, el prefab a instanciar se escojera al azar entre todos los que coincidan con el nombre(tipo de habitacion)
        if (m_selectRandomPrefabs)
        {
            List<GameObject> l_posibleRoomPrefabsList = m_roomPrefabsToSpawn.Where(x => x.name.Contains(l_roomType)).ToList();
            int randomPrefabSelection = Random.Range(0, l_posibleRoomPrefabsList.Count());
            nextRoomToSpawn = Instantiate(l_posibleRoomPrefabsList.ElementAt(randomPrefabSelection));
        }

        else
        {
            nextRoomToSpawn = Instantiate(m_roomPrefabsToSpawn.First(x => x.name == l_roomType));
           
        }


        nextRoomToSpawn.transform.parent = m_parent.transform;
        return nextRoomToSpawn;
    }


    void GenerateNeighbours(string _baseRoomID)
    {
        List<RoomNodeConnectionsData> listOfOutputConnections = m_graphToLoad.roomConnectionsData.Where(x => x.baseNodeId == _baseRoomID).ToList();


        for (int i = 0; i < listOfOutputConnections.Count; i++)
        {

            GameObject l_nextRoom;
            string l_nextRoomID = listOfOutputConnections[i].targetNodeId;

            if (!m_roomDictionary.ContainsKey(l_nextRoomID))
            {
                l_nextRoom = SpawnRoom(l_nextRoomID);
                m_roomDictionary.Add(l_nextRoomID, l_nextRoom);
            }
            else
            {
                l_nextRoom = m_roomDictionary.First(x => x.Key == l_nextRoomID).Value;
            }


            //find connected gameObject by nodeID on dictionary
            GameObject connectedBaseRoom = m_roomDictionary.First(x => x.Key == _baseRoomID).Value;
            string baseConnectionName = listOfOutputConnections[i].basePortName;
            string targetConnectionName = listOfOutputConnections[i].targetPortName;

            ConnectRooms(connectedBaseRoom, l_nextRoom, baseConnectionName, targetConnectionName);


        }//end for 
    }


    void ConnectRooms(GameObject _previousRoom, GameObject _currentRoomToSpawn, string _baseConnectionName, string _targetConnectionName)
    {
        RoomScript baseRoom = _previousRoom.GetComponent<RoomScript>();
        RoomScript targetRoom = _currentRoomToSpawn.GetComponent<RoomScript>();
        Vector3 newPosition = Vector3.zero;
        Vector3 baseRoomSize = baseRoom.ReturnRoomSize();


        baseRoom.GenerateDoor(_baseConnectionName);
        targetRoom.GenerateDoor(_targetConnectionName);

        if (!_baseConnectionName.Contains("stairs"))
        {

            if (_baseConnectionName.Contains("up"))
            {
                newPosition = new Vector3(baseRoom.transform.position.x, baseRoom.transform.position.y, baseRoom.returnPosition("up").z + (baseRoomSize.z / 2));
            }
            else if (_baseConnectionName.Contains("down"))
            {
                newPosition = new Vector3(baseRoom.transform.position.x, baseRoom.transform.position.y, baseRoom.returnPosition("down").z - (baseRoomSize.z / 2));
            }
            else if (_baseConnectionName.Contains("left"))
            {
                newPosition = new Vector3(baseRoom.returnPosition("left").x - (baseRoomSize.x / 2), baseRoom.transform.position.y, baseRoom.transform.position.z);
            }
            else if (_baseConnectionName.Contains("right"))
            {
                newPosition = new Vector3(baseRoom.returnPosition("right").x + (baseRoomSize.x / 2), baseRoom.transform.position.y, baseRoom.transform.position.z);
            }
        }
        else
        {
            //PROBLEMA AL HABER MAS DE 1 STAIR EN LA MISMA SALA
            StaircaseScript conectedStairs = baseRoom.GetComponentInChildren<StaircaseScript>();
            Vector3 positionToConnect = conectedStairs.m_endOfStaircaseGameobject.transform.position;


            if (_baseConnectionName.Contains("up"))
            {
                newPosition = new Vector3(positionToConnect.x, positionToConnect.y + (baseRoomSize.y / 3), positionToConnect.z + (baseRoomSize.z / 2));
            }
            else if (_baseConnectionName.Contains("down"))
            {
                newPosition = new Vector3(positionToConnect.x, positionToConnect.y + (baseRoomSize.y / 3), positionToConnect.z - (baseRoomSize.z / 2));
            }
            else if (_baseConnectionName.Contains("left"))
            {
                newPosition = new Vector3(positionToConnect.x - (baseRoomSize.x / 2), positionToConnect.y + (baseRoomSize.y / 3), positionToConnect.z);
            }
            else if (_baseConnectionName.Contains("right"))
            {
                newPosition = new Vector3(positionToConnect.x + (baseRoomSize.x / 2), positionToConnect.y + (baseRoomSize.y / 3), positionToConnect.z);
            }




        }

        targetRoom.transform.localPosition = newPosition;


        if(_baseConnectionName.Contains("key"))
        {
            GameObject keyPrefab = Instantiate(m_keyPrefab);
            keyPrefab.transform.parent = _previousRoom.transform;
            keyPrefab.transform.position = _previousRoom.transform.position;
        }

    }

}//end of dung gen class
