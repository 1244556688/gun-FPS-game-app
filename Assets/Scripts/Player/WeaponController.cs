using UnityEngine;
using Photon.Pun;
using TMPro;

namespace CyberStrike.Player
{
    public class WeaponController : MonoBehaviourPun
    {
        [Header("Weapon Settings")]
        public float damage = 20f;
        public float fireRate = 0.1f;
        public int maxAmmo = 30;
        public float range = 100f;

        [Header("Visuals")]
        public Transform firePoint;
        public GameObject muzzleFlashPrefab; // Local effect
        public GameObject hitEffectPrefab;  // Local effect

        [Header("UI")]
        public TMP_Text ammoText;

        private float nextTimeToFire = 0f;
        private int currentAmmo;
        private bool isReloading = false;

        private void Start()
        {
            currentAmmo = maxAmmo;
            UpdateAmmoUI();
        }

        private void Update()
        {
            if (!photonView.IsMine) return;
            
            // Auto fire logic or manual button press
        }

        public void Shoot()
        {
            if (!photonView.IsMine || isReloading) return;

            if (currentAmmo <= 0)
            {
                Reload();
                return;
            }

            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + fireRate;
                currentAmmo--;
                UpdateAmmoUI();

                // 1. Local Effect
                Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);

                // 2. Raycast
                RaycastHit hit;
                if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
                {
                    Debug.Log("Hit: " + hit.transform.name);
                    
                    // Show hit effect
                    Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

                    // 3. Network Sync Damage
                    PlayerHealth victim = hit.transform.GetComponent<PlayerHealth>();
                    if (victim != null)
                    {
                        victim.photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage, photonView.Owner.ActorNumber);
                    }
                }
            }
        }

        public void Reload()
        {
            if (currentAmmo < maxAmmo && !isReloading)
            {
                isReloading = true;
                Invoke(nameof(FinishReload), 2f); // 2 second reload
            }
        }

        private void FinishReload()
        {
            currentAmmo = maxAmmo;
            isReloading = false;
            UpdateAmmoUI();
        }

        private void UpdateAmmoUI()
        {
            if (photonView.IsMine && ammoText != null)
            {
                ammoText.text = $"{currentAmmo} / {maxAmmo}";
            }
        }
    }
}
