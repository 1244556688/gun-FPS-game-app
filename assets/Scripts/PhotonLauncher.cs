using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    public static PhotonLauncher Instance;

    [Header("UI Panels")]
    public GameObject loadingPanel;
    public GameObject menuPanel;
    public GameObject lobbyPanel;

    [Header("UI Elements")]
    public TMP_InputField createRoomInput;
    public TMP_InputField joinRoomInput;
    public TMP_Text statusText;
    public Button createBtn;
    public Button joinBtn;

    private void Awake()
    {
        Instance = this;
        // Keep instances across scenes if needed, or just handle in MainMenu
    }

    private void Start()
    {
        Debug.Log("Connecting to Photon...");
        statusText.text = "Connecting to Server...";
        
        loadingPanel.SetActive(true);
        menuPanel.SetActive(false);
        lobbyPanel.SetActive(false);

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        statusText.text = "Joining Lobby...";
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        statusText.text = "Online";
        loadingPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        string roomCode = createRoomInput.text;
        if (string.IsNullOrEmpty(roomCode))
        {
            // Auto generate 6 digit code if empty
            roomCode = GenerateRandomCode(6);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 8;
        
        PhotonNetwork.CreateRoom(roomCode, roomOptions);
        statusText.text = "Creating Room: " + roomCode;
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(joinRoomInput.text)) return;

        PhotonNetwork.JoinRoom(joinRoomInput.text);
        statusText.text = "Joining Room: " + joinRoomInput.text;
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        statusText.text = "Joined Room: " + PhotonNetwork.CurrentRoom.Name;
        
        // In a real app, you might go to a Lobby scene first. 
        // For simplicity, we load the Game scene directly or show Lobby UI.
        menuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "Error: " + message;
        Debug.LogError("Room Creation Failed: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "Error: " + message;
        Debug.LogError("Join Room Failed: " + message);
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[length];
        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[Random.Range(0, chars.Length)];
        }
        return new string(stringChars);
    }
}
