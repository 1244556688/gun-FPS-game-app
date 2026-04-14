using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviourPun
{
    public static ScoreManager Instance;

    public int killsToWin = 10;
    
    [Header("UI References")]
    public TMP_Text scoreDisplay;
    public GameObject winPanel;
    public TMP_Text winnerNameText;

    private Dictionary<int, int> playerKills = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        UpdateScoreUI();
        if (winPanel != null) winPanel.SetActive(false);
    }

    [PunRPC]
    public void RPC_RegisterKill(int attackerActorNumber)
    {
        if (!playerKills.ContainsKey(attackerActorNumber))
        {
            playerKills[attackerActorNumber] = 0;
        }

        playerKills[attackerActorNumber]++;
        UpdateScoreUI();

        // Check if anyone won
        if (playerKills[attackerActorNumber] >= killsToWin)
        {
            photonView.RPC("RPC_ShowWin", RpcTarget.All, attackerActorNumber);
        }
    }

    void UpdateScoreUI()
    {
        if (scoreDisplay == null) return;

        string text = "LEADERBOARD:\n";
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int kills = playerKills.ContainsKey(player.ActorNumber) ? playerKills[player.ActorNumber] : 0;
            text += $"{player.NickName}: {kills} Kills\n";
        }
        scoreDisplay.text = text;
    }

    [PunRPC]
    void RPC_ShowWin(int winnerActorNumber)
    {
        if (winPanel != null) winPanel.SetActive(true);
        
        string winnerName = "Unknown";
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == winnerActorNumber)
            {
                winnerName = p.NickName;
                break;
            }
        }

        if (winnerNameText != null) winnerNameText.text = winnerName + " WINS!";
        
        // Return to Menu after 5 seconds
        Invoke("ReturnToMenu", 5f);
    }

    void ReturnToMenu()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
