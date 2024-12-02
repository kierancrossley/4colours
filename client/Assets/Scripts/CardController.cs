using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public static List<Card> cardList = new List<Card>(); // all the cards available
    public static int cardIdPlayed = -1; // cannot be 0 as it is a cardid 
    public static string wildColour; // colour that has been set by a wild card 

    public static Sprite MTSprite;
    public static Sprite CDSprite;
    public static Sprite P2Sprite;

    // saves creating inside of every cardui class 
    public static Dictionary<string, Color32> cardColours = new Dictionary<string, Color32>()
    {
        {"R", new Color32(155, 64, 64, 255)},
        {"Y", new Color32(115, 112, 64, 255)},
        {"G", new Color32(72, 115, 64, 255)},
        {"B", new Color32(64, 91, 115, 255)},
        {"W", new Color32(31, 31, 31, 255)}
    };

    private void Awake()
    {
        ColourSet("R"); // red set
        ColourSet("Y"); // yellow set
        ColourSet("G"); // green set
        ColourSet("B"); // blue set

        // unity caches, so cannot keep loading everytime, so cannot be in cardui as multiple instances exist 
        MTSprite = Resources.Load<Sprite>("MT");
        CDSprite = Resources.Load<Sprite>("CD");
        P2Sprite = Resources.Load<Sprite>("P2");
    }

    private void ColourSet(string colour)
    {
        cardList.Add(new Card(cardList.Count, "0", colour)); // 0 card
        
        for (int count = 1; count <= 2; count++) // x2 sets of 1-9 cards
        {
            for (int type = 1; type <= 9; type++)
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

    public static bool CanPlayCard(int requestId)
    {
        // can play same type, same colour or if -1 then there has been no card played yet 
        // -1 has to go first as -1 does not exist in cardList
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

    public static void SetCardIdPlayed(int cardId) // saves rewriting code in cardui and gamecontroller
    {
        cardIdPlayed = cardId;
        CardPlayedUI.UpdateCard(cardId);
    }
}