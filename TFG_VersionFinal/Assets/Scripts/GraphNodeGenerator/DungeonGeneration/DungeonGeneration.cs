using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System;


public class DungeonGeneration : MonoBehaviour
{
    public string m_graphFileToGenerate;

    public List<GameObject> m_roomPrefabsToSpawn = new List<GameObject>();

    private RoomInfoContainer m_graphToLoad;

    public Dictionary<string, GameObject> m_roomDictionary = new Dictionary<string, GameObject>();


    public List<GameObject[]> prefabsList = new List<GameObject[]>();


    public void GenerateDungeon(string _fileName)
    {
      
        m_graphToLoad = Resources.Load<RoomInfoContainer>($"FinalGraphs/{_fileName}");
        LoadRoomsInfo();
    }


    void LoadRoomsInfo()
    {
        m_roomDictionary.Clear();


        GameObject l_parent = new GameObject();
        l_parent.name = "Dungeon";

        string l_startRoomID = m_graphToLoad.roomNodeData.Find(x => x.nodeType == "Start").nodeID;

        GameObject l_startRoom = SpawnRoom(l_startRoomID, l_parent.transform);
        m_roomDictionary.Add(l_startRoomID, l_startRoom);
        GenerateNeighbours(l_startRoomID, l_parent.transform);

        List<RoomNodeData> roomsToGenerate = m_graphToLoad.roomNodeData.Where(x => x.nodeType != "Start").ToList();//always generate the start room 1st

        for (int i = 0; i < roomsToGenerate.Count; i++)
        {
            GenerateNeighbours(roomsToGenerate[i].nodeID, l_parent.transform);
        }
       
    }

    GameObject SpawnRoom(string _roomID, Transform _parent) //+string positionToSpawn
    {
        string l_roomType = m_graphToLoad.roomNodeData.Find(x => x.nodeID == _roomID).nodeType;
        GameObject nextRoomToSpawn = Instantiate(m_roomPrefabsToSpawn.Find(x => x.name == l_roomType));
        nextRoomToSpawn.transform.parent = _parent;
        return nextRoomToSpawn;
    }
    

    void GenerateNeighbours(string _baseRoomID, Transform _parent)
    {
        List<RoomNodeConnectionsData> listOfOutputConnections = m_graphToLoad.roomConnectionsData.Where(x => x.baseNodeId == _baseRoomID).ToList();


        for (int i = 0; i < listOfOutputConnections.Count; i++)
        {           

            GameObject l_nextRoom;
            string l_nextRoomID = listOfOutputConnections[i].targetNodeId;
         
            if(!m_roomDictionary.ContainsKey(l_nextRoomID))
            {
                l_nextRoom = SpawnRoom(l_nextRoomID, _parent);
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


        baseRoom.GenerateDoor(_baseConnectionName);
        targetRoom.GenerateDoor(_targetConnectionName);

        if(_baseConnectionName == "up" || _baseConnectionName == "up+key")
            newPosition = new Vector3(baseRoom.transform.position.x, baseRoom.transform.position.y, baseRoom.returnPosition("up").z + (baseRoom.returnRoomSize().z / 2));

        else if (_baseConnectionName == "down" || _baseConnectionName == "down+key")
            newPosition = new Vector3(baseRoom.transform.position.x, baseRoom.transform.position.y, baseRoom.returnPosition("down").z - (baseRoom.returnRoomSize().z / 2));

        else if (_baseConnectionName == "left" || _baseConnectionName == "left+key")
            newPosition = new Vector3(baseRoom.returnPosition("left").x - (baseRoom.returnRoomSize().x / 2), baseRoom.transform.position.y, baseRoom.transform.position.z);

        else if (_baseConnectionName == "right" || _baseConnectionName == "right+key")
            newPosition = new Vector3(baseRoom.returnPosition("right").x + (baseRoom.returnRoomSize().x / 2), baseRoom.transform.position.y, baseRoom.transform.position.z);


            targetRoom.transform.position = newPosition;


    }

}//end of dung gen class
