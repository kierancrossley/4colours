using UnityEngine;
using Riptide;

public class CardPlayedUI : MonoBehaviour
{
    [SerializeField] private GameObject cardPlayed;
    public static GameObject _cardPlayed;

    private void Awake()
    {
        _cardPlayed = cardPlayed; // can't set instance to instance in initialization 
        _cardPlayed.SetActive(false); // set invisible as no cards played yet 
    }
    
    public static void UpdateCard(int cardId) // saves running update each frame call (update method)
    {
        if(!_cardPlayed.activeSelf) // if not active, make active
        {
            _cardPlayed.SetActive(true);
        }

        _cardPlayed.GetComponent<CardUI>().Init(false, cardId); // redraw card 
    }

    [MessageHandler((ushort)ServerToClient.SendColourChange)] // receive the wild colour
    private static void SetColourChange(Message msg)
    {
        string colour = msg.GetString();
        _cardPlayed.GetComponent<CardUI>().SetBackgroundColour(colour); // the the card background
        CardController.wildColour = colour;
    }
}