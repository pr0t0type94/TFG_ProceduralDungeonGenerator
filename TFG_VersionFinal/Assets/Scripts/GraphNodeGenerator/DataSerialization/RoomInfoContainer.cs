using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomInfoContainer : ScriptableObject 
{
    public List<RoomNodeConnectionsData> m_roomConnectionsDataList = new List<RoomNodeConnectionsData>();
    public List<RoomNodeData> m_roomNodeDataList = new List<RoomNodeData>();

}
