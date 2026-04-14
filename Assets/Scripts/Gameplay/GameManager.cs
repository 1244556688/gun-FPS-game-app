using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace CyberStrike.Gameplay
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;

        [Header("UI References")]
        public GameObject gameOverlayUI;
        public GameObject winPanel;
        public TMP_Text winText;
        public TMP_Text killStatsText;

        [Header("Settings")]
        public int killsToWin = 10;
        public string playerPrefabName = "Player"; // Must be in Resources folder
        public Transform[] spawnPoints;

        private Dictionary<int, int> playerKills = new Dictionary<int, int>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (PhotonNetwork.IsConnected)
            {
                SpawnPlayer();
            }
        }

        public void SpawnPlayer()
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation);
        }

        public void AddKill(int shooterActorNumber)
        {
            photonView.RPC("RPC_UpdateKillCount", RpcTarget.All, shooterActorNumber);
        }

        [PunRPC]
        private void RPC_UpdateKillCount(int actorNumber)
        {
            if (!playerKills.ContainsKey(actorNumber))
                playerKills[actorNumber] = 0;

            playerKills[actorNumber]++;
            
            UpdateKillUI();
            CheckWinCondition(actorNumber);
        }

        private void UpdateKillUI()
        {
            string stats = "LEADERBOARD:\n";
            foreach (var entry in playerKills)
            {
                Player p = PhotonNetwork.CurrentRoom.GetPlayer(entry.Key);
                stats += $"{p.NickName}: {entry.Value} Kills\n";
            }
            killStatsText.text = stats;
        }

        private void CheckWinCondition(int actorNumber)
        {
            if (playerKills[actorNumber] >= killsToWin)
            {
                Player winner = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
                ShowWinScreen(winner.NickName);
            }
        }

        private void ShowWinScreen(string winnerName)
        {
            winPanel.SetActive(true);
            winText.text = $"{winnerName} Wins!";
            
            // Return to lobby after delay
            Invoke(nameof(ReturnToLobby), 5f);
        }

        private void ReturnToLobby()
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("MainMenu");
        }
    }
}
