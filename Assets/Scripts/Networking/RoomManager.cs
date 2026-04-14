using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace CyberStrike.Networking
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        [Header("UI Panels")]
        public GameObject lobbyPanel;
        public GameObject roomPanel;
        public GameObject createRoomPanel;

        [Header("Room Info")]
        public TMP_InputField roomNameInput;
        public TMP_InputField joinRoomInput;
        public TMP_Text roomTitleText;
        public Transform playerListContent;
        public GameObject playerEntryPrefab;

        [Header("Settings")]
        public string gameSceneName = "Game";
        public int maxPlayers = 8;

        public void CreateRoom()
        {
            if (string.IsNullOrEmpty(roomNameInput.text)) return;

            RoomOptions options = new RoomOptions { MaxPlayers = (byte)maxPlayers };
            PhotonNetwork.CreateRoom(roomNameInput.text, options);
        }

        public void JoinRoom()
        {
            if (string.IsNullOrEmpty(joinRoomInput.text)) return;
            PhotonNetwork.JoinRoom(joinRoomInput.text);
        }

        public void JoinRandomRoom()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnJoinedRoom()
        {
            lobbyPanel.SetActive(false);
            roomPanel.SetActive(true);
            roomTitleText.text = "Room: " + PhotonNetwork.CurrentRoom.Name;

            UpdatePlayerList();
        }

        public override void OnLeftRoom()
        {
            roomPanel.SetActive(false);
            lobbyPanel.SetActive(true);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdatePlayerList();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            UpdatePlayerList();
        }

        private void UpdatePlayerList()
        {
            // Clear existing entries
            foreach (Transform child in playerListContent)
            {
                Destroy(child.gameObject);
            }

            // Instantiate new entries
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                GameObject entry = Instantiate(playerEntryPrefab, playerListContent);
                entry.GetComponentInChildren<TMP_Text>().text = player.NickName + (player.IsLocal ? " (You)" : "");
            }
        }

        public void StartGame()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(gameSceneName);
            }
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }
    }
}
