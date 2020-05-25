using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System;

public class GrammarGraphWindow : EditorWindow 
{

    ///// VARIABLES
    private GrammarGraphView m_graphView;
    private string m_ruleFileName = "New Grammar";
    private string m_finalGraphName = "Final Grammar";
    private Edge m_currentBeginNodeEdge;
    private RoomNode beginNodeInstantiated;
    private List<RoomInfoContainer> m_resourcesRulesLoaded = new List<RoomInfoContainer>();

    List<RoomNode> m_currentInstantiatedRooms = new List<RoomNode>();

    private int m_numberOfRoomsToGenerate;
    private enum connectionTypes
    {
        up,
        down,
        left,
        right
        
    }

    /////FUNCIONES
    [MenuItem("Grammar/Graph Editor")]
    public static void OpenGrammarGraphWindow()
    {
        //crear la ventana del editor y ponerle nombre
        GrammarGraphWindow window = GetWindow<GrammarGraphWindow>();
        window.titleContent = new GUIContent(text: "GrammarGraph");
    }

    private void OnEnable()//al abrir el editor;
    {
        ConstructGraphView(); //generar la ventana
        GenerateToolbar(); // generar la barra de botones
        GenerateSecondaryToolbar();
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
    }//generar la barra de botones de la ventana

    private void GenerateSecondaryToolbar()
    {
        Toolbar l_secondaryToolbar = new Toolbar();


        Button generateRandomDungeon = new Button();
        generateRandomDungeon.text = "Generate Random Dungeon";
        generateRandomDungeon.clickable.clicked += () => GenerateRandomDungeon();
        l_secondaryToolbar.Add(generateRandomDungeon);

       

        //SliderInt maxNumberOfRooms = new SliderInt();
        //maxNumberOfRooms.tooltip = "Max Number of Rooms instantiated on the dungeon";
        //l_secondaryToolbar.Add(maxNumberOfRooms);

        IntegerField maxNumberOfRooms = new IntegerField();
        maxNumberOfRooms.tooltip = "Max Number of Rooms instantiated on the dungeon";
        maxNumberOfRooms.label = "Max number of rooms";
        maxNumberOfRooms.RegisterValueChangedCallback(evt => m_numberOfRoomsToGenerate = evt.newValue);
        //m_numberOfRoomsToGenerate = maxNumberOfRooms.value;
        l_secondaryToolbar.Add(maxNumberOfRooms);

        IntegerField maxNumberOfFloors = new IntegerField();
        maxNumberOfFloors.tooltip = "Max Number of Floors the dungeon has";
        maxNumberOfFloors.label = "Max number of floors";
        l_secondaryToolbar.Add(maxNumberOfFloors);


        Button clearAllNodesOnGraph = new Button();
        clearAllNodesOnGraph.text = "Clear All";
        clearAllNodesOnGraph.clickable.clicked += () => ClearAllNodesOnGraph();
        l_secondaryToolbar.Add(clearAllNodesOnGraph);

        rootVisualElement.Add(l_secondaryToolbar);
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

    }//guardar info del grafico
    private void LoadGrammarData(string _saveFileName,bool isFinalGraph = false)
    {
        if (string.IsNullOrEmpty(_saveFileName))
        {
            Debug.Log("Invalid file name");
        }

        SavingGraphUtility saveManager = SavingGraphUtility.GetInstance(m_graphView);
        saveManager.LoadGraph(_saveFileName, isFinalGraph);
    }//cargar info del grafico

