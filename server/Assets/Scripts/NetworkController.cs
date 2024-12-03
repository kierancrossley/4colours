using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Riptide;
using Riptide.Utils;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using System.Net.Http;
using System.Threading.Tasks;

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
    public static Server server { get; private set; }
    public static string loggedText { get; private set; } = "";

    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button beginButton;
    [SerializeField] private ScrollRect consoleScroll;
    [SerializeField] private TMP_Text logText;
    [SerializeField] private TMP_Text ipText;
    [SerializeField] private TMP_InputField portInput;

    private void Awake()
    {
        Application.logMessageReceived += LogHandler; // subscribe before the subscribe method to record start up in console
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, true); // init riptide
    }

    private async void Start()
    {
        string publicIP = await GetPublicIPAddress();
        if (!string.IsNullOrEmpty(publicIP))
        {
            ipText.text = publicIP;
        }
        else
        {
            ipText.text = "Unknown";
        }
    }

    private async Task<string> GetPublicIPAddress()
    {
        try
        {
            using (HttpClient httpClient = new HttpClient()) // get their public ip through a http request
            {
                HttpResponseMessage response = await httpClient.GetAsync("https://api.ipify.org?format=text");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
        catch (System.Exception ex)
        {
            RiptideLogger.Log(Riptide.Utils.LogType.Warning, "Network", $"Error fetching public IP: {ex.Message}");
            RiptideLogger.Log(Riptide.Utils.LogType.Info, "Network", "You can find your public ip address here: https://api.ipify.org?format=text");
            return null;
        }
    }

    public void StartServer()
    {

        if (string.IsNullOrWhiteSpace(portInput.text)) // check for empty or whitespace input
        {
            RiptideLogger.Log(Riptide.Utils.LogType.Warning, "Network", "No port entered, returning to default value.");
            portInput.text = "7777"; // set default port value
        }

        // Try parsing the input to a ushort
        if (ushort.TryParse(portInput.text, out ushort port))
        {
            server = new Server();
            server.Start(port, 4); // max players = 4
            Subscribe();

            startButton.interactable = false;
            restartButton.interactable = true;
            stopButton.interactable = true;
            beginButton.interactable = true;
            portInput.interactable = false;
        }
        else
        {
            RiptideLogger.Log(Riptide.Utils.LogType.Error, "Network", "Invalid port input - server could not start!");
        }
    }

    public void StopServer()
    {
        server.Stop();
        EventController.OnStopServer();
        Unsubscribe();

        startButton.interactable = true;
        restartButton.interactable = false;
        stopButton.interactable = false;
        beginButton.interactable = false;
        portInput.interactable = true;
    }

    public void RestartServer()
    {
        StopServer();
        StartServer();
    }

    private void OnDestroy() // unsubscribe from events on destroy
    {
        Unsubscribe();
        Application.logMessageReceived -= LogHandler;
    }

    private void FixedUpdate() // handles delayed envents that need to be invoked 
    {
        if (stopButton.interactable){
            server.Update();
        }
    }

    private void Subscribe() 
    {
        server.ClientDisconnected += Disconnect;
        server.HandleConnection += ConnectionHandler;
        EventController.SendMessageToPlayer += SendToPlayer;
        EventController.SendMessageToAll += SendToAll;
    }

    private void Unsubscribe()
    {
        server.ClientDisconnected -= Disconnect;
        server.HandleConnection -= ConnectionHandler;
        EventController.SendMessageToPlayer -= SendToPlayer;
        EventController.SendMessageToAll -= SendToAll;
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

    private void LogHandler(string logString, string stackTrace, UnityEngine.LogType type)
    {
        loggedText += logString + "\n";
        logText.SetText(loggedText);
        consoleScroll.verticalNormalizedPosition = 0; // scroll to bottom
    }

    private void Disconnect(object sender, ServerDisconnectedEventArgs e)
    {
        Dictionary<ushort, Player> players = PlayerController.GetAllPlayers(); // access the static attribute via a method
        ushort id = e.Client.Id;
        int plyCount = players.Count;
        bool started = GameController.gameStarted;

        if(plyCount == 1){
            Destroy(PlayerController.GetPlayer(id).gameObject);
        }
        else if(plyCount == 2)
        {
            Destroy(PlayerController.GetPlayer(id).gameObject); // destroy player on leave 
            if(started){
                GameController.SendWinner(players.Keys.Min()); // only player left
            } 
        }
        else
        {
            if(started){
                foreach(int card in PlayerController.GetPlayer(id).cards)
                {
                    CardController.playedDeck.Add(card); // return their cards 
                }
            }

            Destroy(PlayerController.GetPlayer(id).gameObject); // destroy player on leave 
            
            if((GameController.turnId == id) && (started)){
                GameController.NextTurn(); // next turn, if it was their turn
            }
        }
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