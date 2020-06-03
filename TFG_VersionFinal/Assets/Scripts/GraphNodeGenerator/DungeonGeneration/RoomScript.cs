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
    public GameObject m_staircasePrefab;

    public Vector3 returnRoomSize()
    {
        var goRenderer = gameObject.GetComponent<MeshRenderer>();
        return new Vector3(goRenderer.bounds.size.x, goRenderer.bounds.size.y, goRenderer.bounds.size.z);
    }
    public void getParents()
    {
        m_doorsPlaceHolders = gameObject.GetComponentsInChildren<GameObject>();
       
    }


    public void GenerateDoor(string _roomConnection)
    {

        if (_roomConnection.Contains("up"))
        {
            m_doorsPlaceHolders[0].SetActive(false);

            if (_roomConnection.Contains("key"))
            {
                GameObject doorPrefab = Instantiate(m_doorPrefab, gameObject.transform);
                doorPrefab.transform.position = m_doorsPlaceHolders[0].transform.position;
                doorPrefab.transform.rotation = m_doorsPlaceHolders[0].transform.rotation;

            }
            else if (_roomConnection.Contains("stairs"))
            {
                GameObject stairsPrefab = Instantiate(m_staircasePrefab,gameObject.transform);
                Vector3 bounds = new Vector3(0, stairsPrefab.GetComponent<MeshRenderer>().bounds.size.y/3, stairsPrefab.GetComponent<MeshRenderer>().bounds.size.z/2);
                stairsPrefab.transform.position = m_doorsPlaceHolders[0].transform.position + bounds;
                stairsPrefab.transform.Rotate(new Vector3(0, 0, 0));
            }
        }
        else if (_roomConnection.Contains("down"))
        {
            m_doorsPlaceHolders[1].SetActive(false);

            if (_roomConnection.Contains("key"))
            {
                GameObject doorPrefab = Instantiate(m_doorPrefab, gameObject.transform);
                doorPrefab.transform.position = m_doorsPlaceHolders[1].transform.position;
                doorPrefab.transform.rotation = m_doorsPlaceHolders[1].transform.rotation;
            }
            else if (_roomConnection.Contains("stairs"))
            {
                GameObject stairsPrefab = Instantiate(m_staircasePrefab, gameObject.transform);
                Vector3 bounds = new Vector3(0, -stairsPrefab.GetComponent<MeshRenderer>().bounds.size.y / 3, stairsPrefab.GetComponent<MeshRenderer>().bounds.size.z / 2);
                stairsPrefab.transform.position = m_doorsPlaceHolders[1].transform.position - bounds;
                stairsPrefab.transform.Rotate(new Vector3(0, 180, 0));
            }
        }
        else if (_roomConnection.Contains("left"))
        {
            m_doorsPlaceHolders[2].SetActive(false);

            if (_roomConnection.Contains("key"))
            {
                GameObject doorPrefab = Instantiate(m_doorPrefab, gameObject.transform);
                doorPrefab.transform.position = m_doorsPlaceHolders[2].transform.position;
                doorPrefab.transform.rotation = m_doorsPlaceHolders[2].transform.rotation;
            }
            else if (_roomConnection.Contains("stairs"))
            {
                GameObject stairsPrefab = Instantiate(m_staircasePrefab, gameObject.transform);
                Vector3 bounds = new Vector3(stairsPrefab.GetComponent<MeshRenderer>().bounds.size.z / 2, -stairsPrefab.GetComponent<MeshRenderer>().bounds.size.y / 3, 0);
                stairsPrefab.transform.position = m_doorsPlaceHolders[2].transform.position - bounds;
                stairsPrefab.transform.Rotate(new Vector3(0, 270, 0));
            }
        }
        else if (_roomConnection.Contains("right"))
        {
            m_doorsPlaceHolders[3].SetActive(false);

            if (_roomConnection.Contains("key"))
            {
                GameObject doorPrefab = Instantiate(m_doorPrefab, gameObject.transform);
                doorPrefab.transform.position = m_doorsPlaceHolders[3].transform.position;
                doorPrefab.transform.rotation = m_doorsPlaceHolders[3].transform.rotation;
            }
            else if (_roomConnection.Contains("stairs"))
            {
                GameObject stairsPrefab = Instantiate(m_staircasePrefab, gameObject.transform);
                Vector3 bounds = new Vector3(stairsPrefab.GetComponent<MeshRenderer>().bounds.size.z / 2, stairsPrefab.GetComponent<MeshRenderer>().bounds.size.y / 3, 0);
                stairsPrefab.transform.position = m_doorsPlaceHolders[3].transform.position + bounds;
                stairsPrefab.transform.Rotate(new Vector3(0, 90, 0));
            }
        }
    }

    public Vector3 returnPosition(string _name)
    {
        GameObject selectedDoor = m_doorsPlaceHolders.First(x => x.name == _name);
        return selectedDoor.transform.position;
    }
}
