using TMPro;
using UnityEngine;

public class HandUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    private ushort id;

    private void Awake()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        EventController.TurnChange += SetUsernameColour; // using events saves checking in an update method 
    }

    private void Unsubscribe()
    {
        EventController.TurnChange -= SetUsernameColour;
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
    
    public void UpdateUsername(ushort Id)
    {
        id = Id; // cache id for setusernamecolour
        usernameText.SetText(PlayerController.GetPlayer(Id).username); // set hand username 
    }
}