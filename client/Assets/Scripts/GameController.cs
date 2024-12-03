using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Riptide;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    private static GameObject _cardPrefab;

    [SerializeField] private GameObject winScreen;
    [SerializeField] private TMP_Text winText;
    private static GameObject _winScreen;
    private static TMP_Text _winText;

    [SerializeField] private GameObject hand1; // thought about iteration, but for a few variables, is it worth it 
    [SerializeField] private GameObject hand2; // the postions and rotations are easier being defined in the inspector
    [SerializeField] private GameObject hand3; // although it seems a little messy
    [SerializeField] private GameObject hand4; // this is likely the best way to do this
    private HandUI _hand1; // cannot use an instance variable to initialize another instance variable
    private HandUI _hand2; // the compiler can rearrange these
    private HandUI _hand3; // no gaurantee that hand1 will load before _hand1 which would error 
    private HandUI _hand4; // so declare without dependancy on a variable and define inside the awake method

    public static ushort turnId = 0;

    private void Awake() // called when scene loaded as does not exist in runtime before then 
    {
        Subscribe();

        _cardPrefab = cardPrefab;
        _winScreen = winScreen;
        _winText = winText;
        _hand1 = hand1.GetComponent<HandUI>();
        _hand2 = hand2.GetComponent<HandUI>();
        _hand3 = hand3.GetComponent<HandUI>();
        _hand4 = hand4.GetComponent<HandUI>();

        AdjustHands();
    }

    private void OnDestroy() // unsubscribe to events when destroyed
    {
        Unsubscribe(); 
    }

    private void Subscribe()
    {
        EventController.StatusChange += RequestStatusChange; // call send status when ready button pressed
        EventController.PlayerUpdated += AdjustHands; // adjust hand positions when player leaves/joins
    }

    private void Unsubscribe()
    {
        EventController.StatusChange -= RequestStatusChange; 
        EventController.PlayerUpdated -= AdjustHands; 
    }

    private void AdjustHands(){
        // had issue where i was removing from static player dictionary here - i was not passing by ref/val correctly 
        Dictionary<ushort, Player> plys = new Dictionary<ushort, Player>(PlayerController.GetAllPlayers());
        int plyCount = plys.Count;

        ushort nextId = PlayerController.localId;
        PlayerController.SetHand(nextId, 1);
        _hand1.UpdateUsername(nextId);

        if(plyCount > 1)
        {
            plys.Remove(nextId); // only if needed 

            nextId = plys.Keys.Min();
            PlayerController.SetHand(nextId, 2);
            _hand2.UpdateUsername(nextId);

            if(plyCount > 2) // 3 or 4
            { 
                plys.Remove(nextId); // only call if needed

                hand3.SetActive(true);
                nextId = plys.Keys.Min();
                PlayerController.SetHand(nextId, 3);
                _hand3.UpdateUsername(nextId);

                if(plyCount == 4){
                    plys.Remove(nextId);

                    hand4.SetActive(true);
                    nextId = plys.Keys.Min();
                    PlayerController.SetHand(nextId, 4);
                    _hand4.UpdateUsername(nextId);
                }
                else
                {
                    hand4.SetActive(false); // hide if not in use 
                }
            }
            else
            {
                hand3.SetActive(false); // hide if not in use 
                hand4.SetActive(false);
            }
        }
    }

    private void RequestStatusChange(bool status) // send ready status to server
    {
        Message msg = Message.Create(MessageSendMode.Reliable, ClientToServer.RequestStatusChange);
            msg.AddBool(status);
        EventController.OnSendMessage(msg);
    }   

    private static void AddCard(ushort id, bool hidden, int cardId = -1)
    {
        Debug.Log("Adding card");
        int handId = PlayerController.GetHand(id);
        GameObject[] parentHand = GameObject.FindGameObjectsWithTag("Hand"); // find the hand with id
       
        // create the card gameobject with the hand as the parent
        CardUI card = Instantiate(_cardPrefab, Vector3.zero, Quaternion.identity, parentHand[handId - 1].transform).GetComponent<CardUI>();
        card.name = $"{id} {cardId}"; // playerid and cardid for identifying 
        card.Init(hidden, cardId); // use constructor method to set attributes 
    }

    public void MenuReturn()
    {
        EventController.OnLeaveGame();
    }  

    public void RejoinGame()
    {
        _winScreen.SetActive(false);
        EventController.OnRejoinGame();
    }

    // Messages

    [MessageHandler((ushort)ServerToClient.SendAddedBlankCard)] // receive blank card netmessage
    private static void AddSentBlankCard(Message msg)
    {
        ushort id = msg.GetUShort(); // player id
        AddCard(id, true); // add blank card to playerid
    }

    [MessageHandler((ushort)ServerToClient.SendAddedCard)] // receive card netmessage
    private static void AddSentCard(Message msg)
    {
        int cardId = msg.GetInt(); // player id
        AddCard(PlayerController.localId, false, cardId); // add card to local player
    }

    [MessageHandler((ushort)ServerToClient.SendTurnId)] // receive card netmessage
    private static void SetTurnId(Message msg)
    {
        turnId = msg.GetUShort(); // player id
        EventController.OnTurnChange();
    }

    [MessageHandler((ushort)ServerToClient.SendCardPlayed)] // receive card netmessage
    private static void ReceiveCardPlayed(Message msg)
    {
        ushort id = msg.GetUShort(); // player id
        int cardId = msg.GetInt(); // card id 

        GameObject card = GameObject.Find($"{id} {-1}"); // this will remove the first object it finds that matches, removes any blank card 
        Destroy(card);
        CardController.SetCardIdPlayed(cardId);
    }

    [MessageHandler((ushort)ServerToClient.SendWinner)] // receive winner netmessage
    private static void ReceiveWinner(Message msg)
    {
        ushort id = msg.GetUShort(); // player id
        _winScreen.SetActive(true);
        _winText.text = $"{PlayerController.GetPlayer(id).username} WINS";
    }
}