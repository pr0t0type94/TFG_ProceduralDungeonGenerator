using UnityEditor.Experimental.GraphView; 

public class RoomNode : Node
{
    public string roomID;//nombre identificador del nodo

    public string roomType;// tipo de nodo -> campo para rellenar

    public bool isSelected = false;
    public bool isTerminal = false;

    public override void OnSelected()
    {
        base.OnSelected();
        isSelected = true;

    }
    public override void OnUnselected()
    {
        base.OnUnselected();
        isSelected = false;
       
    }
}
