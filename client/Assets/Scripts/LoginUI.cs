using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField username;

    public void Connect()
    {
        EventController.OnConnectRequest(username.text);
    }
}