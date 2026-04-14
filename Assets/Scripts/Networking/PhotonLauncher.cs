using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

namespace CyberStrike.Networking
{
    public class PhotonLauncher : MonoBehaviourPunCallbacks
    {
        [Header("UI References")]
        public GameObject loadingPanel;
        public GameObject mainMenuPanel;
        public TMP_Text statusText;

        [Header("Settings")]
        public string gameVersion = "1";

        private void Start()
        {
            loadingPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
            
            statusText.text = "Connecting to Server...";
            
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        public override void OnConnectedToMaster()
        {
            statusText.text = "Connected to Master Server!";
            PhotonNetwork.JoinLobby();
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public override void OnJoinedLobby()
        {
            statusText.text = "Joined Lobby!";
            loadingPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            loadingPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
            statusText.text = "Disconnected: " + cause.ToString();
            
            // Try reconnecting
            PhotonNetwork.ConnectUsingSettings();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
