using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button _StartHostButton;
    [SerializeField] private Button _StartClientButton;

    private void Awake()
    {
        _StartHostButton.onClick.AddListener(()=>
        {
            Debug.Log("Starting Host");
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        _StartClientButton.onClick.AddListener(() =>
        {
            Debug.Log("Starting Client");
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
