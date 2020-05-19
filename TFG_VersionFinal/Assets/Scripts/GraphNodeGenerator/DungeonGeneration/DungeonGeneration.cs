﻿using System.Collections;
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


    public List<GameObject[]> prefabsList = new List<GameObject[]>();


    public void GenerateDungeon(string _fileName)
    {
       

        m_graphToLoad = Resources.Load<RoomInfoContainer>($"FinalGraphs/{_fileName}");
        LoadRoomsInfo();
    }


    void LoadRoomsInfo()
    {
        roomDictionary.Clear();


        GameObject l_parent = new GameObject();
        l_parent.name = "Dungeon";

        string l_startRoomID = m_graphToLoad.roomNodeData.Find(x => x.nodeType == "Start").nodeID;

        GameObject l_startRoom = SpawnRoom(l_startRoomID, l_parent.transform);
        roomDictionary.Add(l_startRoomID, l_startRoom);
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
         
            if(!roomDictionary.ContainsKey(l_nextRoomID))
            {
                l_nextRoom = SpawnRoom(l_nextRoomID, _parent);
                roomDictionary.Add(l_nextRoomID, l_nextRoom);
            }
            else
            {
               
                l_nextRoom = roomDictionary.First(x => x.Key == l_nextRoomID).Value;

            }


            //find connected gameObject by nodeID on dictionary
            GameObject connectedBaseRoom = roomDictionary.First(x => x.Key == _baseRoomID).Value;
            string baseConnectionName = listOfOutputConnections[i].basePortName;
            ConnectRooms(connectedBaseRoom, l_nextRoom, baseConnectionName);
        
            
        }//end for 
    }


    void ConnectRooms(GameObject _previousRoom, GameObject _currentRoomToSpawn, string baseConnection)
    {
        RoomScript baseRoom = _previousRoom.GetComponent<RoomScript>();
        RoomScript targetRoom = _currentRoomToSpawn.GetComponent<RoomScript>();
        Vector3 newPosition;


        switch (baseConnection)
        {
            case "up":
                baseRoom.GenerateDoor("up");
                targetRoom.GenerateDoor("down");
                newPosition = new Vector3(baseRoom.transform.position.x, baseRoom.transform.position.y, baseRoom.returnPosition("up").z - (baseRoom.returnRoomSize().z / 2));
                targetRoom.transform.position = newPosition;
                break;
            case "right":
                baseRoom.GenerateDoor("right");
                targetRoom.GenerateDoor("left");
                newPosition = new Vector3(baseRoom.returnPosition("right").x - (baseRoom.returnRoomSize().x / 2), baseRoom.transform.position.y, baseRoom.transform.position.z);
                targetRoom.transform.position = newPosition;
                break;

            case "down":
                baseRoom.GenerateDoor("down");
                targetRoom.GenerateDoor("up");
                newPosition = new Vector3(baseRoom.transform.position.x, baseRoom.transform.position.y, baseRoom.returnPosition("down").z + (baseRoom.returnRoomSize().z / 2));
                targetRoom.transform.position = newPosition;
                break;
            case "left":
                baseRoom.GenerateDoor("left");
                targetRoom.GenerateDoor("right");
                newPosition = new Vector3(baseRoom.returnPosition("left").x + (baseRoom.returnRoomSize().x / 2), baseRoom.transform.position.y, baseRoom.transform.position.z);
                targetRoom.transform.position = newPosition;
                break;
        }




    }

}//end of dung gen class
