﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


public class DungeonGeneration : MonoBehaviour
{
    public string m_dungeonNameToGenerate;

    public List<GameObject> m_roomPrefabsToSpawn = new List<GameObject>();

    private RoomInfoContainer m_graphToLoad;

    public Dictionary<string, GameObject> m_roomDictionary = new Dictionary<string, GameObject>();

    private GameObject m_parent;

    public GameObject m_playerPrefab;
    public GameObject m_keyPrefab;
    //public GameObject m_doorPrefab;
    //public GameObject m_staircasePrefab;

    [Tooltip("Al marcar esta opción, se cogerá un prefab aleatorio en el caso de que haya mas de 1 con el mismo nombre. Si no esta marcado, se cogerá el primer prefab que encuentre con el nombre de la sala")]
    public bool m_selectRandomPrefabs = false;

    [Range(0.01f,0.1f)]
    public float m_offsetRoomPositions = 0.05f;


    public void GenerateDungeon(string _fileName)
    {
        
        if(!AssetDatabase.FindAssets($"{_fileName}",new[] {"Assets/Resources/FinalGraphs"}).Any())
        {
            Debug.Log($"Caution! There is no graph with the name '{_fileName}' on the Resources/FinalGraphs folder");
            return;
        }

        m_parent = new GameObject();
        m_parent.name = "Dungeon " + _fileName;
        m_graphToLoad = Resources.Load<RoomInfoContainer>($"FinalGraphs/{_fileName}");
        LoadRoomsInfo();
    }

    void LoadRoomsInfo()
    {
        m_roomDictionary.Clear();

        string l_startRoomID = m_graphToLoad.m_roomNodeDataList.Find(x => x.nodeType == "Start").nodeID;

        GameObject l_startRoom = SpawnRoom(l_startRoomID);
        GameObject l_player = Instantiate(m_playerPrefab);
        l_player.transform.position = l_startRoom.transform.position;
        l_player.transform.parent = m_parent.transform;

        m_roomDictionary.Add(l_startRoomID, l_startRoom);
        GenerateNeighbourRoomsOnScene(l_startRoomID);

        List<RoomNodeData> roomsToGenerate = m_graphToLoad.m_roomNodeDataList.Where(x => x.nodeType != "Start").ToList();//always generate the start room 1st

        for (int i = 0; i < roomsToGenerate.Count; i++)
        {
            GenerateNeighbourRoomsOnScene(roomsToGenerate[i].nodeID);
        }

    }

    GameObject SpawnRoom(string _roomID)
    {
        string l_roomType = m_graphToLoad.m_roomNodeDataList.Find(x => x.nodeID == _roomID).nodeType;
        GameObject nextRoomToSpawn = null;

        if (m_roomPrefabsToSpawn.Exists(x => x.name.Contains(l_roomType)))

        {

            //si marcamos la opcion en el inspector, el prefab a instanciar se escojera al azar entre todos los que coincidan con el nombre(tipo de habitacion)
            if (m_selectRandomPrefabs)
            {
                List<GameObject> l_posibleRoomPrefabsList = m_roomPrefabsToSpawn.Where(x => x.name.Contains(l_roomType)).ToList();
                int randomPrefabSelection = Random.Range(0, l_posibleRoomPrefabsList.Count());
                nextRoomToSpawn = Instantiate(l_posibleRoomPrefabsList.ElementAt(randomPrefabSelection));
            }

            else
            {
                nextRoomToSpawn = Instantiate(m_roomPrefabsToSpawn.First(x => x.name.Contains(l_roomType)));

            }

            nextRoomToSpawn.transform.parent = m_parent.transform;


        }
        else
        {
            Debug.Log($"There is no existing room of type {l_roomType}");
        }


        return nextRoomToSpawn;
    }


    void GenerateNeighbourRoomsOnScene(string _baseRoomID)
    {
        List<RoomNodeConnectionsData> listOfOutputConnections = m_graphToLoad.m_roomConnectionsDataList.Where(x => x.baseNodeId == _baseRoomID).ToList();


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


    void ConnectRooms(GameObject _previousRoom, GameObject _currentSpawnedRoom, string _baseConnectionName, string _targetConnectionName)
    {
        RoomScript l_baseRoom = _previousRoom.GetComponent<RoomScript>();
        RoomScript l_targetRoom = _currentSpawnedRoom.GetComponent<RoomScript>();
        Vector3 l_newPosition = Vector3.zero;
        Vector3 l_baseRoomSize = l_baseRoom.ReturnRoomSize();

        l_baseRoom.GenerateConnectionsOnRoom(_baseConnectionName);
        l_targetRoom.GenerateConnectionsOnRoom(_targetConnectionName);

        if (!_baseConnectionName.Contains("stairs"))
        {

            if (_baseConnectionName.Contains("up"))
            {
                l_newPosition = new Vector3(l_baseRoom.transform.position.x, l_baseRoom.transform.position.y, l_baseRoom.returnPosition("up").z + (l_baseRoomSize.z / 2) + m_offsetRoomPositions);
            }
            else if (_baseConnectionName.Contains("down"))
            {
                l_newPosition = new Vector3(l_baseRoom.transform.position.x, l_baseRoom.transform.position.y, l_baseRoom.returnPosition("down").z - (l_baseRoomSize.z / 2) - m_offsetRoomPositions);
            }
            else if (_baseConnectionName.Contains("left"))
            {
                l_newPosition = new Vector3(l_baseRoom.returnPosition("left").x - (l_baseRoomSize.x / 2) - m_offsetRoomPositions, l_baseRoom.transform.position.y, l_baseRoom.transform.position.z);
            }
            else if (_baseConnectionName.Contains("right"))
            {
                l_newPosition = new Vector3(l_baseRoom.returnPosition("right").x + (l_baseRoomSize.x / 2) + m_offsetRoomPositions, l_baseRoom.transform.position.y, l_baseRoom.transform.position.z);
            }
        }
        else
        {
            //PROBLEMA AL HABER MAS DE 1 STAIR EN LA MISMA SALA
            StaircaseScript conectedStairs = l_baseRoom.GetComponentInChildren<StaircaseScript>();
            Vector3 positionToConnect = conectedStairs.m_endOfStaircaseGameobject.transform.position;


            if (_baseConnectionName.Contains("up"))
            {
                l_newPosition = new Vector3(positionToConnect.x, positionToConnect.y + (l_baseRoomSize.y / 3), positionToConnect.z + (l_baseRoomSize.z / 2));
            }
            else if (_baseConnectionName.Contains("down"))
            {
                l_newPosition = new Vector3(positionToConnect.x, positionToConnect.y + (l_baseRoomSize.y / 3), positionToConnect.z - (l_baseRoomSize.z / 2));
            }
            else if (_baseConnectionName.Contains("left"))
            {
                l_newPosition = new Vector3(positionToConnect.x - (l_baseRoomSize.x / 2), positionToConnect.y + (l_baseRoomSize.y / 3), positionToConnect.z);
            }
            else if (_baseConnectionName.Contains("right"))
            {
                l_newPosition = new Vector3(positionToConnect.x + (l_baseRoomSize.x / 2), positionToConnect.y + (l_baseRoomSize.y / 3), positionToConnect.z);
            }




        }

        l_targetRoom.transform.localPosition = l_newPosition;


        if (_baseConnectionName.Contains("key"))
        {
            GameObject keyPrefab = Instantiate(m_keyPrefab);
            keyPrefab.transform.parent = _previousRoom.transform;
            keyPrefab.transform.position = _previousRoom.transform.position;
        }

    }

}//end of dung gen class
