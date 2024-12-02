using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour // inherits MonoBehaviour 
{
    public ushort id { get; private set; } // id property to make use of encapsulation 
    public string username { get; private set; } // username property 
    public bool ready { get; set; } // public set to be changed by message handlers 
    public bool colour { get; set; } // public set to be changed by message handlers 
    public bool canColour { get; set; }
    public List<int> cards = new List<int>();

    public void Init(ushort Id, string Username) // seperate constructor method to allow for an object
    {
        id = Id; 
        username = Username;
        ready = false;
        colour = false;
        canColour = true;
    }

    private void OnDestroy() // remove player if behaviour is destroyed 
    {
        PlayerController.RemovePlayer(id);
    }
}