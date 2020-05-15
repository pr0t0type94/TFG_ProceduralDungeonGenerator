using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RoomScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject[] m_doorsPlaceHolders = new GameObject[4];
    private Vector3 roomSize;
    public GameObject m_doorPrefab;

    public Vector3 returnRoomSize()
    {
        var goRenderer = gameObject.GetComponent<MeshRenderer>();
        return new Vector3(0, 0, goRenderer.bounds.size.z);
    }
    public void getParents()
    {
        m_doorsPlaceHolders = gameObject.GetComponentsInChildren<GameObject>();
       
    }


    public void GenerateDoor(string _roomConnection)
    {
        switch(_roomConnection)
        {
            case "up":

                m_doorsPlaceHolders[0].SetActive(false);

                break;

            case "down":

                m_doorsPlaceHolders[1].SetActive(false);

                break;
            case "left":
                //m_doorsPlaceHolders[2].SetActive(false);


                break;
            case "rigth":

                //m_doorsPlaceHolders[3].SetActive(false);

                break;
        }

    }
    public void ConnectRoom()
    {

    }

    public Vector3 returnPosition(string _name)
    {
        GameObject selectedDoor = m_doorsPlaceHolders.First(x => x.name == _name);
        Debug.Log(selectedDoor.name);
        return selectedDoor.transform.position;
    }
}
