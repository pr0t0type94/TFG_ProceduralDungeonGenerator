using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

public class GrammarGraphWindow : EditorWindow
{

    ///// VARIABLES
    private GrammarGraphView m_graphView;

    private int m_numberOfRoomsToGenerate;
    private int m_numberOfFloorsToGenerate;
    int m_numberOfRoomsInstantiated;

    private string m_ruleFileName = "New Grammar";
    private string m_finalGraphName = "Final Grammar";

    private Edge m_currentBeginNodeEdge;//for expand non terminal function
    private RoomNode m_beginNodeInstantiated;//for expand non term. function
    private List<RoomInfoContainer> m_resourcesRulesLoaded = new List<RoomInfoContainer>();//for expand non terminal function

    List<RoomNode> m_currentlyInstantiatedRoomList = new List<RoomNode>();//lista con los nuevos nodos instanciados (generar random dungeon function)

    UtilitiesAndReferencesClass m_UtilitiesInstance;//para cojer lista de conexiones y tipos de habitacion posible

    Dictionary<RoomNode, string> m_roomAndItsInputsDictionary = new Dictionary<RoomNode, string>();//dictionary con la room spawneada y el tipo de input 

    List<Edge> m_edgesList = new List<Edge>();


    [MenuItem("Grammar Generator/Graph Editor")]
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
        GenerateSecondaryToolbar(); //barra para la generacion random
        m_UtilitiesInstance = UtilitiesAndReferencesClass.GetInstance();//clase con utilidades, es un singleton

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
        Toolbar l_toolbar = new Toolbar();

        Button l_nodeButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        l_nodeButton.text = "Create Room";//nombre del boton
        l_nodeButton.clickable.clicked += () => m_graphView.createNode("Room"); //evento de clickar -> llamamos a la funcion createNode del graphView
        l_toolbar.Add(l_nodeButton);//añadimos el boton a la toolbar

        //campo de texto con el nombre para guardar archivo
        TextField l_fileNameText = new TextField();//campo para guardar el nombre
        l_fileNameText.transform.position = new Vector3(60, 0, 0);
        l_fileNameText.SetValueWithoutNotify(m_ruleFileName);//guardar el valor por defecto
        l_fileNameText.MarkDirtyRepaint();//actualizar el nuevo valor en el siguiente frame
        l_fileNameText.RegisterValueChangedCallback(evt => m_ruleFileName = evt.newValue);//callback para cambiar el nombre del archivo
        l_toolbar.Add(l_fileNameText);//añadimos la variable donde se cambia el nombre del archivo a la toolbar


        Button l_saveRuleButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        l_saveRuleButton.transform.position = new Vector3(60, 0, 0);
        l_saveRuleButton.clickable.clicked += () => SaveGrammarData(m_ruleFileName, false); //evento al clickar 
        l_saveRuleButton.text = "Save Rule";//nombre del boton
        l_toolbar.Add(l_saveRuleButton);//añadimos el boton a la toolbar

        Button l_loadRuleButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        l_loadRuleButton.transform.position = new Vector3(60, 0, 0);
        l_loadRuleButton.clickable.clicked += () => LoadGrammarData(m_ruleFileName, false); //evento al clickar 
        l_loadRuleButton.text = "Load Rule";//nombre del boton
        l_toolbar.Add(l_loadRuleButton);//añadimos el boton a la toolbar


        Button l_expandRoomButton = new Button(); //boton para 
        l_expandRoomButton.transform.position = new Vector3(130, 0, 0);
        l_expandRoomButton.clickable.clicked += () => ExpandNonTerminalNodes(); //evento al clickar 
        l_expandRoomButton.text = "Expand NonTerminal";//nombre del boton
        l_toolbar.Add(l_expandRoomButton);//añadimos el boton a la toolbar

        //campo de texto con el nombre para guardar el grafico final
        TextField l_finalGraphNameText = new TextField();//campo para guardar el nombre
        l_finalGraphNameText.transform.position = new Vector3(180, 0, 0);
        l_finalGraphNameText.SetValueWithoutNotify(m_finalGraphName);//guardar el valor por defecto
        l_finalGraphNameText.MarkDirtyRepaint();//actualizar el nuevo valor en el siguiente frame
        l_finalGraphNameText.RegisterValueChangedCallback(evt => m_finalGraphName = evt.newValue);//callback para cambiar el nombre del archivo
        l_toolbar.Add(l_finalGraphNameText);//añadimos la variable donde se cambia el nombre del archivo a la toolbar


