using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;

    [Header("Settings")]
    public string playerPrefabName = "Player"; // Must be in Resources folder
    public Transform[] spawnPoints;
    
    [Header("UI")]
    public TMP_Text roomCodeText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
            
            if (roomCodeText != null)
                roomCodeText.text = "ROOM: " + PhotonNetwork.CurrentRoom.Name;
        }
    }

    public void SpawnPlayer()
    {
        Transform sp = GetRandomSpawnPoint();
        // Photon instantiation requires the prefab to be in a 'Resources' folder
        PhotonNetwork.Instantiate(playerPrefabName, sp.position, Quaternion.identity);
    }

    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned! Using origin.");
            return transform;
        }
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
