using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class UtilitiesAndReferencesClass
{
    // Start is called before the first frame update
    public static UtilitiesAndReferencesClass Instance = null;

    public Dictionary<string, string> m_myConnectionsDictionary = new Dictionary<string, string>();

    public List<string> m_typesOfRoomsList = new List<string>();

    public UtilitiesAndReferencesClass ()
    {
        GenerateConnectionsNames();
        ReturnListOfRoomTypes();
    }     

    public static UtilitiesAndReferencesClass GetInstance()
    {
        return new UtilitiesAndReferencesClass { };
    }
    

   public void GenerateConnectionsNames()
    {
        //BASIC VALUES
        m_myConnectionsDictionary.Add("up","down");
        m_myConnectionsDictionary.Add("down", "up");
        m_myConnectionsDictionary.Add("left", "right");
        m_myConnectionsDictionary.Add("right", "left");

        //KEY VALUES
        m_myConnectionsDictionary.Add("up key", "down key");
        m_myConnectionsDictionary.Add("down key", "up key");
        m_myConnectionsDictionary.Add("left key", "right key");
        m_myConnectionsDictionary.Add("right key", "left key");

        //STAIRS VALUES
        m_myConnectionsDictionary.Add("up stairs", "down");
        m_myConnectionsDictionary.Add("down stairs", "up");
        m_myConnectionsDictionary.Add("left stairs", "right");
        m_myConnectionsDictionary.Add("right stairs", "left");


    }

    public string ReturnConnectionNameByReference(string _refName)
    {

        return m_myConnectionsDictionary.First(x => x.Key == _refName).Value;

    }


    public List<string> ReturnListOfRoomTypes()
    {
        m_typesOfRoomsList = new List<string>();

        List<GameObject> gameObjList = Resources.LoadAll<GameObject>("Prefabs/RoomPrefabs").ToList();
     
        foreach(GameObject go in gameObjList)
        {
            //no añadimos rooms start, floor o end
            if (go.name.Contains("Start") || go.name.Contains("End") || go.name.Contains("Floor")) continue;
            else m_typesOfRoomsList.Add(go.name);

        }

        return m_typesOfRoomsList;
    }
}
