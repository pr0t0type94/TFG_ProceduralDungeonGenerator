using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GrammarGraphWindow : EditorWindow
{

    ///// VARIABLES
    private GrammarGraphView m_graphView;

    private int m_numberOfRoomsToGenerate;
    int m_currentNumberOfRooms;

    private string m_ruleFileName = "New Grammar";
    private string m_finalGraphName = "Final Grammar";

    private Edge m_currentBeginNodeEdge;
    private RoomNode m_beginNodeInstantiated;
    private List<RoomInfoContainer> m_resourcesRulesLoaded = new List<RoomInfoContainer>();

    List<RoomNode> m_currentInstantiatedRooms = new List<RoomNode>();

    UtilitiesAndReferencesClass m_UtilitiesInstance;

    Dictionary<RoomNode, string> m_roomInputsDictionary = new Dictionary<RoomNode, string>();


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
        m_UtilitiesInstance = UtilitiesAndReferencesClass.GetInstance();

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

        //poner el boton de guardar y cargar en la toolbar

        //toolbar.Add(new Button(() => SaveGrammarData()) { text = "Save Rule" });
        Button l_saveRuleButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        l_saveRuleButton.transform.position = new Vector3(60, 0, 0);
        l_saveRuleButton.clickable.clicked += () => SaveGrammarData(m_ruleFileName, false); //evento de clickar 
        l_saveRuleButton.text = "Save Rule";//nombre del boton
        l_toolbar.Add(l_saveRuleButton);//añadimos el boton a la toolbar

        //toolbar.Add(new Button(() => LoadGrammarData()) { text = "Load Rule" });
        Button l_loadRuleButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        l_loadRuleButton.transform.position = new Vector3(60, 0, 0);
        l_loadRuleButton.clickable.clicked += () => LoadGrammarData(m_ruleFileName, false); //evento de clickar 
        l_loadRuleButton.text = "Load Rule";//nombre del boton
        l_toolbar.Add(l_loadRuleButton);//añadimos el boton a la toolbar


        Button l_expandRoomButton = new Button(); //boton para 
        l_expandRoomButton.transform.position = new Vector3(130, 0, 0);
        l_expandRoomButton.clickable.clicked += () => ExpandNonTerminalNodes(); //evento de clickar 
        l_expandRoomButton.text = "Expand NonTerminal";//nombre del boton
        l_toolbar.Add(l_expandRoomButton);//añadimos el boton a la toolbar

        //campo de texto con el nombre para guardar archivo
        TextField l_finalGraphNameText = new TextField();//campo para guardar el nombre
        l_finalGraphNameText.transform.position = new Vector3(180, 0, 0);
        l_finalGraphNameText.SetValueWithoutNotify(m_finalGraphName);//guardar el valor por defecto
        l_finalGraphNameText.MarkDirtyRepaint();//actualizar el nuevo valor en el siguiente frame
        l_finalGraphNameText.RegisterValueChangedCallback(evt => m_finalGraphName = evt.newValue);//callback para cambiar el nombre del archivo
        l_toolbar.Add(l_finalGraphNameText);//añadimos la variable donde se cambia el nombre del archivo a la toolbar
                                        //toolbar.Add(new Button(() => LoadGrammarData()) { text = "Load Rule" });

        Button l_saveFinalGraphButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        l_saveFinalGraphButton.transform.position = new Vector3(180, 0, 0);
        l_finalGraphNameText.SetValueWithoutNotify(m_finalGraphName);//guardar el valor por defecto
        l_saveFinalGraphButton.clickable.clicked += () => SaveGrammarData(m_finalGraphName, true); //evento de clickar 
        l_saveFinalGraphButton.text = "Save Final Graph";//nombre del boton
        l_toolbar.Add(l_saveFinalGraphButton);//añadimos el boton a la toolbar

        Button l_loadFinalGraphButton = new Button(); //boton para crear un nodo tipo "Room", el clickEvent especifica que debe hacer el boton cuando es clicado
        l_loadFinalGraphButton.transform.position = new Vector3(180, 0, 0);
        l_loadFinalGraphButton.clickable.clicked += () => LoadGrammarData(m_finalGraphName, true); //evento de clickar 
        l_loadFinalGraphButton.text = "Load Final Graph";//nombre del boton
        l_toolbar.Add(l_loadFinalGraphButton);//añadimos el boton a la toolbar

        rootVisualElement.Add(l_toolbar);//finalmente añadimos la toolbar a la ventana del editor
    }//generar la barra de botones de la ventana

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
        //m_numberOfRoomsToGenerate = maxNumberOfRooms.value;
        l_secondaryToolbar.Add(l_maxNumberOfRoomsIntField);

        Button l_clearAllNodesOnGrapButton = new Button();
        l_clearAllNodesOnGrapButton.text = "Clear All";
        l_clearAllNodesOnGrapButton.clickable.clicked += () => ClearAllNodesOnGraph();
        l_secondaryToolbar.Add(l_clearAllNodesOnGrapButton);

        rootVisualElement.Add(l_secondaryToolbar);
    }


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

    private void ExpandNonTerminalNodes()//expandir los nodos NO MARCADOS con isTerminal
    {
        foreach (RoomNode rn in m_graphView.nodes.ToList())
        {
            if (!rn.isTerminal)
            {
                m_resourcesRulesLoaded = Resources.LoadAll<RoomInfoContainer>("Rules").ToList();

                if (m_resourcesRulesLoaded.Exists(x => x.name.Contains(rn.roomType)))
                {
                    RoomInfoContainer[] l_candidateRules = m_resourcesRulesLoaded.Where(x => x.name.Contains(rn.roomType)).ToArray();

                    int l_randomRuleSelection = Random.Range(0, l_candidateRules.Length);
                    RoomInfoContainer l_ruleToLoad = l_candidateRules[l_randomRuleSelection];

                    CreateExpandedNodes(l_ruleToLoad, rn);
                    m_graphView.RemoveElement(rn);

                    foreach (Edge e in m_graphView.edges.ToList())
                    {
                        if ((e.input.node as RoomNode) == rn)
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
            RoomNode l_tempRoom = m_graphView.CreateRoomNode(rData.nodeType, rData.isTerminal);
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
                string l_targetNodeId = l_connectionDataCache[j].targetNodeId;
                RoomNode l_targetRoom = l_newInstantiatedRoomsList.First(x => x.roomID == l_targetNodeId);

                string l_targetPortName = l_connectionDataCache[j].targetPortName;

                Port l_targetPortToConnect = m_graphView.ports.ToList().First(x => (x.node as RoomNode) == l_targetRoom && x.portName == l_targetPortName);

                if (l_newInstantiatedRoomsList[i].roomType == "begin")
                {
                    m_beginNodeInstantiated = l_newInstantiatedRoomsList[i];
                }

                LinkRoomPorts(l_newInstantiatedRoomsList[i].outputContainer[j].Q<Port>(), l_targetPortToConnect);

                l_targetRoom.SetPosition(new Rect(_graphToLoadCache.roomNodeData.First(x => x.nodeID == l_targetRoom.roomID).position +
                    roomToExpand.GetPosition().position, m_graphView.defaultNodeSize));
            }//end for
        }//end of for loop

        Port l_basePort = m_graphView.edges.ToList().Find(x => (x.input.node as RoomNode) == roomToExpand).output;
        l_basePort.portColor = new Color(0, 0, 200);//blue


        Port l_targetPort = m_currentBeginNodeEdge.input;
        l_targetPort.portColor = new Color(200, 0, 0);//red

        m_graphView.RemoveElement(m_beginNodeInstantiated);


        LinkRoomPorts(l_basePort, l_targetPort);
        //basePort.ConnectTo(targetPort);

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


    }
    private void LinkRoomPorts(Port _outPort, Port _inPort)
    {
        Edge l_tempEdge = new Edge()
        {
            output = _outPort,
            input = _inPort
        };

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
            m_roomInputsDictionary.Clear();

            m_currentNumberOfRooms = 0;
            ClearAllNodesOnGraph();
            m_currentInstantiatedRooms.Clear();


            RoomNode l_startNode = m_graphView.CreateRoomNode("Start", true);
            m_graphView.AddElement(l_startNode);
            m_currentNumberOfRooms++;
            GenereteNeighbourRooms(l_startNode);

            //for MAX ROOM NUMBER
            while (m_currentNumberOfRooms < m_numberOfRoomsToGenerate)
            {
                if (m_currentInstantiatedRooms.Count > 0)
                {
                    GenereteNeighbourRooms(m_currentInstantiatedRooms.First());
                    m_currentInstantiatedRooms.Remove(m_currentInstantiatedRooms.First());
                }
            }

        }

        else
        {
            Debug.Log("enter a max number of rooms to generate (bigger than 1)");
        }


    }



    private void GenereteNeighbourRooms(RoomNode _baseRoom)
    {
        //si es la 'start' room puedo crear 4 conexiones, sino solo puedo crear 3 (1 viene con el input previo)
        int l_numberOfConnections;
        if (_baseRoom.roomType == "Start")
        {
            l_numberOfConnections = Random.Range(1, 5);
        }
        else
        {
            l_numberOfConnections = Random.Range(1, 4);
        }

        //cojo todos los strings posibles 
        List<string> l_posibleBasePortNames = m_UtilitiesInstance.myConnectionsDictionary.Keys.ToList();
        
        //busco en el diccionary el tipo de input que tiene la _baseRoom
        string l_conectedInputName = m_roomInputsDictionary.ToList().Find(x => x.Key == _baseRoom).Value;

        //dpendiendo del input que tengo, debo limitar las opciones de mis outputs
        if(l_conectedInputName!=null)
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
            int l_typesOfRoomsCount = m_UtilitiesInstance.typesOfRoomsList.Count();
            int l_randomSelectionValue = Random.Range(0, l_typesOfRoomsCount);
            string l_selectedRoomType = m_UtilitiesInstance.typesOfRoomsList[l_randomSelectionValue];

            //seleccionar un nombre aleatorio de la lista l_posibleBasePortNames para el puerto
            int l_numberOfDifferentConnections = l_posibleBasePortNames.Count();
            int l_randomSelectionPortName = Random.Range(0, l_numberOfDifferentConnections);
            string l_selectedNameOfBasePort = l_posibleBasePortNames.ElementAt(l_randomSelectionPortName);
            string l_nameOfTargetPortByReference = m_UtilitiesInstance.returnConnectionNameReferences(l_selectedNameOfBasePort);
            
            //spawnear la habitacion y añadirla al graph (+dictionary +list +counter)
            RoomNode l_newRoomToSpawn = m_graphView.CreateRoomNode(l_selectedRoomType, true);
            m_graphView.AddElement(l_newRoomToSpawn);
            m_roomInputsDictionary.Add(l_newRoomToSpawn, l_nameOfTargetPortByReference);
            m_currentInstantiatedRooms.Add(l_newRoomToSpawn);
            m_currentNumberOfRooms++;
                        

            //DONT REPEAT NAMES OF NEXT PORTS
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

            //assign new positions
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
