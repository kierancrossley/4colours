using UnityEngine.Events;
using Riptide;

public static class EventController
{
    public static event UnityAction<Message> SendMessageToAll; // send message to all event
    public static void Send(Message msg) => SendMessageToAll?.Invoke(msg); // when send is called, invoke all subscribers

    public static event UnityAction<Message, ushort> SendMessageToPlayer; // send message to specific player
    public static void Send(Message msg, ushort id) => SendMessageToPlayer?.Invoke(msg, id);

    public static event UnityAction StopServer; // stop the server
    public static void Stop() => StopServer?.Invoke();
}