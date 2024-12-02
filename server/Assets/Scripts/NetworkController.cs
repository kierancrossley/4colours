using UnityEngine;
using System.Collections.Generic;
using Riptide;
using Riptide.Utils;
using System.Linq;

// enum gives each string a numerical value that is converted for simplicity 

public enum ServerToClient : ushort // server to client netmessages
{
    SendConnectedPlayers,
    SendNewPlayer,
    SendDisconnectedPlayer,
    SendAddedCard,
    SendAddedBlankCard,
    SendGameBegun,
    SendTurnId,
    SendCardPlayed,
    SendColourChange,
    SendWinner
}

public enum ClientToServer : ushort // client to server netmessages
{
    RequestConnectedPlayers,
    RequestStatusChange,
    RequestCardPlay,
    RequestColourChange,
    RequestCardPickup,
    RequestColourStatus,
}

public class NetworkController : MonoBehaviour
{
    public Server server { get; private set; } 

    private void Awake()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, true); // init riptide
    }

    private void Start()
    {
        server = new Server();
        server.Start(7777, 4); // port 7777 with maxplayers 4
        Subscribe(); 
    }

    private void OnDestroy() // unsubscribe from events on destroy
    {
        Unsubscribe();
    }

    private void FixedUpdate() // handles delayed envents that need to be invoked 
    {
        server.Update();
    }

    private void Subscribe() 
    {
        server.ClientDisconnected += Disconnect;
        server.HandleConnection += ConnectionHandler;
        EventController.SendMessageToPlayer += SendToPlayer;
        EventController.SendMessageToAll += SendToAll;
        EventController.StopServer += Stop;
    }

    private void Unsubscribe()
    {
        server.ClientDisconnected -= Disconnect;
        server.HandleConnection -= ConnectionHandler;
        EventController.SendMessageToPlayer -= SendToPlayer;
        EventController.SendMessageToAll -= SendToAll;
        EventController.StopServer -= Stop;
    }

    private void ConnectionHandler(Connection conn, Message message)
    {
        if(GameController.gameStarted) // if game started then refuse connection
        {
            server.Reject(conn);
            return;
        }

        server.Accept(conn);
    }

    private void Disconnect(object sender, ServerDisconnectedEventArgs e)
    {
        Dictionary<ushort, Player> players = PlayerController.GetAllPlayers(); // access the static attribute via a method
        ushort id = e.Client.Id;

        if(players.Count == 2)
        {
            players.Remove(id);
            GameController.SendWinner(players.Keys.Min()); // only player left 
        }
        else
        {
            foreach(int card in PlayerController.GetPlayer(id).cards)
            {
                CardController.playedDeck.Add(card); // return their cards 
            }

            Destroy(PlayerController.GetPlayer(id).gameObject); // destroy player on leave 
            
            if (GameController.turnId == id){
                GameController.NextTurn(); // next turn, if it was their turn
            }
        }
    }

    // NTS: https://medium.com/@javatechie/how-to-kill-the-process-currently-using-a-port-on-localhost-in-windows-31ccdea2a3ea
    private void Stop()
    {
        server.Stop();
    }

    private void SendToPlayer(Message msg, ushort id) // send message to player with specific id 
    {
        server.Send(msg, id);
    }

    private void SendToAll(Message msg) // send message to all players
    {
        server.SendToAll(msg);
    }
}