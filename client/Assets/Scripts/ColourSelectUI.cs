using UnityEngine;
using Riptide;

public class ColourSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject colourSelect;
    private static GameObject _colourSelect;

    void Awake()
    {
        _colourSelect = colourSelect; // same again, no instance=instance in initialization
        colourSelect.SetActive(false); 
    }

    public static void Open()
    {
        _colourSelect.SetActive(true);
    }

    public void ChangeColour(string colour)
    {
        Message msg = Message.Create(MessageSendMode.Reliable, ClientToServer.RequestColourChange); // request for colour change upon wild card call 
            msg.AddString(colour);
        EventController.OnSendMessage(msg);

        CardPlayedUI._cardPlayed.GetComponent<CardUI>().SetBackgroundColour(colour); // set playedcard background
        CardController.wildColour = colour;

        colourSelect.SetActive(false);
    }
}