    private void ExpandNonTerminalNodes()//expandir los nodos NO MARCADOS con isTerminal
    {
        foreach(RoomNode _currRoom in m_graphView.nodes.ToList())
        {
            if (!_currRoom.isTerminal)
            {
                m_resourcesRulesLoaded = Resources.LoadAll<RoomInfoContainer>("Rules").ToList();

                if(m_resourcesRulesLoaded.Exists(x=>x.name.Contains(_currRoom.roomType)))
                {
                    RoomInfoContainer[] m_candidateRules = m_resourcesRulesLoaded.Where(x => x.name.Contains(_currRoom.roomType)).ToArray();

                    int randomNumber = UnityEngine.Random.Range(0, m_candidateRules.Length-1);                   
                    RoomInfoContainer expandedLoadedGraph = m_candidateRules[randomNumber];                  

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

            List<RoomNodeConnectionsData> l_tempInputRoomPorts = _graphToLoadCache.roomConnectionsData.Where(x => x.targetNodeId == rData.nodeID).ToList();
            l_tempInputRoomPorts.ForEach(x => m_graphView.GenerateInputPortsOnNode(l_tempRoom, x.targetPortName));
        }

        for (int i = 0; i < l_newInstantiatedRoomsList.Count; i++)
        {
            List<RoomNodeConnectionsData> l_connectionDataCache = _graphToLoadCache.roomConnectionsData.Where(x => x.baseNodeId == l_newInstantiatedRoomsList[i].roomID).ToList();
           
            for (int j = 0; j < l_connectionDataCache.Count; j++)
            {
                string targetNodeId = l_connectionDataCache[j].targetNodeId;
                RoomNode targetRoom = l_newInstantiatedRoomsList.First(x => x.roomID == targetNodeId);
                //targetRoom.inputContainer.Q<Port>().portName = connectionDataCache[j].targetPortName;

                string targetPortName = l_connectionDataCache[j].targetPortName;

                Port targetPortToConnect = m_graphView.ports.ToList().First(x => (x.node as RoomNode) == targetRoom && x.portName == targetPortName);

                if(l_newInstantiatedRoomsList[i].roomType == "begin")
                beginNodeInstantiated = l_newInstantiatedRoomsList[i];

                LinkRoomPorts(l_newInstantiatedRoomsList[i].outputContainer[j].Q<Port>(), targetPortToConnect);

                targetRoom.SetPosition(new Rect(_graphToLoadCache.roomNodeData.First(x => x.nodeID == targetRoom.roomID).position + 
                    roomToExpand.GetPosition().position, m_graphView.defaultNodeSize));
            }
        }//end of for loop
   
        Port basePort = m_graphView.edges.ToList().Find(x => (x.input.node as RoomNode) == roomToExpand).output;
        basePort.portColor = new Color(0, 0, 200);//blue


        Port targetPort = m_currentBeginNodeEdge.input;
        targetPort.portColor = new Color(200, 0, 0);//red

        m_graphView.RemoveElement(beginNodeInstantiated);


        LinkRoomPorts(basePort, targetPort);
        //basePort.ConnectTo(targetPort);

        List<RoomNode> l_currentNodesOnGraph = m_graphView.nodes.ToList().Cast<RoomNode>().ToList();
     
       for (int i = 0; i < l_currentNodesOnGraph.Count; i++)
       {
            for (int j = 0; j < l_newInstantiatedRoomsList.Count(); j++)
            {
                if (l_currentNodesOnGraph[i] == l_newInstantiatedRoomsList[j])
                {

                    List<Edge> l_currentOutputRoomEdges = m_graphView.edges.ToList().Where(x => (x.output.node as RoomNode) == l_currentNodesOnGraph[i]).ToList();
                    List<Edge> l_currentInputRoomEdges = m_graphView.edges.ToList().Where(x => (x.input.node as RoomNode) == l_currentNodesOnGraph[i]).ToList();

                    string newRoomID = l_currentNodesOnGraph[i].roomID = System.Guid.NewGuid().ToString();

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

        if ((_outPort.node as RoomNode) == beginNodeInstantiated)
        {
            m_currentBeginNodeEdge = tempEdge;
        }
        else
        {
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            m_graphView.Add(tempEdge);

        }
    }

    private void GenerateRandomDungeon()
    {

        ClearAllNodesOnGraph();
        m_currentInstantiatedRooms.Clear();

        if (m_graphView.nodes.ToList().Count > 0)
        {
            //clear current nodes and conections
        }
        Debug.Log(m_numberOfRoomsToGenerate);



        RoomNode startNode = m_graphView.CreateRoomNode("Start",true);
        m_graphView.AddElement(startNode);
        m_currentInstantiatedRooms.Add(startNode);
        GenereteNeighbourRooms(startNode);


        int newRandomValue = UnityEngine.Random.Range(0,10);

        //for MAX ROOM NUMBER
        

        //List<RoomNode> l_currentInstantiatedRooms = m_graphView.nodes.ToList().Cast<RoomNode>().ToList();





    }

    private void GenereteNeighbourRooms(RoomNode _baseRoom)
    {

        ConectionsDictionary newDictionaryReference = new ConectionsDictionary();


        int numberOfConnections;

        if (_baseRoom.roomType == "Start") numberOfConnections = UnityEngine.Random.Range(1, 5);
        else numberOfConnections = UnityEngine.Random.Range(0, 5);

        int randomConnectionName = UnityEngine.Random.Range(0, 10);

        List<string> currentAddedNames = new List<string>();

        for (int i = 0; i < numberOfConnections; i++)
        {

            int randomTerminalProbability = UnityEngine.Random.Range(0, 2); //50% chance

            RoomNode l_newRoomToSpawn = m_graphView.CreateRoomNode("random TYPE");
            //l_newRoomToSpawn.isTerminal = randomTerminalProbability > 0 ? true : false;
            m_graphView.AddElement(l_newRoomToSpawn);
            m_currentInstantiatedRooms.Add(l_newRoomToSpawn);

            //select a random name for base port
            int numberOfDifferentConnections = ConectionsDictionary.GetInstance().myConnectionsDictionary.Count();

            int randomSelection = UnityEngine.Random.Range(0, numberOfDifferentConnections);
            string selectedNameOfBasePort = ConectionsDictionary.GetInstance().myConnectionsDictionary.ElementAt(randomSelection).Key;

            while(currentAddedNames.Contains(selectedNameOfBasePort))
            {
                randomSelection = UnityEngine.Random.Range(0, numberOfDifferentConnections);
                selectedNameOfBasePort = ConectionsDictionary.GetInstance().myConnectionsDictionary.ElementAt(randomSelection).Key;
            }



            currentAddedNames.Add(selectedNameOfBasePort);

            string nameOfTargetPort = ConectionsDictionary.GetInstance().returnConnectionNameReferences(selectedNameOfBasePort);



            //generate and connect ports
            Port l_basePort = m_graphView.GenerateOutputPortsOnNode(_baseRoom, selectedNameOfBasePort);
            Port l_targetPort = m_graphView.GenerateInputPortsOnNode(l_newRoomToSpawn, ConectionsDictionary.GetInstance().returnConnectionNameReferences(selectedNameOfBasePort));          
            LinkRoomPorts(l_basePort, l_targetPort);

            if (l_basePort.portName.Contains("up"))
            {        

                l_newRoomToSpawn.SetPosition(new Rect(_baseRoom.GetPosition().position - new Vector2(0, 150), m_graphView.defaultNodeSize));

            }
            if (l_basePort.portName.Contains("down"))
            {
                l_newRoomToSpawn.SetPosition(new Rect(_baseRoom.GetPosition().position + new Vector2(0, 200), m_graphView.defaultNodeSize));

            }
            if (l_basePort.portName.Contains("left"))
            {
                l_newRoomToSpawn.SetPosition(new Rect(_baseRoom.GetPosition().position + new Vector2(400,0), m_graphView.defaultNodeSize));

            }
            if (l_basePort.portName.Contains("right"))
            {
                l_newRoomToSpawn.SetPosition(new Rect(_baseRoom.GetPosition().position - new Vector2(400, 0), m_graphView.defaultNodeSize));

            }

            //if(l_newRoomToSpawn.isTerminal == true) { ExpandNonTerminalNodes(); }


        }

    }


    private void ClearAllNodesOnGraph()
    {


        foreach(Edge e in m_graphView.edges.ToList())
        {
            m_graphView.RemoveElement(e);

        }

        foreach (Node n in m_graphView.nodes.ToList())
        {
            m_graphView.RemoveElement(n);
        }
    }
}
