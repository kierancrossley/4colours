using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Riptide;

public class ColourUI : MonoBehaviour
{
    [SerializeField] private Button button; 
    [SerializeField] private TMP_Text colour; 
    private static Button _button; 
    private static TMP_Text _colour; 
    private static bool status = false; 

    private void Awake()
    {
        _button = button; // cannot set instance var to instance in initialization 
        _colour = colour;
    }

    public static void Available()
    {
        _button.GetComponent<Button>().interactable = true;
    }

    public static void Unavailable()
    {
        _button.GetComponent<Button>().interactable = false;
        status = false;
        _colour.color = new Color32(155, 64, 64, 255);
    }

    public void Toggle() // visible from the connect feature 
    {
        if(status == false)
        {
            status = true;
            colour.color = new Color32(72, 115, 64, 255);
        }
        else if(status == true)
        {
            status = false; 
            colour.color = new Color32(155, 64, 64, 255);
        }

        Message msg = Message.Create(MessageSendMode.Reliable, ClientToServer.RequestColourStatus); // request for colour status to update 
            msg.AddBool(status);
        EventController.OnSendMessage(msg);
    }
}
