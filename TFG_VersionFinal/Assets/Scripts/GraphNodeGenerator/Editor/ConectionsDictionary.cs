using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ConectionsDictionary
{
    // Start is called before the first frame update
    public static ConectionsDictionary Instance = null;

    public Dictionary<string, string> myConnectionsDictionary = new Dictionary<string, string>();

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
        myConnectionsDictionary.Add("up","down");
        myConnectionsDictionary.Add("down", "up");
        myConnectionsDictionary.Add("left", "right");
        myConnectionsDictionary.Add("right", "left");

        //KEY VALUES
        myConnectionsDictionary.Add("up+key", "down+key");
        myConnectionsDictionary.Add("down+key", "down+key");
        myConnectionsDictionary.Add("left+key", "right+key");
        myConnectionsDictionary.Add("right+key", "left+key");

        //STAIRS VALUE
        myConnectionsDictionary.Add("up+stairs", "down");
        myConnectionsDictionary.Add("down+stairs", "up");
        myConnectionsDictionary.Add("left+stairs", "right");
        myConnectionsDictionary.Add("right+stairs", "left");


    }



    public string returnConnectionNameReferences(string _refName)
    {

        return myConnectionsDictionary.First(x => x.Key == _refName).Value;

    }

}
