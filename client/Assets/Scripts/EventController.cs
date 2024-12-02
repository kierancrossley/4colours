using Riptide;
using UnityEngine.Events;

public static class EventController
{
    public static event UnityAction<Message> SendMessage; // when a message is sent to server 
    public static void OnSendMessage(Message msg) => SendMessage?.Invoke(msg);

    public static event UnityAction<string> ConnectRequest; // request to connect to server 
    public static void OnConnectRequest(string username) => ConnectRequest?.Invoke(username);
    
    public static event UnityAction<ushort, string> ConnectSuccess; // successfully connected ti server 
    public static void OnConnectSuccess(ushort id, string username) => ConnectSuccess?.Invoke(id, username);

    public static event UnityAction<bool> StatusChange; // on change of their ready status 
    public static void OnStatusChange(bool status) => StatusChange?.Invoke(status);

    public static event UnityAction PlayerUpdated; // on any player updated (conn/disconn)
    public static void OnPlayerUpdated() => PlayerUpdated?.Invoke();

    public static event UnityAction TurnChange; // on turn changes
    public static void OnTurnChange() => TurnChange?.Invoke();
}