using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

public class GrammarGraphWindow : EditorWindow 
{
    private GrammarGraphView m_graphView;
    private string m_ruleFileName = "New Grammar";
    private string m_finalGraphName = "Final Grammar";

    public RoomNode m_selectedRoomNode;

    public Edge m_currentBeginNodeEdge;
    [MenuItem("Grammar/Graph Editor")]
    public static void OpenGrammarGraphWindow()
    {
        //crear la ventana del editor y ponerle nombre
        GrammarGraphWindow window = GetWindow<GrammarGraphWindow>();
        window.titleContent = new GUIContent(text: "GrammarGraph");
    }

    private void OnEnable()//al abrir el editor debemos;
    {
        ConstructGraphView(); //generar la ventana
        GenerateToolbar(); // generar la barra de botones
    }

    private void OnDisable()//al cerrar el editor;
    {
        rootVisualElement.Remove(m_graphView);//eliminar la ventana 
    }
    private void ConstructGraphView()//generar la ventana del editor
    {
        m_graphView = new GrammarGraphView();//crear la variable ventana
        m_graphView.name = "GraphGrammarEditor";//asignar nombre a la ventana
        m_graphView.StretchToParentSize(); //ajustar el ancho y alto de la ventana
        rootVisualElement.Add(m_graphView); //añadimos la ventana al editor como elemento visual
    }

    private void GenerateToolbar()
    {
        Toolbar toolbar = new Toolbar();

        //lambda expression para no tner el cuenta el tipo; de createNode (void) a clickEvent (Event)
        Button nodeButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        nodeButton.text = "Create Room";//nombre del boton
        nodeButton.clickable.clicked += () => m_graphView.createNode("Room"); //evento de clickar -> llamamos a la funcion createNode del graphView
        toolbar.Add(nodeButton);//añadimos el boton a la toolbar



        //campo de texto con el nombre para guardar archivo
        TextField fileNameText = new TextField();//campo para guardar el nombre
        fileNameText.transform.position = new Vector3(60, 0, 0);
        fileNameText.SetValueWithoutNotify(m_ruleFileName);//guardar el valor por defecto
        fileNameText.MarkDirtyRepaint();//actualizar el nuevo valor en el siguiente frame
        fileNameText.RegisterValueChangedCallback(evt=>m_ruleFileName = evt.newValue );//callback para cambiar el nombre del archivo
        toolbar.Add(fileNameText);//añadimos la variable donde se cambia el nombre del archivo a la toolbar

        //poner el boton de guardar y cargar en la toolbar

        //toolbar.Add(new Button(() => SaveGrammarData()) { text = "Save Rule" });
        Button saveRuleButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        saveRuleButton.transform.position = new Vector3(60, 0, 0);
        saveRuleButton.clickable.clicked += () => SaveGrammarData(m_ruleFileName,false); //evento de clickar 
        saveRuleButton.text = "Save Rule";//nombre del boton
        toolbar.Add(saveRuleButton);//añadimos el boton a la toolbar

        //toolbar.Add(new Button(() => LoadGrammarData()) { text = "Load Rule" });
        Button loadRuleButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        loadRuleButton.transform.position = new Vector3(60, 0, 0);
        loadRuleButton.clickable.clicked += () => LoadGrammarData(m_ruleFileName,false); //evento de clickar 
        loadRuleButton.text = "Load Rule";//nombre del boton
        toolbar.Add(loadRuleButton);//añadimos el boton a la toolbar


        Button expandRoom = new Button(); //boton para 
        expandRoom.transform.position = new Vector3(130,0,0);
        expandRoom.clickable.clicked += () => ExpandNonTerminalNodes(); //evento de clickar 
        expandRoom.text = "Expand NonTerminal";//nombre del boton
        toolbar.Add(expandRoom);//añadimos el boton a la toolbar

        //campo de texto con el nombre para guardar archivo
        TextField finalGraphNameText = new TextField();//campo para guardar el nombre
        finalGraphNameText.transform.position = new Vector3(180, 0, 0);
        finalGraphNameText.SetValueWithoutNotify(m_finalGraphName);//guardar el valor por defecto
        finalGraphNameText.MarkDirtyRepaint();//actualizar el nuevo valor en el siguiente frame
        finalGraphNameText.RegisterValueChangedCallback(evt => m_finalGraphName = evt.newValue);//callback para cambiar el nombre del archivo
        toolbar.Add(finalGraphNameText);//añadimos la variable donde se cambia el nombre del archivo a la toolbar
                                        //toolbar.Add(new Button(() => LoadGrammarData()) { text = "Load Rule" });

        Button saveFinalGraph = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        saveFinalGraph.transform.position = new Vector3(180, 0, 0);
        finalGraphNameText.SetValueWithoutNotify(m_finalGraphName);//guardar el valor por defecto
        saveFinalGraph.clickable.clicked += () => SaveGrammarData(m_finalGraphName, true); //evento de clickar 
        saveFinalGraph.text = "Save Final Graph";//nombre del boton
        toolbar.Add(saveFinalGraph);//añadimos el boton a la toolbar

        Button loadFinalGraph = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        loadFinalGraph.transform.position = new Vector3(180, 0, 0);
        loadFinalGraph.clickable.clicked += () => LoadGrammarData(m_finalGraphName,true); //evento de clickar 
        loadFinalGraph.text = "Load Final Graph";//nombre del boton
        toolbar.Add(loadFinalGraph);//añadimos el boton a la toolbar

        rootVisualElement.Add(toolbar);//finalmente añadimos la toolbar a la ventana del editor
    }


