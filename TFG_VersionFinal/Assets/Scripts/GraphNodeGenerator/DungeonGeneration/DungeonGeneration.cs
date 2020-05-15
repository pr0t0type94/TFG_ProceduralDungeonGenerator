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

    public Dictionary<string, GameObject> roomDictionary = new Dictionary<string, GameObject>();




    public void GenerateDungeon(string _fileName)
    {
        m_graphToLoad = Resources.Load<RoomInfoContainer>($"FinalGraphs/{_fileName}");
        LoadRoomsInfo();
    }


    void LoadRoomsInfo()
    {
        GameObject l_parent = new GameObject();
        l_parent.name = "Dungeon";

        string l_startRoomID = m_graphToLoad.roomNodeData.Find(x => x.nodeType == "start").ToString();
        //var startRoomNeighbours = m_graphToLoad.roomConnectionsData.Where(x => x.baseNodeId == l_startRoomID).ToList();
        GameObject l_startRoom = SpawnRoom(l_startRoomID, l_parent.transform);
        roomDictionary.Add(l_startRoomID, l_startRoom);
        GenerateNeighbours(l_startRoomID, l_parent.transform);

        List<RoomNodeData> roomsToGenerate = m_graphToLoad.roomNodeData.Where(x => x.nodeType != "start").ToList();//always generate the start room 1st
      
        for (int i = 0; i < roomsToGenerate.Count; i++)
        {

            ///SpawnRoom(roomsToGenerate[i].nodeID, l_parent.transform);
            GenerateNeighbours(roomsToGenerate[i].nodeID, l_parent.transform);
            //generate doors
        }


        //////
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
            //find connected gameObject by nodeID on dictionary
            GameObject connectedBaseRoom = roomDictionary.First(x=>x.Key == _baseRoomID).Value;

            GameObject l_nextRoom = SpawnRoom(m_graphToLoad.roomNodeData.First(x=> x.nodeID == listOfOutputConnections[i].targetNodeId).nodeType, _parent);
            //List<RoomNodeConnectionsData> listOfConnections = m_graphToLoad.roomConnectionsData.Where(x => x.targetNodeId == _roomID).ToList();
            ConnectRooms(connectedBaseRoom, l_nextRoom);
        
            


        }



    }


    void ConnectRooms(GameObject _previousRoom, GameObject _currentRoomToSpawn)
    {
        RoomScript baseRoom = _previousRoom.GetComponent<RoomScript>();
        RoomScript targetRoom = _currentRoomToSpawn.GetComponent<RoomScript>();

        string connection = baseRoom.m_doorsPlaceHolders[0].name;

        switch (connection)
        {
            case "up":

                break;
        }
        _currentRoomToSpawn.transform.position = new Vector3();



    }

}//end of dung gen class
