using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;


public class GrammarGraphView : GraphView
{

    public readonly Vector2 defaultNodeSize = new Vector2(200, 200);

   public GrammarGraphView()
    {

        //zoom in and out
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        //logica basica para seleccionar y hacer dragg de objetos // viene por defecto en el Engine
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new RectangleSelector());

        
        //generar el nodo start 
        //AddElement(GenerateStartNode());
        //AddElement(GenerateEndNode());
        //createNode("Room");
    }

    //generar un puerto en el nodo -> puerto sirve para enlazar conexiones
    public Port GeneratePort(RoomNode targetRoom, Direction portDir, Port.Capacity capacity)
    {
        return targetRoom.InstantiatePort(Orientation.Horizontal, portDir, capacity, typeof(string));
    }

    //nodo start
    //private RoomNode GenerateStartNode()
    //{
    //    RoomNode node = new RoomNode
    //    {
    //        title = "START",
    //        roomID = Guid.NewGuid().ToString(),
    //        roomType = "start",
                        
    //    };

    //    //generar el puerto de salida 
    //    Port generatedPort = GeneratePort(node, Direction.Output, Port.Capacity.Multi);
    //    generatedPort.portName = "Next";
    //    node.outputContainer.Add(generatedPort);
    //    //refresh size of node
    //    node.RefreshExpandedState();
    //    node.RefreshPorts();

    //    node.SetPosition(new Rect(100, 200, 200, 250));
    //    return node;
    //}


    public void createNode(string nodeName)
    {
        AddElement(CreateRoomNode(nodeName));
    }

    public RoomNode CreateRoomNode(string _roomName, bool _isTerminal =false)
    {
        RoomNode l_roomNode = new RoomNode
        {
            title = _roomName,
            roomID = Guid.NewGuid().ToString(),
            roomType = _roomName,
            isTerminal = _isTerminal
             
        };
        //set node position and size
        l_roomNode.SetPosition(new Rect(20,20, 200,300));

        Toggle isTerminalRoom = new Toggle();
        isTerminalRoom.text = "isTerminal";
        isTerminalRoom.RegisterValueChangedCallback(evt => l_roomNode.isTerminal = evt.newValue);
        isTerminalRoom.value = l_roomNode.isTerminal;
        isTerminalRoom.SetValueWithoutNotify(isTerminalRoom.value);
        l_roomNode.titleContainer.Add(isTerminalRoom);
        l_roomNode.RefreshExpandedState();


        //creamos un textField donde poner el tipo de habitacion
        TextField roomTypeTextField = new TextField(string.Empty);
        //registramos un callback que hara lo siguiente cuando se llame;
        roomTypeTextField.RegisterValueChangedCallback(evt => {
            l_roomNode.roomType = evt.newValue;//cambiamos el tipo de hab. al nuevo valor
            l_roomNode.title = evt.newValue;//actualizamos el titulo del nodo con el nuevo valor

        });
        //para no triggear el callback de arriba, utilizamos el without notify y actualizamos los textos
        roomTypeTextField.SetValueWithoutNotify(l_roomNode.title);//ponemos el nombre generado 
        l_roomNode.mainContainer.Add(roomTypeTextField);//añadimos el nombre al titulo del nodo


        Button inButton = new Button();
        inButton.text = "Add Input";
        inButton.clickable.clicked += () => GenerateInputPortsOnNode(l_roomNode);
        l_roomNode.titleContainer.Add(inButton);

        //botton para añadir puertos de salida
        Button portButton = new Button();
        portButton.text = "Add Output";
        portButton.clickable.clicked += () => GenerateOutputPortsOnNode(l_roomNode);
        l_roomNode.titleContainer.Add(portButton);


        //if (l_roomNode.IsSelected(l_roomNode)) { m_selectedRoomNode = l_roomNode;  Debug.Log("selected room"); } 



        return l_roomNode;
    }



    //agregar mas puertos de conexion out al nodo
    public Port GenerateOutputPortsOnNode(RoomNode _roomNode, string overridedName = "")
    {

        Port generatedPort = GeneratePort(_roomNode, Direction.Output, Port.Capacity.Single);
        

        //asignar nombre al puerto de salida
        //int outputPortCount = _roomNode.outputContainer.Query(name: "connector").ToList().Count;
        string portName;
        portName = string.IsNullOrEmpty(overridedName)? "Output" : overridedName;//si no ponemos nombre, que se muestre 'output' por defecto

        //borrar las etiquetas(nombres) anteriores 
        //Label oldPortName = generatedPort.contentContainer.Q<Label>("type");//cogemos los elementos <Label> de la cola del nodo
        //generatedPort.contentContainer.Remove(oldPortName);

        //generamos el campo de texto donde pondremos el nombre de la conexion
        TextField portText = new TextField();
        portText.value = portName; //asignamos el valor del puerto a la nueva string
        portText.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);//capturamos el evento de cambiar el nombre en el textField y le asignamos ese nombre al puertoGenerado
        generatedPort.contentContainer.Add(portText);
        generatedPort.portName = portName;


        Button deleteOutPortButton = new Button();
        deleteOutPortButton.text = "-";
        deleteOutPortButton.clickable.clicked += () => RemovePort(_roomNode, generatedPort,"out");
        generatedPort.contentContainer.Add(deleteOutPortButton);


        _roomNode.outputContainer.Add(generatedPort);
        _roomNode.RefreshPorts();//actualizamos la lista de puertos
        _roomNode.RefreshExpandedState();//hacer visibles los nuevos elementos del container

        return generatedPort;
    }

    public Port GenerateInputPortsOnNode(RoomNode _roomNode, string overridedName = "") {

        Port generatedPort = GeneratePort(_roomNode, Direction.Input, Port.Capacity.Single);

        //asignar nombre al puerto de salida
        //int outputPortCount = _roomNode.outputContainer.Query(name: "connector").ToList().Count;
        string portName;
        portName = string.IsNullOrEmpty(overridedName) ? "Input" : overridedName;//si no ponemos nombre, que se muestre el por defecto

        //borrar las etiquetas(nombres) anteriores 
        //Label oldPortName = generatedPort.contentContainer.Q<Label>("type");//cogemos los elementos <Label> de la cola del nodo
        //generatedPort.contentContainer.Remove(oldPortName);

        //generamos el campo de texto donde pondremos el nombre de la conexion
        TextField portText = new TextField();
        portText.value = portName; //asignamos el valor del puerto a la nueva string
        portText.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);//capturamos el evento de cambiar el nombre en el textField y le asignamos ese nombre al puertoGenerado
        generatedPort.contentContainer.Add(portText);
        generatedPort.portName = portName;

        Button deleteInputPortButton = new Button();
        deleteInputPortButton.text = "-";
        deleteInputPortButton.clickable.clicked += () => RemovePort(_roomNode, generatedPort,"input");
        generatedPort.contentContainer.Add(deleteInputPortButton);



        _roomNode.inputContainer.Add(generatedPort);
        _roomNode.RefreshPorts();
        _roomNode.RefreshExpandedState();

        return generatedPort;
    }


    private void RemovePort(RoomNode _roomNode, Port _portToRemove, string _portDirection)
    {
        List<Edge> edgesToList;
        //identificamos los puertos que provengan de este room node & make sure este port proviene de este room node
        if (_portDirection=="out")
        {
            _roomNode.outputContainer.Remove(_portToRemove);
            edgesToList = edges.ToList().Where(x => x.output.portName == _portToRemove.portName && x.output.node == _portToRemove.node).ToList();

            if (edgesToList.Any())
            {

                Edge targetEdge = edgesToList.First();
                targetEdge.input.Disconnect(targetEdge);
                RemoveElement(edgesToList.First());

            }

        }
        else
        {
            _roomNode.inputContainer.Remove(_portToRemove);
            edgesToList = edges.ToList().Where(x => x.input.portName == _portToRemove.portName && x.input.node == _portToRemove.node).ToList();

            if (edgesToList.Any())
            {

                Edge targetEdge = edgesToList.First();
                targetEdge.output.Disconnect(targetEdge);
                RemoveElement(edgesToList.First());

            }
        }


        _roomNode.RefreshPorts();
        _roomNode.RefreshExpandedState();
        
        //List<Edge> l_edgesToRemove = edges.ToList();

        //foreach(Edge e in l_edgesToRemove)
        //{         

        //    if(e.output.portName == _portToRemove.portName && e.output.node == _portToRemove.node)
        //    {
        //        e.input.Disconnect(e);
        //        RemoveElement(e);
        //        _roomNode.outputContainer.Remove(_portToRemove);
        //        _roomNode.RefreshPorts();
        //        _roomNode.RefreshExpandedState();
        //        //_roomNode.connectedPorts.Remove(_portToRemove);

        //    }
        //}
    }

    // metodo para generar nuevos puertos en un nodo //definimos los posibles tipos de input de los puertos //obligatorio hacer override sino, no poemos conectar nodos 
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> nodePortsList = new List<Port>();//creamos una lista con todos los puertos de ese nodo

        foreach(Port p in ports.ToList())
        {
            //nos aseguramos de que el nodo que miramos no es el nuestro mismo, ni conectamos un puerto consigo mismo
            if (startPort != p && startPort.node != p.node)
            {
                nodePortsList.Add(p);
            }
        }

        //ports.ForEach((port) => //para cada nodo;
        //{
        //    //nos aseguramos de que el nodo que miramos no es el nuestro mismo, ni conectamos un puerto consigo mismo
        //    if (startPort != port && startPort.node != port.node)
        //    {
        //        nodePortsList.Add(port);
        //    }
        //});

        return nodePortsList;

    }


    //public RoomNode returnSelectedNode() {

    //    RoomNode l_selectedNode = nodes.ToList().Find(x => x.selected == true) as RoomNode;
    //    Debug.Log(l_selectedNode.name.ToString());

    //    return l_selectedNode;
    //}

}//end of graphView class