    private void SaveGrammarData(string _saveFileName,bool isFinalGraph = false)
    {
        if (string.IsNullOrEmpty(_saveFileName))
        {
            Debug.Log("Invalid file name");
        }
        else
        {
            SavingGraphUtility saveManager = SavingGraphUtility.GetInstance(m_graphView);
            saveManager.SaveGraph(_saveFileName, isFinalGraph);
        }

    }
    private void LoadGrammarData(string _saveFileName,bool isFinalGraph = false)
    {
        if (string.IsNullOrEmpty(_saveFileName))
        {
            Debug.Log("Invalid file name");
        }

        SavingGraphUtility saveManager = SavingGraphUtility.GetInstance(m_graphView);
        saveManager.LoadGraph(_saveFileName, isFinalGraph);
    }


    //private void SaveFinalGraph()
    //{
    //    if (string.IsNullOrEmpty(m_finalGraphName))
    //    {
    //        Debug.Log("Invalid file name");
    //    }
    //    else
    //    {
    //        SavingGraphUtility saveManager = SavingGraphUtility.GetInstance(m_graphView);
    //        saveManager.SaveGraph(m_finalGraphName, true);
    //    }

    //}
    //private void LoadFinalGraph()
    //{
    //    if (string.IsNullOrEmpty(m_finalGraphName))
    //    {
    //        Debug.Log("Invalid file name");
    //    }
    //    else
    //    {
    //        SavingGraphUtility saveManager = SavingGraphUtility.GetInstance(m_graphView);
    //        saveManager.LoadRulesGraph(m_finalGraphName, true);
    //    }

    //}


    private void ExpandNonTerminalNodes()
    {

        //RoomNode l_roomToExpand = m_graphView.returnSelectedNode();
        //RoomInfoContainer expandedLoadedGraph = Resources.Load<RoomInfoContainer>($"Rules/{l_roomToExpand.roomType}");

        //m_graphView.RemoveElement(l_roomToExpand);

        foreach(RoomNode _currRoom in m_graphView.nodes.ToList())
        {
            if (!_currRoom.isTerminal)
            {

                RoomInfoContainer expandedLoadedGraph = Resources.Load<RoomInfoContainer>($"Rules/{_currRoom.roomType}");
                CreateExpandedNodes(expandedLoadedGraph, _currRoom);

                m_graphView.RemoveElement(_currRoom);
                foreach(Edge e in m_graphView.edges.ToList())
                {
                    if ((e.input.node as RoomNode) == _currRoom)
                    {
                        m_graphView.RemoveElement(e);                        
                    }
                }
            }
        }



    }



