using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


public class GrammarGraphView : GraphView
{

    public readonly Vector2 defaultNodeSize = new Vector2(200, 200);

    public GrammarGraphView()
    {
        //zoom in and out
        SetupZoom(0.1f, ContentZoomer.DefaultMaxScale);

        //logica basica para seleccionar y hacer dragg de objetos // viene por defecto en el Engine
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new RectangleSelector());
    }

    //generar un puerto en el nodo -> puerto sirve para enlazar conexiones
    public Port GeneratePort(RoomNode _targetRoom, Direction _portDir, Port.Capacity _capacity)
    {
        return _targetRoom.InstantiatePort(Orientation.Horizontal, _portDir, _capacity, typeof(string));
    }

    public void createNode(string _nodeName)
    {
        AddElement(CreateRoomNode(_nodeName));
    }

    public RoomNode CreateRoomNode(string _roomName, bool _isTerminal = true)
    {
        RoomNode l_roomNode = new RoomNode
        {
            title = _roomName,
            roomID = Guid.NewGuid().ToString(),
            roomType = _roomName,
            isTerminal = _isTerminal

        };
        //set node position and size
        l_roomNode.SetPosition(new Rect(20, 20, 200, 300));

        //boton para marcar si es terminal o no
        Toggle l_isTerminalRoom = new Toggle();
        l_isTerminalRoom.text = "isTerminal";
        l_isTerminalRoom.RegisterValueChangedCallback(evt => l_roomNode.isTerminal = evt.newValue);
        l_isTerminalRoom.value = l_roomNode.isTerminal;
        l_isTerminalRoom.SetValueWithoutNotify(l_isTerminalRoom.value);
        l_roomNode.titleContainer.Add(l_isTerminalRoom);
        l_roomNode.RefreshExpandedState();


        //creamos un textField donde poner el tipo de habitacion
        TextField l_roomTypeTextField = new TextField(string.Empty);
        //registramos un callback que hara lo siguiente cuando se llame;
        l_roomTypeTextField.RegisterValueChangedCallback(evt =>
        {
            l_roomNode.roomType = evt.newValue;//cambiamos el tipo de hab. al nuevo valor
            l_roomNode.title = evt.newValue;//actualizamos el titulo del nodo con el nuevo valor

        });
        //para no triggear el callback de arriba, utilizamos el without notify y actualizamos los textos
        l_roomTypeTextField.SetValueWithoutNotify(l_roomNode.title);//ponemos el nombre generado 
        l_roomNode.mainContainer.Add(l_roomTypeTextField);//añadimos el nombre al titulo del nodo


        Button l_addInputPortButton = new Button();
        l_addInputPortButton.text = "Add Input";
        l_addInputPortButton.clickable.clicked += () => GenerateInputPortsOnNode(l_roomNode);
        l_roomNode.titleContainer.Add(l_addInputPortButton);

        //botton para añadir puertos de salida
        Button l_addOutPortButton = new Button();
        l_addOutPortButton.text = "Add Output";
        l_addOutPortButton.clickable.clicked += () => GenerateOutputPortsOnNode(l_roomNode);
        l_roomNode.titleContainer.Add(l_addOutPortButton);

        //if (l_roomNode.IsSelected(l_roomNode)) { m_selectedRoomNode = l_roomNode;  Debug.Log("selected room " +); } 


        return l_roomNode;
    }



    //agregar mas puertos de conexion out al nodo
    public Port GenerateOutputPortsOnNode(RoomNode _roomNode, string _overridedName = "")
    {

        Port l_generatedPort = GeneratePort(_roomNode, Direction.Output, Port.Capacity.Single);


        //asignar nombre al puerto de salida
        //int outputPortCount = _roomNode.outputContainer.Query(name: "connector").ToList().Count;
        string l_portName;
        l_portName = string.IsNullOrEmpty(_overridedName) ? "Output" : _overridedName;//si no ponemos nombre, que se muestre 'output' por defecto

        //borrar las etiquetas(nombres) anteriores 
        //Label oldPortName = generatedPort.contentContainer.Q<Label>("type");//cogemos los elementos <Label> de la cola del nodo
        //generatedPort.contentContainer.Remove(oldPortName);

        //generamos el campo de texto donde pondremos el nombre de la conexion
        TextField l_portText = new TextField();
        l_portText.value = l_portName; //asignamos el valor del puerto a la nueva string
        l_portText.RegisterValueChangedCallback(evt => l_generatedPort.portName = evt.newValue);//capturamos el evento de cambiar el nombre en el textField y le asignamos ese nombre al puertoGenerado
        l_generatedPort.contentContainer.Add(l_portText);
        l_generatedPort.portName = l_portName;

        //boton para borrar el puerto
        Button l_deleteOutPortButton = new Button();
        l_deleteOutPortButton.text = "-";
        l_deleteOutPortButton.clickable.clicked += () => RemovePort(_roomNode, l_generatedPort, "out");
        l_generatedPort.contentContainer.Add(l_deleteOutPortButton);


        _roomNode.outputContainer.Add(l_generatedPort);
        _roomNode.RefreshPorts();//actualizamos la lista de puertos
        _roomNode.RefreshExpandedState();//hacer visibles los nuevos elementos del container

        return l_generatedPort;
    }

    public Port GenerateInputPortsOnNode(RoomNode _roomNode, string _overridedName = "")
    {

        Port l_generatedPort = GeneratePort(_roomNode, Direction.Input, Port.Capacity.Single);

        string portName;
        portName = string.IsNullOrEmpty(_overridedName) ? "Input" : _overridedName;//si no ponemos nombre, que se muestre el por defecto

        //generamos el campo de texto donde pondremos el nombre de la conexion
        TextField portText = new TextField();
        portText.value = portName; //asignamos el valor del puerto a la nueva string
        portText.RegisterValueChangedCallback(evt => l_generatedPort.portName = evt.newValue);//capturamos el evento de cambiar el nombre en el textField y le asignamos ese nombre al puertoGenerado
        l_generatedPort.contentContainer.Add(portText);
        l_generatedPort.portName = portName;

        //boton para borrar el puerto
        Button deleteInputPortButton = new Button();
        deleteInputPortButton.text = "-";
        deleteInputPortButton.clickable.clicked += () => RemovePort(_roomNode, l_generatedPort, "input");
        l_generatedPort.contentContainer.Add(deleteInputPortButton);

        _roomNode.inputContainer.Add(l_generatedPort);
        _roomNode.RefreshPorts();
        _roomNode.RefreshExpandedState();

        return l_generatedPort;
    }


    private void RemovePort(RoomNode _roomNode, Port _portToRemove, string _portDirection)
    {
        List<Edge> edgesToList;
        //identificamos los puertos que provengan de este room node & nos aseguramos de que este port proviene de este room node
        if (_portDirection == "out")
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

    }

    //metodo para aceptar conexiones de un puerto //definimos los posibles tipos de input de los puertos 
    //obligatorio hacer override de la funcion, sino no podemos conectar nodos 
    public override List<Port> GetCompatiblePorts(Port _basePort, NodeAdapter nodeAdapter)
    {
        List<Port> nodePortsList = new List<Port>();//creamos una lista con todos los puertos de ese nodo

        foreach (Port p in ports.ToList())
        {
            //nos aseguramos de que el nodo que miramos no es el nuestro mismo, ni conectamos un puerto consigo mismo
            if (_basePort != p && _basePort.node != p.node)
            {
                nodePortsList.Add(p);
            }
        }

        return nodePortsList;

    }

}//end of graphView class
