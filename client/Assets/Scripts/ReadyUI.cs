using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Riptide;

public class ReadyUI : MonoBehaviour
{
    [SerializeField] private Button button;
    private static Button _button;
    [SerializeField] private TMP_Text ready;
    private static TMP_Text _ready;
    private static bool status = false; 

    void Awake()
    {
        _button = button;
        _ready = ready;
        Subscribe();
    }

    private void Subscribe()
    {
        EventController.RejoinGame += Reset;
    }

    private void Unsubscribe()
    {
        EventController.RejoinGame -= Reset;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Reset()
    {
        button.GetComponent<Button>().interactable = true;
    }

    public static void Toggle() // visible from the connect feature 
    {
        if(status == false)
        {
            status = true;
            _ready.color = new Color32(72, 115, 64, 255);
        }
        else if(status == true)
        {
            status = false; 
            _ready.color = new Color32(155, 64, 64, 255);
        }

        EventController.OnStatusChange(status); // call event to send status change 
    }

    // Messages

    [MessageHandler((ushort)ServerToClient.SendGameBegun)] // receive game has begun net message from server 
    private static void ReceiveGameBegun(Message msg)
    {
        _button.GetComponent<Button>().interactable = false;
        Toggle();
    } 
}
