using System.Linq;
using UnityEngine;


public class RoomScript : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 roomSize;

    [SerializeField]
    private GameObject[] m_doorsPlaceHolders = new GameObject[4];

    public GameObject m_doorPrefab;
    public GameObject m_staircasePrefab;
    

    public Vector3 ReturnRoomSize()
    {
        MeshRenderer l_gameobjRenderer = gameObject.GetComponent<MeshRenderer>();
        return new Vector3(l_gameobjRenderer.bounds.size.x, l_gameobjRenderer.bounds.size.y, l_gameobjRenderer.bounds.size.z);
    }

    public void GenerateDoor(string _roomConnection)
    {

        if (_roomConnection.Contains("up"))
        {
            GenerateRoomConections(_roomConnection, 0);

        }
        else if (_roomConnection.Contains("down"))
        {
            GenerateRoomConections(_roomConnection, 1);

        }
        else if (_roomConnection.Contains("left"))
        {
            GenerateRoomConections(_roomConnection, 2);

        }
        else if (_roomConnection.Contains("right"))
        {
            GenerateRoomConections(_roomConnection, 3);
        }
    }


    public Vector3 returnPosition(string _name)
    {
        GameObject l_selectedDoor = m_doorsPlaceHolders.First(x => x.name == _name);
        return l_selectedDoor.transform.position;
    }

    public void GenerateRoomConections(string _type, int _doorIndex)
    {
        m_doorsPlaceHolders[_doorIndex].SetActive(false);

         if (_type.Contains("key"))
         {
            InstantiateDoor(_doorIndex);
         }

        if (_type.Contains("up"))
        {
            if (_type.Contains("key") && _type.Contains("stairs"))
            {
                InstantiateDoor(_doorIndex);
                InstantiateStairs("up", _doorIndex);
            }
            else if (_type.Contains("stairs"))
            {
                InstantiateStairs("up", _doorIndex);
            }
        }

        else if (_type.Contains("down"))
        {
            if (_type.Contains("key") && _type.Contains("stairs"))
            {
                InstantiateDoor(_doorIndex);
                InstantiateStairs("down", _doorIndex);
            }
            else if (_type.Contains("stairs"))
            {
                InstantiateStairs("down", _doorIndex);
            }
        }

        else if (_type.Contains("left"))
        {
            if (_type.Contains("key") && _type.Contains("stairs"))
            {
                InstantiateDoor(_doorIndex);
                InstantiateStairs("left", _doorIndex);
            }
            else if (_type.Contains("stairs"))
            {
                InstantiateStairs("left", _doorIndex);
            }
        }

        else if (_type.Contains("right"))
        {
            if (_type.Contains("key") && _type.Contains("stairs"))
            {
                InstantiateDoor(_doorIndex);
                InstantiateStairs("right", _doorIndex);
            }
            else if (_type.Contains("stairs"))
            {
                InstantiateStairs("right", _doorIndex);
            }
        }
    }

    private void InstantiateDoor(int _doorIndex)
    {
        GameObject doorPrefab = Instantiate(m_doorPrefab, gameObject.transform);
        doorPrefab.transform.position = m_doorsPlaceHolders[_doorIndex].transform.position;
        doorPrefab.transform.rotation = m_doorsPlaceHolders[_doorIndex].transform.rotation;
    }

    private void InstantiateStairs(string _type, int _doorIndex)
    {

            GameObject stairsPrefab = Instantiate(m_staircasePrefab, gameObject.transform);
        Vector3 bounds = Vector3.zero;

        if (_type == "up")
        {
             bounds = new Vector3(0, stairsPrefab.GetComponent<MeshRenderer>().bounds.size.y / 3, stairsPrefab.GetComponent<MeshRenderer>().bounds.size.z / 2);
            stairsPrefab.transform.Rotate(new Vector3(0, 0, 0));
            stairsPrefab.transform.position = m_doorsPlaceHolders[_doorIndex].transform.position + bounds;

        }
        if (_type == ("down"))
        {
             bounds = new Vector3(0, -stairsPrefab.GetComponent<MeshRenderer>().bounds.size.y / 3, stairsPrefab.GetComponent<MeshRenderer>().bounds.size.z / 2);
            stairsPrefab.transform.Rotate(new Vector3(0, 180, 0));
            stairsPrefab.transform.position = m_doorsPlaceHolders[_doorIndex].transform.position - bounds;

        }
        if (_type == ("left"))
        {
             bounds = new Vector3(stairsPrefab.GetComponent<MeshRenderer>().bounds.size.z / 2, -stairsPrefab.GetComponent<MeshRenderer>().bounds.size.y / 3, 0);
            stairsPrefab.transform.Rotate(new Vector3(0, 270, 0));
            stairsPrefab.transform.position = m_doorsPlaceHolders[_doorIndex].transform.position - bounds;

        }
        if (_type == ("right"))
        {
             bounds = new Vector3(stairsPrefab.GetComponent<MeshRenderer>().bounds.size.z / 2, stairsPrefab.GetComponent<MeshRenderer>().bounds.size.y / 3, 0);
            stairsPrefab.transform.Rotate(new Vector3(0, 90, 0));
            stairsPrefab.transform.position = m_doorsPlaceHolders[_doorIndex].transform.position + bounds;

        }




    }
}
