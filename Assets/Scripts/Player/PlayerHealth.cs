using UnityEngine;
using Photon.Pun;
using CyberStrike.Gameplay;
using UnityEngine.UI;

namespace CyberStrike.Player
{
    public class PlayerHealth : MonoBehaviourPunCallbacks
    {
        [Header("Stats")]
        public float maxHealth = 100f;
        public float currentHealth;

        [Header("UI")]
        public Slider healthSlider; // Health bar

        private bool isDead = false;

        private void Start()
        {
            currentHealth = maxHealth;
            UpdateHealthUI();
        }

        [PunRPC]
        public void RPC_TakeDamage(float damage, int shooterActorNumber)
        {
            if (isDead) return;

            currentHealth -= damage;
            UpdateHealthUI();

            if (currentHealth <= 0)
            {
                Die(shooterActorNumber);
            }
        }

        private void Die(int shooterActorNumber)
        {
            isDead = true;

            // Notify GameManager of the kill
            if (PhotonNetwork.IsMasterClient)
            {
                GameManager.Instance.AddKill(shooterActorNumber);
            }

            if (photonView.IsMine)
            {
                // Disable visuals/movement
                photonView.RPC("RPC_ToggleVisibility", RpcTarget.All, false);
                
                // Respawn after 3 seconds
                Invoke(nameof(Respawn), 3f);
            }
        }

        private void Respawn()
        {
            isDead = false;
            currentHealth = maxHealth;
            UpdateHealthUI();

            // Reset position
            Transform spawnPoint = GameManager.Instance.spawnPoints[Random.Range(0, GameManager.Instance.spawnPoints.Length)];
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            photonView.RPC("RPC_ToggleVisibility", RpcTarget.All, true);
        }

        [PunRPC]
        private void RPC_ToggleVisibility(bool visible)
        {
            // Toggle mesh renderers or character model
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = visible;
            }
        }

        private void UpdateHealthUI()
        {
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth / maxHealth;
            }
        }
    }
}
