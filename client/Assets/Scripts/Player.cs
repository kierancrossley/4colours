using UnityEngine;

public class Player : MonoBehaviour // inherits MonoBehaviour 
{
    public ushort id { get; private set; } // id property to make use of encapsulation 
    public string username { get; private set; } // username property 
    public bool isLocal { get; private set; } // local property
    public int handId { get; set; }

    public void Init(ushort Id, string Username, bool IsLocal) // constructor method 
    {
        id = Id; 
        username = Username;
        isLocal = IsLocal;
    }

    private void OnDestroy() // remove player if behaviour is destroyed 
    {
        PlayerController.RemovePlayer(id);
    }
}