    private void CreateExpandedNodes(RoomInfoContainer _graphToLoadCache, RoomNode roomToExpand)
    {
       

        List<RoomNode> l_newInstantiatedRoomsList = new List<RoomNode>();

        foreach (RoomNodeData rData in _graphToLoadCache.roomNodeData)
        {
            RoomNode l_tempRoom = m_graphView.CreateRoomNode(rData.nodeType,rData.isTerminal);
            l_newInstantiatedRoomsList.Add(l_tempRoom);
            l_tempRoom.roomID = rData.nodeID;
            m_graphView.AddElement(l_tempRoom);

            List<RoomNodeConnectionsData> l_tempOutRoomPorts = _graphToLoadCache.roomConnectionsData.Where(x => x.baseNodeId == rData.nodeID).ToList();
            l_tempOutRoomPorts.ForEach(x => m_graphView.GenerateOutputPortsOnNode(l_tempRoom, x.basePortName));

            List<RoomNodeConnectionsData> l_inputOutRoomPorts = _graphToLoadCache.roomConnectionsData.Where(x => x.targetNodeId == rData.nodeID).ToList();
            l_inputOutRoomPorts.ForEach(x => m_graphView.GenerateInputPortsOnNode(l_tempRoom, x.targetPortName));
        }

        #region
        //string targetRoomID = _graphToLoadCache.roomConnectionsData.Find(x => x.baseNodeId == beginNodeInstantiated.roomID).targetNodeId.ToString();
        //Port targetPort = m_graphView.edges.ToList().Find(x => (x.input.node as RoomNode).roomID 
        //== targetRoomID).input;


        //string endNodeID = _graphToLoadCache.roomNodeData.Find(x => x.nodeType == "ending").nodeID;
        //RoomNode endNode = l_newInstantiatedRoomsList.Find(x => x.name == "ending");
        //beginNode.outputContainer.

        //LinkRoomPorts(edgesBaseConnected.output, beginConnectedRoom.input);
        //m_graphView.GenerateInputPortsOnNode(beginNodeInstantiated);
        #endregion

        for (int i = 0; i < l_newInstantiatedRoomsList.Count; i++)
        {
            List<RoomNodeConnectionsData> l_connectionDataCache = _graphToLoadCache.roomConnectionsData.Where(x => x.baseNodeId == l_newInstantiatedRoomsList[i].roomID).ToList();
           
            for (int j = 0; j < l_connectionDataCache.Count; j++)
            {
                string targetNodeId = l_connectionDataCache[j].targetNodeId;
                RoomNode targetRoom = l_newInstantiatedRoomsList.First(x => x.roomID == targetNodeId);
                //targetRoom.inputContainer.Q<Port>().portName = connectionDataCache[j].targetPortName;
                
               
                LinkRoomPorts(l_newInstantiatedRoomsList[i].outputContainer[j].Q<Port>(), (Port)targetRoom.inputContainer[0]);

                
                targetRoom.SetPosition(new Rect(_graphToLoadCache.roomNodeData.First(x => x.nodeID == targetRoom.roomID).position + 
                    roomToExpand.GetPosition().position, m_graphView.defaultNodeSize));
            }
        }//end of for loop
   
        Port basePort = m_graphView.edges.ToList().Find(x => (x.input.node as RoomNode) == roomToExpand).output;
        basePort.portColor = new Color(0, 0, 200);//blue


        RoomNode beginNodeInstantiated = l_newInstantiatedRoomsList.First(x => x.roomType == "begin");
        Port targetPort = m_currentBeginNodeEdge.input;
        targetPort.portColor = new Color(200, 0, 0);//red

        m_graphView.RemoveElement(beginNodeInstantiated);


        LinkRoomPorts(basePort, targetPort);
        //basePort.ConnectTo(targetPort);

        List<RoomNode> currentNodesOnGraph = m_graphView.nodes.ToList().Cast<RoomNode>().ToList();
     
       for (int i = 0; i < currentNodesOnGraph.Count; i++)
       {
            for (int j = 0; j < l_newInstantiatedRoomsList.Count(); j++)
            {
                if (currentNodesOnGraph[i] == l_newInstantiatedRoomsList[j])
                {

                    List<Edge> l_currentOutputRoomEdges = m_graphView.edges.ToList().Where(x => (x.output.node as RoomNode) == currentNodesOnGraph[i]).ToList();
                    List<Edge> l_currentInputRoomEdges = m_graphView.edges.ToList().Where(x => (x.input.node as RoomNode) == currentNodesOnGraph[i]).ToList();

                    string newRoomID = currentNodesOnGraph[i].roomID = System.Guid.NewGuid().ToString();

                    foreach (Edge e in l_currentOutputRoomEdges)
                    {
                        e.output.name = newRoomID;
                    }
                    foreach (Edge e in l_currentInputRoomEdges)
                    {
                        e.input.name = newRoomID;
                    }
                }
            }
       }//end of for loop
       
       
    }

    private void LinkRoomPorts(Port _outPort, Port _inPort)
    {
        Edge tempEdge = new Edge()
        {
            output = _outPort,
            input = _inPort
        };
        if ((_outPort.node as RoomNode).roomType == "begin")
            m_currentBeginNodeEdge = tempEdge;
        else
        {
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            m_graphView.Add(tempEdge);

        }
    }
}
