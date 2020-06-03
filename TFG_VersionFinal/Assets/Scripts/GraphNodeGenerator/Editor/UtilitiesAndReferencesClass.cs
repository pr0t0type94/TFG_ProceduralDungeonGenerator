using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class UtilitiesAndReferencesClass
{
    // Start is called before the first frame update
    public static UtilitiesAndReferencesClass Instance = null;

    public Dictionary<string, string> myConnectionsDictionary = new Dictionary<string, string>();

    public List<string> typesOfRoomsList = new List<string>();

    public UtilitiesAndReferencesClass ()
    {
        generateConnectionsNames();
        returnListOfRoomTypes();
    }     

    public static UtilitiesAndReferencesClass GetInstance()
    {
        return new UtilitiesAndReferencesClass { };
    }
    

   public void generateConnectionsNames()
    {
        //BASIC VALUES
        myConnectionsDictionary.Add("up","down");
        myConnectionsDictionary.Add("down", "up");
        myConnectionsDictionary.Add("left", "right");
        myConnectionsDictionary.Add("right", "left");

        //KEY VALUES
        myConnectionsDictionary.Add("up key", "down key");
        myConnectionsDictionary.Add("down key", "up key");
        myConnectionsDictionary.Add("left key", "right key");
        myConnectionsDictionary.Add("right key", "left key");

        //STAIRS VALUES
        myConnectionsDictionary.Add("up stairs", "down");
        myConnectionsDictionary.Add("down stairs", "up");
        myConnectionsDictionary.Add("left stairs", "right");
        myConnectionsDictionary.Add("right stairs", "left");


    }

    public string returnConnectionNameReferences(string _refName)
    {

        return myConnectionsDictionary.First(x => x.Key == _refName).Value;

    }


    public List<string> returnListOfRoomTypes()
    {
        typesOfRoomsList = new List<string>();

        List<GameObject> gameObjList = Resources.LoadAll<GameObject>("Prefabs/RoomPrefabs").ToList();
        //for (int i = 0; i < gameObjList.Count; i++)
        //{
        //    if (gameObjList[i].name != "Start" || gameObjList[i].name != "End")
        //        typesOfRoomsList.Add(gameObjList[i].name); 
        //}
        foreach(GameObject go in gameObjList)
        {
            if (go.name == "Start" || go.name == "End") continue;
            else typesOfRoomsList.Add(go.name);

        }

        return typesOfRoomsList;
    }
}
