using UnityEngine;
using Riptide;

public class CardPickupUI : MonoBehaviour
{
    // thought about adding validation to check whether they can play a card insead of picking up for their turn
    // but even UNO/other card games in real life rely on some trust from the player, plus the goal is no cards 

    public void PickupCard() // add card to deck instead of turn 
    {
        ColourUI.Unavailable(); // easier to just call than create loads of variables to check if 1 card 
        Message msg = Message.Create(MessageSendMode.Reliable, ClientToServer.RequestCardPickup); // request for card to be picked up 
        EventController.OnSendMessage(msg);
    }
}