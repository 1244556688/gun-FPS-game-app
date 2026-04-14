using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviourPun
{
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("UI References")]
    public Slider healthBar;
    
    private bool isDead = false;
    private PlayerMovement movement;
    private WeaponController weapon;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponent<WeaponController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    [PunRPC]
    public void TakeDamage(float damage, int attackerActorNumber)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die(attackerActorNumber);
        }
    }

    void Die(int attackerActorNumber)
    {
        isDead = true;
        
        // Disable components locally and visuals everywhere
        SetPlayerState(false);

        if (photonView.IsMine)
        {
            // Notify ScoreManager about the kill
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.photonView.RPC("RPC_RegisterKill", RpcTarget.All, attackerActorNumber);
            }

            // Start respawn sequence
            StartCoroutine(RespawnTimer());
        }
    }

    IEnumerator RespawnTimer()
    {
        // UI feedback could be added here
        yield return new WaitForSeconds(3f);
        
        // Find a spawn point from GameManager
        Transform spawnPoint = GameManager.Instance.GetRandomSpawnPoint();
        transform.position = spawnPoint.position;
        
        photonView.RPC("RPC_Respawn", RpcTarget.All);
    }

    [PunRPC]
    void RPC_Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        SetPlayerState(true);
        UpdateUI();
    }

    private void SetPlayerState(bool active)
    {
        // Disable movement and weapons
        if (movement) movement.enabled = active;
        if (weapon) weapon.enabled = active;

        // Hide visuals
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers) r.enabled = active;
        
        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = active;
    }

    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }
}
