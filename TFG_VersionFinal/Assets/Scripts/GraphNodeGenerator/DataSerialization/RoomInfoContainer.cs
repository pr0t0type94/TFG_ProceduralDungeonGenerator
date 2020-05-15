using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomInfoContainer : ScriptableObject 
{
    public List<RoomNodeConnectionsData> roomConnectionsData = new List<RoomNodeConnectionsData>();
    public List<RoomNodeData> roomNodeData = new List<RoomNodeData>();

}
