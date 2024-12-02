using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Riptide;
[System.Serializable]

public class CardUI : MonoBehaviour
{
    [SerializeField] private Button cardButton;
    [SerializeField] private Image background;
    [SerializeField] private GameObject allImages;
    [SerializeField] private Image leftImage;
    [SerializeField] private Image centerImage;
    [SerializeField] private Image rightImage;
    [SerializeField] private TMP_Text leftText;
    [SerializeField] private TMP_Text centerText;
    [SerializeField] private TMP_Text rightText;
    [SerializeField] private GameObject changeColour;
    [SerializeField] private GameObject cardBack;

    private bool hidden;
    private int id;
    private Sprite typeImage;
    private bool called = false;

    public void Init(bool Hidden, int Id = -1) // if hidden, no ID needed (constructor)
    { 
        hidden = Hidden;
        id = Id;

        DisplayCard();
    }
    
    private void SetAllImages(Sprite img)
    {
        leftImage.sprite = img;
        centerImage.sprite = img;
        rightImage.sprite = img;
    }

    private void ResetComponents() // had an issue where I wasn't resetting all components correctly 
    {
        leftText.SetText(""); // where card can be initialised more than once, need to reset text to none
        centerText.SetText("");
        rightText.SetText("");

        leftText.color = new Color32(28, 28, 28, 219);
        rightText.color = new Color32(28, 28, 28, 219);

        leftImage.color = new Color32(183, 183, 183, 183); // image has no method setactive
        rightImage.color = new Color32(183, 183, 183, 183); // so change alpha to 0
        allImages.SetActive(true);

        if((leftText.fontStyle & FontStyles.Underline) != 0) // reset underline for same reasons as resetting text 
        {
            leftText.fontStyle ^= FontStyles.Underline;
            rightText.fontStyle ^= FontStyles.Underline;
        }

        changeColour.SetActive(false);
    }

    public void SetBackgroundColour(string colour)
    {
        background.GetComponent<Image>().color = CardController.cardColours[colour];
    }

    private void DisplayCard()
    {
        if (hidden == true)
        {
            cardButton.GetComponent<Button>().interactable = false; // cannot be pressed 
            cardBack.SetActive(true); // show back of card
        }
        else
        {
            string type = CardController.cardList[id].type;
            string colour = CardController.cardList[id].colour;

            if(called) // if previously called then need to reset components 
            {
                ResetComponents();
            }

            SetBackgroundColour(colour);
            
            switch(type){
                case "MT":
                    SetAllImages(CardController.MTSprite);
                    break;
                case "CD":
                    SetAllImages(CardController.CDSprite);
                    break;
                case "P2": case "P4":
                    typeImage = CardController.P2Sprite; // use the same sprite for both
                    centerImage.sprite = typeImage;
                    leftImage.color = new Color32(255, 255, 255, 0); // image has no method setactive
                    rightImage.color = new Color32(255, 255, 255, 0); // so change alpha to 0
                    leftText.color = new Color32(183, 183, 183, 183);
                    rightText.color = new Color32(183, 183, 183, 183);
                    string pickupText = $"+{type.Substring(1)}"; // 2 or 4
                    leftText.SetText(pickupText);
                    rightText.SetText(pickupText);
                    break;
                case "CC":
                    allImages.SetActive(false);
                    changeColour.SetActive(true);
                    break;
                default:
                    allImages.SetActive(false);
                    leftText.SetText(type);
                    centerText.SetText(type);
                    rightText.SetText(type);
                    if(type == "6" || type == "9") // underline 6 and 9
                    {
                        leftText.fontStyle = FontStyles.Underline;
                        rightText.fontStyle = FontStyles.Underline;
                    } 
                    break;
            }
        }

        called = true;
    }

    public void PlayCard() // validate on both client and server, never trust client 
    {
        ushort localId = PlayerController.localId;
        if(GameController.turnId == localId)
        {
           if(CardController.CanPlayCard(id))
           {
            CardController.SetCardIdPlayed(id);
            
            Message msg = Message.Create(MessageSendMode.Reliable, ClientToServer.RequestCardPlay); // request for card to be played 
                msg.AddInt(id);
            EventController.OnSendMessage(msg);

            if(CardController.cardList[id].colour == "W") // open colour select for wild 
            {
                ColourSelectUI.Open();
            }

            int handId = PlayerController.GetHand(localId);
            GameObject[] hands = GameObject.FindGameObjectsWithTag("Hand"); // find the hand with id
            int cardCount = hands[handId - 1].transform.childCount;

            if(cardCount == 2) // have to include self, as count will be 2 as not destroyed yet 
            {
                ColourUI.Available();
            }
            else
            {
                ColourUI.Unavailable();
            }

            Destroy(gameObject); // final call as rest of code wouldn't execute 
           }
        }
    }
}