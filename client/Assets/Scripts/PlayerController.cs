using System.Collections.Generic;
using UnityEngine;
using Riptide;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // PlayerController exits as one object, use static method to share data
    [SerializeField] private GameObject playerPrefab;
    private static GameObject _playerPrefab;
    public static Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();
    public static ushort localId; 

    private void Awake()
    {
        _playerPrefab = playerPrefab;
        Subscribe(); // subscribe to events when loaded
    }

    private void OnDestroy()
    {
        Unsubscribe(); // unsubscribe to events when destroyed
    }

    private void Subscribe()
    {
        EventController.ConnectSuccess += AddLocalPlayer; // call addplayer method when server connected
    }

    private void Unsubscribe() // if disconnected remove subscribers from calling
    {
        EventController.ConnectSuccess -= AddLocalPlayer; 
    }

    public static Player GetPlayer(ushort id)  // check if player exists in players
    {
        players.TryGetValue(id, out Player result);
        return result;
    }

    public static Dictionary<ushort, Player> GetAllPlayers() 
    {
        return players;
    }

    public static void SetHand(ushort id, int hand){
        if (players.ContainsKey(id))
        {
            players[id].handId = hand;
        }
    }

    public static int GetHand(ushort id){
        return players[id].handId;
    }

    public static void RemovePlayer(ushort id) // remove player from players if exists
    {
        if (players.TryGetValue(id, out Player result))
        {
            players.Remove(id);
            EventController.OnPlayerUpdated(); // adjust hand position 
        }
    } 

    private static void AddLocalPlayer(ushort id, string username)
    {
        localId = id;
        AddPlayer(id, username, true);

        Message msg = Message.Create(MessageSendMode.Reliable, ClientToServer.RequestConnectedPlayers); // request the other players now we are connected
            msg.AddString(username);
        EventController.OnSendMessage(msg);
    }

    private static void AddPlayer(ushort id, string username, bool local)
    {
        // create game object in scene using the player prefab 
        Player player = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
        player.name = $"{id}"; // set display name of game object
        player.Init(id, username, local); // use constructor method to set attributes 
        players.Add(id, player); // add player into players dictionary
    }

    // Messages

    [MessageHandler((ushort)ServerToClient.SendConnectedPlayers)] // connected players netmessage from server 
    private static void AddConnectedPlayers(Message msg)
    {
        int plyCount = msg.GetInt();
        if(plyCount != 0)
        {
            for(int count = 0; count < plyCount; count++)
            {
                ushort id = msg.GetUShort(); // player id
                string username = msg.GetString(); // player username
                AddPlayer(id, username, false);
            }
        }

        SceneManager.LoadScene("Game", LoadSceneMode.Additive); // addictive keeps controller game objects 
    }

    [MessageHandler((ushort)ServerToClient.SendNewPlayer)] // receive new client who has connected 
    private static void AddNewPlayer(Message msg)
    {
        ushort id = msg.GetUShort(); // player id
        string username = msg.GetString(); // player username

        AddPlayer(id, username, false);

        EventController.OnPlayerUpdated(); // adjust hand position when added, already adjusted from scene change
    }

    [MessageHandler((ushort)ServerToClient.SendDisconnectedPlayer)] // receive client who has disconnected 
    private static void RemoveDisconnectedPlayer(Message msg)
    {
        ushort id = msg.GetUShort(); // player id
        RemovePlayer(id); // onplayerupdated called within remove player method 
    }
}