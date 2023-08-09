using System.Collections;
using UnityEngine;
using TMPro;
using Mirror;

public class Menu : MonoBehaviour
{
    private MyNetworkManager Room { get => NetworkManager.singleton as MyNetworkManager; }

    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject attemptConnectErrorText;

    void Start()
    {
        addressInput.text = "localhost";
    }

    public void HostGame()
    {
        Room.StartHost();
    }

    public void ConfirmJoindGame()
    {
        Room.networkAddress = addressInput.text;
        Room.StartClient();
    }

    public IEnumerator TimeOutText()
    {
        attemptConnectErrorText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        attemptConnectErrorText.SetActive(false);
    }
}
