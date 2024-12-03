using TMPro;
using UnityEngine;

public class HandUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private GameObject layout;
    private ushort id;

    private void Awake()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        EventController.TurnChange += SetUsernameColour; // using events saves checking in an update method 
        EventController.RejoinGame += ResetCards;
    }

    private void Unsubscribe()
    {
        EventController.TurnChange -= SetUsernameColour;
        EventController.RejoinGame -= ResetCards;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void SetUsernameColour()
    {
        if(GameController.turnId == id){ // if turn active set to red
            usernameText.color = CardController.cardColours["R"];
        }
        else // or set to white if inactive
        {
            usernameText.color = new Color32(255, 255, 255, 103);
        }
            
    }

    private void ResetCards()
    {
        foreach (Transform child in layout.transform) 
        {
            Destroy(child.gameObject); // destroy all cards
        }
    }
    
    public void UpdateUsername(ushort Id)
    {
        id = Id; // cache id for setusernamecolour

        if (PlayerController.GetPlayer(Id) == null)
        {
            return;
        }

        usernameText.SetText(PlayerController.GetPlayer(Id).username); // set hand username 
    }
}