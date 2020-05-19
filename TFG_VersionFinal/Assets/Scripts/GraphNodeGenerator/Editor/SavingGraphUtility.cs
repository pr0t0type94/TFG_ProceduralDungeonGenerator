using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class SavingGraphUtility
{

    private GrammarGraphView m_targetGraph;
    private List<Edge> m_edgesList => m_targetGraph.edges.ToList();//inicializamos y asignamos la variable
    private List<RoomNode> m_roomsList => m_targetGraph.nodes.ToList().Cast<RoomNode>().ToList();//inicializamos y asignamos la variable y lo casteamos al tipo RoomNode

    private RoomInfoContainer m_graphToLoadCache;

    //creamos una instancia unica (singleton) de la clase
    public static SavingGraphUtility GetInstance(GrammarGraphView _targetGraph)
    {
        return new SavingGraphUtility { m_targetGraph = _targetGraph };
    }

    public void SaveGraph(string _fileName, bool isFinalGraph = false)
    {
        if (!m_roomsList.Any()) { Debug.Log("No nodes to save found"); return; } // si no hay rooms que guardar, no hacemos nada

        RoomInfoContainer l_nodeContainerData = ScriptableObject.CreateInstance<RoomInfoContainer>();

        Edge[] l_portsConnected = m_edgesList.Where(x => x.output.node != null).ToArray();

        for (int i = 0; i < l_portsConnected.Length; i++)//para cada puerto del nodo, guardamos 
        {
            RoomNode l_outRoom = l_portsConnected[i].output.node as RoomNode;
            RoomNode l_inputRoom = l_portsConnected[i].input.node as RoomNode;

            l_nodeContainerData.roomConnectionsData.Add(new RoomNodeConnectionsData
            {
                baseNodeId = l_outRoom.roomID,
                targetNodeId = l_inputRoom.roomID,

                basePortName = l_portsConnected[i].output.portName,
                targetPortName = l_portsConnected[i].input.portName,

            }
            );


        }
        foreach(RoomNode r in m_roomsList)//guardamos cada nodo, su id, el tipo, y la posicion,
        {
            l_nodeContainerData.roomNodeData.Add(new RoomNodeData
            {
                nodeID = r.roomID,
                nodeType = r.roomType,
                position = r.GetPosition().position,
                isTerminal = r.isTerminal
                
            });
        }

        if (!isFinalGraph)
            if(!m_roomsList.Exists(x => x.roomType == "Begin")) { Debug.Log("You need to create a Begin node to save a Rule"); return; }
            else AssetDatabase.CreateAsset(l_nodeContainerData, $"Assets/Resources/Rules/{_fileName}.asset");//generamos un asset con la informacion 

        else
            if (!m_roomsList.Exists(x => x.roomType == "Start")) { Debug.Log("You need to create a Start node to save a FinalGraph"); return; }
            else AssetDatabase.CreateAsset(l_nodeContainerData, $"Assets/Resources/FinalGraphs/{_fileName}.asset");//generamos un asset con la informacion 



        AssetDatabase.SaveAssets();

    }
    public void LoadGraph(string _fileName, bool isFinalGraph = false)
    {
        if(!isFinalGraph)
        m_graphToLoadCache = Resources.Load<RoomInfoContainer>($"Rules/{_fileName}");
        else
            m_graphToLoadCache = Resources.Load<RoomInfoContainer>($"FinalGraphs/{_fileName}");

        if (m_graphToLoadCache == null)
        {
            Debug.Log("Error, no existing Graph to load");
            return;
        }

        ClearCurrentGraph();
        CreateLoadedNodes();
        ConnectLoadedNodes();

    }

    private void ClearCurrentGraph()
    {
        foreach(Edge e in m_edgesList)
        {
            m_targetGraph.RemoveElement(e);
        }
        foreach (RoomNode r in m_roomsList)
        {
            m_targetGraph.RemoveElement(r);
        }

    }

    private void CreateLoadedNodes()
    {
        foreach(RoomNodeData rData in m_graphToLoadCache.roomNodeData)
        {
            RoomNode l_tempRoom = m_targetGraph.CreateRoomNode(rData.nodeType,rData.isTerminal);
            l_tempRoom.roomID = rData.nodeID;
            l_tempRoom.RefreshExpandedState();
            m_targetGraph.AddElement(l_tempRoom);

            List<RoomNodeConnectionsData> l_tempOutRoomPorts = m_graphToLoadCache.roomConnectionsData.Where(x => x.baseNodeId == rData.nodeID).ToList();
            l_tempOutRoomPorts.ForEach(x => m_targetGraph.GenerateOutputPortsOnNode(l_tempRoom, x.basePortName));

            List<RoomNodeConnectionsData> l_tempInputRoomPorts = m_graphToLoadCache.roomConnectionsData.Where(x => x.targetNodeId == rData.nodeID).ToList();
            l_tempInputRoomPorts.ForEach(x => m_targetGraph.GenerateInputPortsOnNode(l_tempRoom, x.targetPortName));
        }
    }

    private void ConnectLoadedNodes()
    {
        for (int i = 0; i < m_roomsList.Count; i++)
        {
            List<RoomNodeConnectionsData> l_connectionDataCache = m_graphToLoadCache.roomConnectionsData.Where(x => x.baseNodeId == m_roomsList[i].roomID).ToList();
            for (int j = 0; j < l_connectionDataCache.Count; j++)
            {
                string targetNodeId = l_connectionDataCache[j].targetNodeId;
                RoomNode targetRoom = m_roomsList.First(x => x.roomID == targetNodeId);
                string targetPortName = l_connectionDataCache[j].targetPortName;

                Port targetPort = m_targetGraph.ports.ToList().First(x => (x.node as RoomNode) == targetRoom && x.portName == targetPortName);

                //targetRoom.inputContainer.Q<Port>().portName = connectionDataCache[j].targetPortName;
                //LinkRoomPorts(m_roomsList[i].outputContainer[j].Q<Port>(), (Port)targetRoom.inputContainer[0]);
                LinkRoomPorts(m_roomsList[i].outputContainer[j].Q<Port>(), targetPort);

                targetRoom.SetPosition(new Rect(m_graphToLoadCache.roomNodeData.First(x => x.nodeID == targetRoom.roomID).position, m_targetGraph.defaultNodeSize));
            }
        }
    }

    private void LinkRoomPorts(Port _outPort, Port _inPort)
    {
        Edge tempEdge = new Edge()
        {
            output = _outPort,
            input = _inPort
        };
        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        m_targetGraph.Add(tempEdge);
    }




}//end of saving utility class
