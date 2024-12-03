using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField host;

    public void Connect()
    {
        EventController.OnConnectRequest(username.text, host.text);
    }
}