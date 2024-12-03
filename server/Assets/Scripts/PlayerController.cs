using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

public class PlayerController : MonoBehaviour // inherits MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; 
    [SerializeField] private Transform playerList; 
    private static GameObject staticPlayerPrefab; // only statics can access statics :(
    private static Transform staticPlayerList; 
    [SerializeField] private static Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();

    private void Awake()
    {
        staticPlayerPrefab = playerPrefab; // cannot set an instance variables to eachother in initialization 
        staticPlayerList = playerList;
    }

    public static Player GetPlayer(ushort id)  // check if player exists in players
    {
        players.TryGetValue(id, out Player result);
        return result;
    }

    public static Dictionary<ushort, Player> GetAllPlayers() // get all in players 
    {
        return players;
    }

    public static void RemovePlayer(ushort id) // remove player from players if exists
    {
        if (players.TryGetValue(id, out Player result))
        {
            NetworkController.server.DisconnectClient(id);
            players.Remove(id);

            Message msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendDisconnectedPlayer);
                msg.AddUShort(id);
            EventController.Send(msg);
        }
    } 

    private static void AddPlayer(ushort id, string username, string ip)
    {
        Player player = Instantiate(staticPlayerPrefab, Vector3.zero, Quaternion.identity, staticPlayerList).GetComponent<Player>(); // create game object
        player.name = $"{id}"; // set display name of game object
        player.Init(id, username, ip);
        players.Add(id, player);
    }

    // Messages

    private static void SendConnectedPlayers(ushort id)
    {
        // reliable send mode to ensure the message is sent (TCP instead of UDP)
        Message msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendConnectedPlayers);
            int plyCount = players.Count;
            msg.AddInt(plyCount);
            
            // had a problem here where I forgot plycount is before added, I had set to 1 when should be 0
            if(plyCount != 0) // this is the only player, do not add again
            { 
                foreach (KeyValuePair<ushort, Player> ply in players)
                {
                    if(id == ply.Key) // do not resend the same player 
                    {
                        continue;
                    }

                    msg.AddUShort(ply.Key); // player id
                    msg.AddString(ply.Value.username); // player username
                }
            }

        EventController.Send(msg, id); // send netmessage to the player requesting login
    }

    private static void SendNewPlayer(ushort id, string username){
        foreach (KeyValuePair<ushort, Player> ply in players)
        {
            if(id == ply.Key) // do not send player player to the player
            { 
                continue;
            }

            Message msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendNewPlayer);
                msg.AddUShort(id);
                msg.AddString(username);
            EventController.Send(msg, ply.Key);
        }
    }

    public static void AddCard(ushort id, int cardId){
        players[id].cards.Add(cardId); // issue where I had inside of for loop, and card count was *playercount higher than should be

        // could have sent as 1 netmessage and sent either the actual cardid or -1 to correct players
        foreach (KeyValuePair<ushort, Player> ply in players){
            if(id == ply.Key) // send the cardid to the player so they can see the card they got 
            { 
                Message msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendAddedCard);
                    msg.AddInt(cardId);
                EventController.Send(msg, id); // send netmessage to id (quicker than accessing key)
                continue;
            }

            // cannot have 2 local scope variables with the same name 
            Message _msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendAddedBlankCard);
                _msg.AddUShort(id);
            EventController.Send(_msg, ply.Key); // send netmessage for blank cards to other players
        }
    }

    public static void RemoveCard(ushort id, int cardId){
        players[id].cards.Remove(cardId);

        foreach (KeyValuePair<ushort, Player> ply in players){
            if(id == ply.Key) // do not send to the client who sent it 
            { 
                continue;
            }

            // cannot call in a message handler anyway
            Message msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendCardPlayed);
                msg.AddUShort(id);
                msg.AddInt(cardId);
            EventController.Send(msg, ply.Key); // send netmessage to each player, other than the player joining
        }
    }

    public static bool HasCard(ushort id, int cardId) // check if player has a card 
    {
        return players[id].cards.Contains(cardId);
    }
    
    [MessageHandler((ushort)ClientToServer.RequestConnectedPlayers)] // login request netmessage from client 
    private static void InitializeNewPlayer(ushort id, Message msg)
    {
        string username = msg.GetString();
        string ip = "Unknown";

        if(NetworkController.server.TryGetClient(id, out Connection client)){
            ip = client.ToString();
        }

        SendConnectedPlayers(id); // send connected players if any 
        SendNewPlayer(id, username); // send new player to connected players if any 

        AddPlayer(id, username, ip); // add the player on the server 
        RiptideLogger.Log(Riptide.Utils.LogType.Info, "Player", $"Initialised client {id} as: {username}");
    }
}