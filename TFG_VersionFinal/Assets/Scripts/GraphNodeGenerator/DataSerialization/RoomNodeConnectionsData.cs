using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]//utilizamos serializable para poder inspeccionar estos elementos desde el editor del engine
public class RoomNodeConnectionsData { 

    public string baseNodeId;
    public string basePortName;

    public string targetNodeId;
    public string targetPortName;
}
