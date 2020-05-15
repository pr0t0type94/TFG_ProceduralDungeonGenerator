using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform[] m_doorsPlaceHolders = new Transform[4];


    public void getParents()
    {
        m_doorsPlaceHolders = gameObject.GetComponentsInChildren<Transform>();
       
    }
    public GameObject m_doorPrefab;


    public void GenerateDoor(string _roomConnection)
    {
        switch(_roomConnection)
        {
            case "up":

                m_doorsPlaceHolders[0].gameObject.SetActive(false);
                Debug.Log("generate next door enter");

                break;

            case "down":

                m_doorsPlaceHolders[1].gameObject.SetActive(false);

                break;
            case "left":
                //m_doorsPlaceHolders[2].SetActive(false);


                break;
            case "rigth":

                //m_doorsPlaceHolders[3].SetActive(false);

                break;
        }

    }
    public void GenerateNextDoor(string _roomConnection)
    {

        switch (_roomConnection)
        {
            
            case "up":
                m_doorsPlaceHolders[1].gameObject.SetActive(false);

                break;

            case "down":
                m_doorsPlaceHolders[0].gameObject.SetActive(false);


                break;
            case "left":

                //m_doorsPlaceHolders[3].SetActive(false);

                break;
            case "rigth":
                //m_doorsPlaceHolders[2].SetActive(false);


                break;
        }

    }

    public void ConnectRoom()
    {

    }
}
