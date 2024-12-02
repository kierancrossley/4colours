using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Riptide;

public class GameController : MonoBehaviour
{
    public static ushort turnId = 0; 
    public static int playDirection = 1;
    public static bool missTurn = false;
    public static int cardDebt = 0;
    public static bool gameStarted = false;

    private void Start()
    {
        InvokeRepeating("CheckReadyStatus", 1.0f, 5.0f); // starting after 1 second, repeating every 5 seconds, time factor f
    }

    private void CheckReadyStatus()
    {
        Dictionary<ushort, Player> players = PlayerController.GetAllPlayers(); // access the static attribute via a method

        if(players.Count > 1) // can't play with 1 player 
        {
            bool allReady = true; // flag variable 
            foreach(KeyValuePair<ushort, Player> ply in players)
            {
                if(ply.Value.ready == false)
                { 
                    allReady = false; // flag that a player is not ready
                    return; // so no point continuing anything (had a break here originally) 
                }
            }
            if(allReady)
            {
                gameStarted = true;

                Message msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendGameBegun);
                EventController.Send(msg);

                CancelInvoke("CheckReadyStatus"); // cancel the ready status check as game has started 

                foreach(KeyValuePair<ushort, Player> ply in players)
                {
                    for(int count = 0; count <= 6; count++) // give starting 7 cards to each player
                    {
                        CardController.AddCard(ply.Key);
                    }
                }

                // had in next turn originally, checking if turnid==0 in if statement etc. (checking every call)
                turnId = (ushort)players.Keys.Min(); // client 1 may leave, player key now starts at 2 etc.
                SendNextTurn();
            }
        }
    }

    private static void SendNextTurn()
    {
        Message msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendTurnId);
            msg.AddUShort(turnId);
        EventController.Send(msg);
    }

    private static void CheckCardDebt() // check to see if cards need to be added 
    {
        if(cardDebt > 0)
        {
            for(int i = 0; i < cardDebt; i++) 
            {
                CardController.AddCard(turnId);
            }

            cardDebt = 0;
        }
    }

    public static void NextTurn()
    {
        Dictionary<ushort, Player> players = PlayerController.GetAllPlayers(); // access the static attribute via a method
        int maxKey = players.Keys.Max();
        int minKey = players.Keys.Min();
        bool flag = false; // flag variable 
        bool microFlag = false; // flag variable 
        int nextId;

        while(flag == false)
        {
            nextId = (int)turnId + playDirection; // turn+1 clockwise, turn+-1=turn-1 anticlockwise
            var ply = PlayerController.GetPlayer((ushort)nextId);
            if(ply)
            {
                turnId = (ushort)nextId;
                microFlag = true;
            }
            else if(nextId > maxKey && playDirection == 1) // max key means go back to min (clockwise)
            {
                turnId = (ushort)minKey; 
                microFlag = true;
            }
            else if(nextId < minKey && playDirection == -1) // and vise versa for anticlockwise 
            {
                turnId = (ushort)maxKey;
                microFlag = true;
            }

            if(microFlag) // saves repeating code in each if statement 
            {
                if(missTurn){
                    missTurn = false;
                    continue; // go to next player 
                }
                CheckCardDebt();
                flag = true;
            }
        }

        SendNextTurn();
    }

    public static void SendWinner(ushort id)
    {
        Message msg = Message.Create(MessageSendMode.Reliable, ServerToClient.SendWinner);
            msg.AddUShort(id);
        EventController.Send(msg);

        //EventController.Stop(); // you would restart server here 
    }

    // Messages

    [MessageHandler((ushort)ClientToServer.RequestStatusChange)] // ready status request netmessage from client 
    private static void ReceiveStatusChange(ushort id, Message msg)
    {
        bool status = msg.GetBool();
        Player ply = PlayerController.GetPlayer(id);
        ply.ready = status;
    } 

    [MessageHandler((ushort)ClientToServer.RequestCardPlay)] // play card netmessage from client 
    private static void ReceiveCardPlay(ushort id, Message msg)
    {
        if(turnId == id){
            int cardId = msg.GetInt();
            if(PlayerController.HasCard(id, cardId))
            {
                if(CardController.CanPlayCard(cardId))
                {
                    Player ply = PlayerController.GetPlayer(id);
                    int cardCount = ply.cards.Count;
                    CardController.cardIdPlayed = cardId;
                    PlayerController.RemoveCard(id, cardId);
                    Card card = CardController.cardList[cardId];

                    if(card.colour != "W") // nextturn is called upon colour change submital for wild cards
                    {
                        if(card.type == "MT")
                        {
                            missTurn = true;
                            NextTurn();
                        }
                        else if(card.type == "CD")
                        {
                            playDirection *= -1; // 1*-1 = clockwise to anticlockwise, -1*-1 = anticlockwise to clockwise

                            // no point storing as a varialbe in only used once 
                            if(PlayerController.GetAllPlayers().Count == 2)
                            {
                                SendNextTurn(); // do not change and send the turnid again 

                                if(cardCount == 1) // same reason as above (0 cards left)
                                {
                                    if(ply.colour) // check here too as returned out of
                                    {
                                        SendWinner(id);
                                    }
                                    else
                                    {
                                        ply.canColour = false;
                                        CardController.AddCard(id);
                                        CardController.AddCard(id);
                                    }
                                }

                                return;
                            }
                        }
                        else if(card.type == "P2")
                        {
                            cardDebt = 2;
                            NextTurn();
                        }
                        else
                        {
                            NextTurn();
                        }  
                    }

                    if(card.type == "P4")
                    {
                        cardDebt = 4;
                    }

                    if(cardCount == 2) // higher as counted before card removed (1 cards left)
                    {
                        ply.canColour = true;
                    }

                    if(cardCount == 1) // same reason as above (0 cards left)
                    {
                        if(ply.colour)
                        {
                            SendWinner(id);
                        }
                        else
                        {
                            ply.canColour = false;
                            CardController.AddCard(id);
                            CardController.AddCard(id);
                        }
                    }
                }
            }
        }
    } 

    [MessageHandler((ushort)ClientToServer.RequestCardPickup)] // request to pickup card netmessage from client 
    private static void ReceiveCardPickup(ushort id, Message msg)
    {
        if(turnId == id) // check its their turn to pickup a card 
        {
            CardController.AddCard(id); // give them a card
            NextTurn(); // don't let them play a card 
        }
    } 

    [MessageHandler((ushort)ClientToServer.RequestColourStatus)] // colour status request netmessage from client 
    private static void ReceiveColourStatus(ushort id, Message msg)
    {
        Player ply = PlayerController.GetPlayer(id);
        if(ply.canColour &&
        (turnId != id || 
        CardController.cardList[CardController.cardIdPlayed].type == "MT" ||
        CardController.cardList[CardController.cardIdPlayed].type == "CD") &&
        ply.cards.Count == 1)
        {
            bool status = msg.GetBool();
            ply.colour = status;
        }
    } 
}