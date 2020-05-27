using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ConectionsDictionary
{
    // Start is called before the first frame update
    public static ConectionsDictionary Instance = null;

    public Dictionary<string, string> myConnectionsDictionary = new Dictionary<string, string>();

    public List<string> typesOfRoomsList = new List<string>();

    public ConectionsDictionary ()
    {
        generateConnectionsNames();
    }     

    public static ConectionsDictionary GetInstance()
    {
        return new ConectionsDictionary { };
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

        ////STAIRS VALUES
        //myConnectionsDictionary.Add("upstairs", "down");
        //myConnectionsDictionary.Add("downstairs", "up");
        //myConnectionsDictionary.Add("leftstairs", "right");
        //myConnectionsDictionary.Add("rightstairs", "left");


    }



    public string returnConnectionNameReferences(string _refName)
    {

        return myConnectionsDictionary.First(x => x.Key == _refName).Value;

    }

}
