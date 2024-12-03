using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Player : MonoBehaviour // inherits MonoBehaviour 
{
    public ushort id { get; private set; } // id property to make use of encapsulation 
    public string username { get; private set; } // username property 
    public bool ready { get; set; } // public set to be changed by message handlers 
    public bool colour { get; set; } // public set to be changed by message handlers 
    public bool canColour { get; set; }
    public List<int> cards = new List<int>();

    [SerializeField] private TMP_Text usernameText; 
    [SerializeField] private TMP_Text userdataText; 

    public void Init(ushort Id, string Username, string Ip) // seperate constructor method to allow for an object
    {
        id = Id; 
        username = Username;
        ready = false;
        colour = false;
        canColour = true;

        usernameText.text = Username;
        userdataText.text = $"{Id} @ {Ip}";

        EventController.StopServer += Kick;
    }

    public void Kick()
    {
        Destroy(gameObject); // destroy the GameObject the script is attached to
    }

    private void OnDestroy() // remove player if behaviour is destroyed 
    {
        EventController.StopServer -= Kick;
        PlayerController.RemovePlayer(id);
    }
}