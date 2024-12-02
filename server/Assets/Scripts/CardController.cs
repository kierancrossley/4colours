using System.Collections.Generic;
using UnityEngine;
using Riptide;

public class CardController : MonoBehaviour
{
    public static List<Card> cardList = new List<Card>(); // all the cards available
    // pickup deck can just be a list of ids that link to cardList, no need to copy all data, shuffle ids into it
    public static List<int> pickupDeck = new List<int>(); // cards in the deck to be picked up 
    public static List<int> playedDeck = new List<int>(); // cards in the deck that have been played 
    public static int cardIdPlayed = -1; // cannot be 0 as 0 is a card id 
    public static string wildColour; // colour that has been set by a wild card 

    private void Awake()
    {
        ColourSet("R"); // red set
        ColourSet("Y"); // yellow set
        ColourSet("G"); // green set
        ColourSet("B"); // blue set

        pickupDeck.Clear(); // full of zeros due to being a set size (via unity editor)
        playedDeck.Clear();

        foreach(Card card in cardList)
        {
            pickupDeck.Add(card.id);
        }
    }

    private void Start()
    {
        ShuffleList(pickupDeck); // cannot have in awake, shuffles before cards added to pickup
    }

    private static void ShuffleList<T>(List<T> list) // Fisher-Yates algorithm
    {
        System.Random rng = new System.Random(); // differentiate from unity random

        int n = list.Count;
        while (n > 1) // iterate through the list in reverse order
        {
            n--;
            int k = rng.Next(n + 1); // generate a random index between 0 and n 
            // Swap the elements at indices k and n
            T value = list[k]; 
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void ColourSet(string colour)
    {
        cardList.Add(new Card(cardList.Count, "0", colour)); // 0 card
        
        for(int count = 1; count <= 2; count++) // x2 sets of 1-9 cards
        {
            for(int type = 1; type <= 9; type++)
            {
                cardList.Add(new Card(cardList.Count, type.ToString(),  colour));
            } 

            cardList.Add(new Card(cardList.Count, "MT", colour)); // miss turn card
            cardList.Add(new Card(cardList.Count, "CD", colour)); // change direction card
            cardList.Add(new Card(cardList.Count, "P2", colour)); // pickup 2 card    
        }

        cardList.Add(new Card(cardList.Count, "CC", "W")); // change colour card
        cardList.Add(new Card(cardList.Count, "P4", "W")); // pickup 4 card
    }

    public static void AddCard(ushort id){
        if(pickupDeck.Count > 0)
        {
            int cardId;
            cardId = pickupDeck[0]; // get 1st card from shuffled deck
            pickupDeck.Remove(cardId);
            playedDeck.Add(cardId);
            PlayerController.AddCard(id, cardId);
        }
        else // if no cards left, add cards back and reshuffle
        {
            pickupDeck = playedDeck;
            ShuffleList(pickupDeck);
            AddCard(id); // retry
        }
    }

    public static bool CanPlayCard(int requestId)
    {
        if((cardIdPlayed == -1 || // no card played yet, goes first as -1 is not an id
        (cardList[requestId].colour == cardList[cardIdPlayed].colour) || // same colours
        cardList[requestId].type == cardList[cardIdPlayed].type) || // same types
        cardList[requestId].colour == "W" || // wild can be played on any card 
        (cardList[cardIdPlayed].colour == "W" && // if wild card been played 
        cardList[requestId].colour == wildColour)) // and cardplayed colour is equal to the wild set 
        {
            return true;
        }

        return false; // no need for else as it would return true before 
    }

    private static void SendColourChange(ushort id, string colour)
    {
        Dictionary<ushort, Player> players = PlayerController.GetAllPlayers(); // access the static attribute via a method

        foreach(KeyValuePair<ushort, Player> ply in players)
        {
            if(id == ply.Key) // do not send to the client who sent it 
            { 
                continue;
            }

            Message msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendColourChange);
                msg.AddString(colour);
            EventController.Send(msg, ply.Key);
        }
    }

    [MessageHandler((ushort)ClientToServer.RequestColourChange)] // colour change request from client 
    private static void SetColourChange(ushort id, Message msg)
    {
        if(GameController.turnId == id) // no need to check if W card played as val only checked if W card played 
        {
            string colour = msg.GetString();
            wildColour = colour;
            SendColourChange(id, colour); // cannot send message in message handler
            GameController.NextTurn();
        }
    }
}