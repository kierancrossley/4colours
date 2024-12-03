using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Riptide;
using Riptide.Utils;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using TMPro;

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
    SendWinner,
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
    public Client client { get; private set; }
    public static string localUsername;
    private static string ipFormat = @"^((25[0-5]|(2[0-4][0-9])|([01]?[0-9][0-9]?))\.){3}(25[0-5]|(2[0-4][0-9])|([01]?[0-9][0-9]?)):(\d{1,5})$";
    private static Regex ipRegex = new Regex(ipFormat);
    
    [SerializeField] private TMP_Text errorText;

    private void Awake()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, true); // init riptide
    }  

    private void Start()
    {
        client = new Client();
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void FixedUpdate()
    {
        client.Update();
    }

    private void Subscribe()
    {
        client.Connected += OnConnected;
        client.Disconnected += OnDisconnected;
        client.ConnectionFailed += ConnectionHandler;
        EventController.ConnectRequest += Connect;
        EventController.SendMessage += OnSendMessage;
        EventController.LeaveGame += Disconnect;
    }

    private void Unsubscribe()
    {
        client.Connected -= OnConnected;
        client.Disconnected -= OnDisconnected;
        client.ConnectionFailed -= ConnectionHandler;
        EventController.ConnectRequest -= Connect;
        EventController.SendMessage -= OnSendMessage;
        EventController.LeaveGame -= Disconnect;
    }

    private void ConnectionHandler(object sender, ConnectionFailedEventArgs e)
    {
        // default error message 
        string msg = "Unknown error!";
        RejectReason reason = e.Reason;

        // custom error messages 
        if(reason == RejectReason.Rejected)
        {
            msg = "Game has started";
        }
        else if(reason == RejectReason.ServerFull)
        {
            msg = "Server is full";
        }
        else if(reason == RejectReason.NoConnection)
        {
            msg = "Server is offline";
        }

        errorText.text = msg;
    }

    public void Connect(string username, string host)
    {
        if(ipRegex.IsMatch(host)) // riptide requires connection in ip:port format 
        {
            errorText.text = "Attempting connection...";
            localUsername = string.IsNullOrEmpty(username) ? $"Guest" : username;
            client.Connect(host);
        }
        else
        {
            errorText.text = "Invalid host address"; 
        }
    }

    private void Disconnect()
     {
        client.Disconnect();
     }

    private void OnSendMessage(Message msg)
    {
        client.Send(msg);
    }

    private void OnConnected(object sender, EventArgs e)
    {
        EventController.OnConnectSuccess(client.Id, localUsername);
    }

    private void OnDisconnected(object sender, EventArgs e)
    {
        SceneManager.LoadScene("Login"); // restart client
    }
}