        Button l_saveFinalGraphButton = new Button();
        l_saveFinalGraphButton.transform.position = new Vector3(180, 0, 0);
        l_finalGraphNameText.SetValueWithoutNotify(m_finalGraphName);//guardar el valor por defecto
        l_saveFinalGraphButton.clickable.clicked += () => SaveGrammarData(m_finalGraphName, true); //evento al clickar 
        l_saveFinalGraphButton.text = "Save Final Graph";//nombre del boton
        l_toolbar.Add(l_saveFinalGraphButton);//añadimos el boton a la toolbar

        Button l_loadFinalGraphButton = new Button();
        l_loadFinalGraphButton.transform.position = new Vector3(180, 0, 0);
        l_loadFinalGraphButton.clickable.clicked += () => LoadGrammarData(m_finalGraphName, true); //evento al clickar 
        l_loadFinalGraphButton.text = "Load Final Graph";//nombre del boton
        l_toolbar.Add(l_loadFinalGraphButton);//añadimos el boton a la toolbar

        rootVisualElement.Add(l_toolbar);//finalmente añadimos la toolbar a la ventana del editor
    }//generar la barra de botones de la ventana del editor

    private void GenerateSecondaryToolbar()
    {
        Toolbar l_secondaryToolbar = new Toolbar();

        Button l_generateRandomDungeonButton = new Button();
        l_generateRandomDungeonButton.text = "Generate Random Dungeon";
        l_generateRandomDungeonButton.clickable.clicked += () => GenerateRandomDungeon();
        l_secondaryToolbar.Add(l_generateRandomDungeonButton);

        IntegerField l_maxNumberOfRoomsIntField = new IntegerField();
        l_maxNumberOfRoomsIntField.tooltip = "Max Number of Rooms instantiated on the dungeon";
        l_maxNumberOfRoomsIntField.label = "Max number of rooms";
        l_maxNumberOfRoomsIntField.RegisterValueChangedCallback(evt => m_numberOfRoomsToGenerate = evt.newValue);
        l_secondaryToolbar.Add(l_maxNumberOfRoomsIntField);

        IntegerField l_maxNumberOfFloorsField = new IntegerField();
        l_maxNumberOfFloorsField.tooltip = "Max Number of floors on the dungeon";
        l_maxNumberOfFloorsField.label = "Max number of floors";
        l_maxNumberOfFloorsField.RegisterValueChangedCallback(evt => m_numberOfFloorsToGenerate = evt.newValue);
        l_secondaryToolbar.Add(l_maxNumberOfFloorsField);

        Button l_clearAllNodesOnGrapButton = new Button();
        l_clearAllNodesOnGrapButton.text = "Clear All";
        l_clearAllNodesOnGrapButton.clickable.clicked += () => ClearAllNodesOnGraph();
        l_secondaryToolbar.Add(l_clearAllNodesOnGrapButton);

        rootVisualElement.Add(l_secondaryToolbar);
    }//barra generacion aleatoria


    private void SaveGrammarData(string _saveFileName, bool _isFinalGraph = false)
    {
        if (string.IsNullOrEmpty(_saveFileName))
        {
            Debug.Log("Invalid file name");
        }
        else
        {
            SavingGraphUtility saveManager = SavingGraphUtility.GetInstance(m_graphView);
            saveManager.SaveGraph(_saveFileName, _isFinalGraph);
        }

    }//guardar info del grafico
    private void LoadGrammarData(string _saveFileName, bool _isFinalGraph = false)
    {
        if (string.IsNullOrEmpty(_saveFileName))
        {
            Debug.Log("Invalid file name");
        }

        SavingGraphUtility saveManager = SavingGraphUtility.GetInstance(m_graphView);
        saveManager.LoadGraph(_saveFileName, _isFinalGraph);
    }//cargar info del grafico

    private void ExpandNonTerminalNodes()//expandir los nodos NO MARCADOS con isTerminal -> nodos Non-Terminal
    {
        if (m_graphView.nodes.ToList().Cast<RoomNode>().Where(x => x.isTerminal == false).Count() > 0)
        {
            foreach (RoomNode rn in m_graphView.nodes.ToList().Cast<RoomNode>().Where(x => x.isTerminal == false))
            {
                if (rn.outputContainer.Q<Port>() == null)
                {

                    m_resourcesRulesLoaded = Resources.LoadAll<RoomInfoContainer>("Rules").ToList();//cargo todas las reglas disponibles

                    if (m_resourcesRulesLoaded.Exists(x => x.name.Contains(rn.roomType)))//si existe una regla que contenga el nombre del nodo que quiero expandir;
                    {
                        RoomInfoContainer[] l_candidateRules = m_resourcesRulesLoaded.Where(x => x.name.Contains(rn.roomType)).ToArray();//cojo las reglas que coincidan

                        int l_randomRuleSelection = Random.Range(0, l_candidateRules.Length);//escojo una regla al azar
                        RoomInfoContainer l_ruleToLoad = l_candidateRules[l_randomRuleSelection];

                        CreateExpandedNodes(l_ruleToLoad, rn);//expando el nodo dependiendo de la regla
                        m_graphView.RemoveElement(rn);//elimino el nodo expandido para dejar sitio a los nuevos

                        //elimino los edges que pertenezcan al nodo expandido
                        foreach (Edge e in m_graphView.edges.ToList())
                        {
                            if ((e.input.node as RoomNode) == rn)
                            {
                                m_graphView.RemoveElement(e);
                            }
                        }

                    }
                    else
                    {
                        Debug.Log($"The node '{rn.roomType}' you'r trying to expand doesn't have a rule specified");
                    }

                }
                else
                {
                    Debug.Log("Non-terminal nodes must have 0 outputs (should be placed at the end of the graph)");

                }
            }
        }
        else
        {
            Debug.Log("0 Nodes to Expand. Only non-terminal nodes can be expanded. You should dismark 'isTerminal' from any node to expand it");
        }





    }
    private void CreateExpandedNodes(RoomInfoContainer _graphToLoadCache, RoomNode roomToExpand)
    {
        List<RoomNode> l_newInstantiatedRoomsList = new List<RoomNode>();

        foreach (RoomNodeData rData in _graphToLoadCache.roomNodeData) //para cada nodo en el nuevo grafico (la regla que acabo de cargar);
        {
            RoomNode l_tempRoom = m_graphView.CreateRoomNode(rData.nodeType, rData.isTerminal);//instancio el nodo
            l_newInstantiatedRoomsList.Add(l_tempRoom);//lo añado a la lista de "nuevas salas"
            l_tempRoom.roomID = rData.nodeID;//le asigno su ID
            m_graphView.AddElement(l_tempRoom);//añado el elemento al grafico

            //creo los puertos de entrada y salida que le tocan al nodo
            List<RoomNodeConnectionsData> l_tempOutRoomPorts = _graphToLoadCache.roomConnectionsData.Where(x => x.baseNodeId == rData.nodeID).ToList();
            l_tempOutRoomPorts.ForEach(x => m_graphView.GenerateOutputPortsOnNode(l_tempRoom, x.basePortName));

            List<RoomNodeConnectionsData> l_tempInputRoomPorts = _graphToLoadCache.roomConnectionsData.Where(x => x.targetNodeId == rData.nodeID).ToList();
            l_tempInputRoomPorts.ForEach(x => m_graphView.GenerateInputPortsOnNode(l_tempRoom, x.targetPortName));
        }

        for (int i = 0; i < l_newInstantiatedRoomsList.Count; i++)// para cada nodo que acabo de instanciar
        {
            //busco las conexiones que tiene
            List<RoomNodeConnectionsData> l_connectionDataCache = _graphToLoadCache.roomConnectionsData.Where(x => x.baseNodeId == l_newInstantiatedRoomsList[i].roomID).ToList();

            for (int j = 0; j < l_connectionDataCache.Count; j++)
            {
                string l_targetNodeId = l_connectionDataCache[j].targetNodeId;//guardo el id del nodo target
                RoomNode l_targetRoom = l_newInstantiatedRoomsList.First(x => x.roomID == l_targetNodeId);//busco la sala que conicida con ese ID

                string l_targetPortName = l_connectionDataCache[j].targetPortName;//guardo el nombre del puerto target

                Port l_targetPortToConnect = m_graphView.ports.ToList().First(x => (x.node as RoomNode) == l_targetRoom && x.portName == l_targetPortName);//busco el puerto al que me tengo que conectar

                //si es un nodo 'begin', marco este nodo como el beginNodeInstantiated (en el begin node empieza la regla) -> sirve como referencia para saber donde empezar a conectar el puerto q quiero expandir 
                if (l_newInstantiatedRoomsList[i].roomType == "begin")
                {
                    m_beginNodeInstantiated = l_newInstantiatedRoomsList[i];
                }

                //conecto el puerto del nodo [i] con el targetPort -> en este primer linkado asigno el tempEdge a una variable m_currentBeginNodeEdge 
                //que luego me servira para encontrar el nodo conectado al nodo 'begin'
                LinkRoomPorts(l_newInstantiatedRoomsList[i].outputContainer[j].Q<Port>(), l_targetPortToConnect);

                //seteo su position basadome en la posicion del nodo expandido
                l_targetRoom.SetPosition(new Rect(_graphToLoadCache.roomNodeData.First(x => x.nodeID == l_targetRoom.roomID).position +
                    roomToExpand.GetPosition().position, m_graphView.defaultNodeSize));
            }//end for
        }//end of for loop

        //busco el puerto de entrada al que esta conectado la habitacion que quiero expandir
        Port l_basePort = m_graphView.edges.ToList().Find(x => (x.input.node as RoomNode) == roomToExpand).output;
        l_basePort.portColor = new Color(0, 0, 200);//blue

        //el puerto target será el que hemos marcado previamente al conectar los nodos de la regla que acabamos de cargar
        //el currentBeginNodeEdge es la conexion del nodo 'begin' -> cojemos el input, que es el puerto q pertenece al nodo 'begin'
        Port l_targetPort = m_currentBeginNodeEdge.input;
        l_targetPort.portColor = new Color(200, 0, 0);//red
        l_targetPort.portName = m_UtilitiesInstance.ReturnConnectionNameByReference(l_basePort.portName);//asignar el nuevo nombre del puerto dependiendo del puerto base conectado

        //elimino el nodo 'begin' -> solo sirve como referencia para saber donde conectar el nodo que queremos expandir
        m_graphView.RemoveElement(m_beginNodeInstantiated);

        //conecto los puertos 
        LinkRoomPorts(l_basePort, l_targetPort);

        List<RoomNode> l_currentNodesOnGraph = m_graphView.nodes.ToList().Cast<RoomNode>().ToList();

        //asignar nuevas id a cada nodo 
        for (int i = 0; i < l_currentNodesOnGraph.Count; i++)
        {
            for (int j = 0; j < l_newInstantiatedRoomsList.Count(); j++)
            {
                if (l_currentNodesOnGraph[i] == l_newInstantiatedRoomsList[j])
                {

                    List<Edge> l_currentOutputRoomEdges = m_graphView.edges.ToList().Where(x => (x.output.node as RoomNode) == l_currentNodesOnGraph[i]).ToList();
                    List<Edge> l_currentInputRoomEdges = m_graphView.edges.ToList().Where(x => (x.input.node as RoomNode) == l_currentNodesOnGraph[i]).ToList();

                    string l_newRoomID = l_currentNodesOnGraph[i].roomID = System.Guid.NewGuid().ToString();

                    foreach (Edge e in l_currentOutputRoomEdges)
                    {
                        e.output.name = l_newRoomID;
                    }
                    foreach (Edge e in l_currentInputRoomEdges)
                    {
                        e.input.name = l_newRoomID;
                    }
                }
            }
        }//end of for loop
    }//end expand nodes function

    private void LinkRoomPorts(Port _outPort, Port _inPort)
    {
        Edge l_tempEdge = new Edge()
        {
            output = _outPort,
            input = _inPort
        };
        m_edgesList.Add(l_tempEdge);

        if ((_outPort.node as RoomNode) == m_beginNodeInstantiated)
        {
            m_currentBeginNodeEdge = l_tempEdge;
        }
        else
        {
            l_tempEdge.input.Connect(l_tempEdge);
            l_tempEdge.output.Connect(l_tempEdge);
            m_graphView.Add(l_tempEdge);
        }
    }

    private void GenerateRandomDungeon()
    {
        if (m_numberOfRoomsToGenerate > 1)
        {
            m_roomAndItsInputsDictionary.Clear();
            m_edgesList.Clear();

            m_numberOfRoomsInstantiated = 0;
            ClearAllNodesOnGraph();
            m_currentlyInstantiatedRoomList.Clear();

            //empiezo instanciando un nodo 'Start', imprescindible para crear la geometria 
            RoomNode l_startNode = m_graphView.CreateRoomNode("Start", true);
            m_graphView.AddElement(l_startNode);
            m_numberOfRoomsInstantiated++;
            GenereteNeighbourRooms(l_startNode);

            //mientras no se hayan creado el numero maximo de habitaciones;
            while (m_numberOfRoomsInstantiated < m_numberOfRoomsToGenerate)
            {
                if (m_currentlyInstantiatedRoomList.Count > 0)
                {
                    GenereteNeighbourRooms(m_currentlyInstantiatedRoomList.First());
                    m_currentlyInstantiatedRoomList.Remove(m_currentlyInstantiatedRoomList.First());
                }
            }

            if (m_numberOfFloorsToGenerate > 0)
            {
                Debug.Log($"Generating {m_numberOfFloorsToGenerate} floors");

                GenerateStairsNodes();
            }
            else
            {
                Debug.Log("Currently generating 0 floors. You can create floors if you specify a number of floors to generate");
            }

        }

        else
        {
            Debug.Log("enter a max number of rooms to generate (bigger than 1)");
        }


    }

    private void GenereteNeighbourRooms(RoomNode _baseRoom)
    {

        int l_numberOfConnections;
        //si es la 'start' room puedo crear 4 conexiones, sino solo puedo crear 3 (1 viene con el input previo)
        if (_baseRoom.roomType == "Start")
        {
            l_numberOfConnections = Random.Range(1, 5);
        }
        else
        {
            l_numberOfConnections = Random.Range(1, 4);
        }

        //cojo todos los strings posibles -> no usamos los que contengan stairs
        List<string> l_posibleBasePortNames = m_UtilitiesInstance.m_myConnectionsDictionary.Keys.ToList().Where(x => !x.Contains("stairs")).ToList();

        //busco en el diccionary el tipo de input que tiene la _baseRoom
        string l_conectedInputName = m_roomAndItsInputsDictionary.ToList().Find(x => x.Key == _baseRoom).Value;

        //dependiendo del input que tengo, debo limitar las opciones de mis outputs
        if (l_conectedInputName != null)
        {
            if (l_conectedInputName.Contains("up"))
            {
                l_posibleBasePortNames.RemoveAll(x => x.Contains("up"));
            }
            else if (l_conectedInputName.Contains("down"))
            {
                l_posibleBasePortNames.RemoveAll(x => x.Contains("down"));
            }
            else if (l_conectedInputName.Contains("left"))
            {
                l_posibleBasePortNames.RemoveAll(x => x.Contains("left"));
            }
            else if (l_conectedInputName.Contains("right"))
            {
                l_posibleBasePortNames.RemoveAll(x => x.Contains("right"));
            }
        }

        for (int i = 0; i < l_numberOfConnections; i++)
        {
            //seleccionar un tipo de habitacion random
            int l_typesOfRoomsCount = m_UtilitiesInstance.m_typesOfRoomsList.Count();
            int l_randomSelectionValue = Random.Range(0, l_typesOfRoomsCount);
            string l_selectedRoomType = m_UtilitiesInstance.m_typesOfRoomsList[l_randomSelectionValue];

            //seleccionar un nombre aleatorio de la lista l_posibleBasePortNames para el puerto
            int l_numberOfDifferentConnections = l_posibleBasePortNames.Count();
            int l_randomSelectionPortName = Random.Range(0, l_numberOfDifferentConnections);
            string l_selectedNameOfBasePort = l_posibleBasePortNames.ElementAt(l_randomSelectionPortName);
            string l_nameOfTargetPortByReference = m_UtilitiesInstance.ReturnConnectionNameByReference(l_selectedNameOfBasePort);

            //spawnear la habitacion y añadirla al graph (+dictionary +list +counter)
            RoomNode l_newRoomToSpawn = m_graphView.CreateRoomNode(l_selectedRoomType, true);
            m_graphView.AddElement(l_newRoomToSpawn);
            m_roomAndItsInputsDictionary.Add(l_newRoomToSpawn, l_nameOfTargetPortByReference);
            m_currentlyInstantiatedRoomList.Add(l_newRoomToSpawn);
            m_numberOfRoomsInstantiated++;


            //DONT REPEAT NAMES OF PREVIOUS PORTS
            if (l_selectedNameOfBasePort.Contains("up"))
            {
                l_posibleBasePortNames.RemoveAll(x => x.Contains("up"));
            }
            else if (l_selectedNameOfBasePort.Contains("down"))
            {
                l_posibleBasePortNames.RemoveAll(x => x.Contains("down"));
            }
            else if (l_selectedNameOfBasePort.Contains("left"))
            {
                l_posibleBasePortNames.RemoveAll(x => x.Contains("left"));
            }
            else if (l_selectedNameOfBasePort.Contains("right"))
            {
                l_posibleBasePortNames.RemoveAll(x => x.Contains("right"));
            }

            //generate and connect ports
            Port l_basePort = m_graphView.GenerateOutputPortsOnNode(_baseRoom, l_selectedNameOfBasePort);
            Port l_targetPort = m_graphView.GenerateInputPortsOnNode(l_newRoomToSpawn, l_nameOfTargetPortByReference);
            LinkRoomPorts(l_basePort, l_targetPort);

            //assign new position to new node
            Vector3 l_newPosition = Vector3.zero;
            Vector3 l_prevPosition = _baseRoom.transform.position;
            if (l_basePort.portName.Contains("up"))
            {
                l_newPosition.x = l_prevPosition.x;
                l_newPosition.y = l_prevPosition.y - 200;
                l_newPosition.z = l_prevPosition.z;
            }
            if (l_basePort.portName.Contains("down"))
            {
                l_newPosition.x = l_prevPosition.x;
                l_newPosition.y = l_prevPosition.y + 200;
                l_newPosition.z = l_prevPosition.z;
            }
            if (l_basePort.portName.Contains("left"))
            {
                l_newPosition.x = l_prevPosition.x - 400;
                l_newPosition.y = l_prevPosition.y;
                l_newPosition.z = l_prevPosition.z;

            }
            if (l_basePort.portName.Contains("right"))
            {
                l_newPosition.x = l_prevPosition.x + 400;
                l_newPosition.y = l_prevPosition.y;
                l_newPosition.z = l_prevPosition.z;

            }

            //AL USAR LOS DOS METODOS A LA VEZ, se guarda la posicion correctamente, 
            //pero al generarse los nodos se desplazan de las posicion asignada-> ARREGLAR

            //posicion del visual object -> si uso esta sola, no se guardan correctamente al usar SaveGraph()
            l_newRoomToSpawn.transform.position = l_newPosition;
            //posicion del nodo como "rect" -> si uso esta sola, el nuevo nodo siempre parte de la pos del nodo 'Start'
            l_newRoomToSpawn.SetPosition(new Rect(l_newPosition, m_graphView.defaultNodeSize));


        }
    }


    private void GenerateStairsNodes()
    {

        List<RoomNode> l_allNodesOnGraphWithOutputs = m_graphView.nodes.ToList().Where(x => x.outputContainer.Q<Port>() != null).Cast<RoomNode>().ToList();
        List<RoomNode> nodesAlreadyUsed = new List<RoomNode>();
        int numberOfNodes = l_allNodesOnGraphWithOutputs.Count();
        for (int i = 0; i < m_numberOfFloorsToGenerate; i++)
        {

            int randomSelectionNode = Random.Range(0, numberOfNodes);
            RoomNode selectedNode = l_allNodesOnGraphWithOutputs.ElementAt(randomSelectionNode);

            while (nodesAlreadyUsed.Contains(selectedNode))
            {
                randomSelectionNode = Random.Range(0, numberOfNodes);
                selectedNode = l_allNodesOnGraphWithOutputs.ElementAt(randomSelectionNode);
            }

            nodesAlreadyUsed.Add(selectedNode);

            List<Port> l_portsConnectedToSelectedNode = m_graphView.ports.ToList().Where(x => x.node == selectedNode && x.direction == Direction.Output).ToList();


            int randomSelectionPort = Random.Range(0, l_portsConnectedToSelectedNode.Count());
            Port selectedPort = l_portsConnectedToSelectedNode.ElementAt(randomSelectionPort);

            Edge conectorEdge = m_edgesList.Find(x => x.output == selectedPort);
            conectorEdge.output.portName += " stairs";

        }



    }


    //EXTRA FUNCTIONS
    private void ClearAllNodesOnGraph()
    {
        foreach (Edge e in m_graphView.edges.ToList())
        {
            m_graphView.RemoveElement(e);
        }
        foreach (Node n in m_graphView.nodes.ToList())
        {
            m_graphView.RemoveElement(n);
        }
    }
}
